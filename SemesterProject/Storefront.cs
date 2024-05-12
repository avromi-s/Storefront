using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Threading;

namespace SemesterProject
{
    public partial class Storefront : Form
    {
        // todo fix issue with loggedInCustomer reference not beign updated after purchase made with sp
        // - throws exception on attempted change to balance and displayed balance itself is unupdated after the purchase
        // todo fix currency display accross all displays
        // todo separate different sections of the GUI Store into classes within Storefront class for better organization, instead of just regions
        //      Store > Listings, Store > Cart, & Account > Balance, Account > Purchases
        // todo manage when less than 4 items in store > remove item triggers exception because iterating over all 4 listings, also, other 3 items should be disabled
        // todo username & likely also password not case sensitive
        // todo make balance label red or green based on positive or negative balance
        // todo update listing title + desc to nicer GUI and make uneditable, also move add to cart confirmation to either just visual cue, or to better area
        // todo add grand total of num orders, and sum of all purchases on account > purchases screen

        private DataClasses1DataContext db;
        private IEnumerator<STORE_ITEM> allStoreItems;
        private List<STORE_ITEM> cachedStoreItems = new List<STORE_ITEM>();
        private readonly CUSTOMER loggedInCustomer;
        private BindingList<CartItem> cartItems = new BindingList<CartItem>();
        private bool isAnotherItem { get; set; } // todo naming

        private readonly int NUM_LISTINGS_PER_PAGE = 4; // todo maybe derive from gui
        private int currentPageIndex = 0; // 0-indexed for easy use with collections

        private int currentPageNumDisplay => currentPageIndex + 1; // 1-indexed for user display

        public Storefront(DataClasses1DataContext db, CUSTOMER loggedInCustomer)
        {
            InitializeComponent();

            this.db = db;
            this.loggedInCustomer = loggedInCustomer;
        }

        #region Store

        // When a tab is directly selected, refresh it
        private void tc_Listings_Cart_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == tpListings)
            {
                RefreshListingsTab();
            }
            else if (e.TabPage == tpCart)
            {
                RefreshCartTab();
            }
        }

        // A tab may be immediately visible without being directly selected, so we refresh a tab when it becomes visible
        private void tc_Listings_Cart_VisibleChanged(object sender, EventArgs e)
        {
            if (tpListings.Visible)
            {
                RefreshListingsTab();
            }
            else if (tpCart.Visible)
            {
                RefreshCartTab();
            }
        }

        #region Listings

        #region ListingLoading

        private void RefreshListingsTab()
        {
            //RefreshAllStoreItems();
            LoadStoreItemsIntoGui(GetStoreItems(currentPageIndex));
            RefreshLblPageNum();
            if (isAnotherItem)
            {
                btnNextPage.Enabled = true;
            }
        }

        /// <summary>
        /// Go through each of the storeItems and populate each GUI listing with the item's details.
        /// </summary>
        /// <param name="storeItems">The items to populate the GUI listings with</param>
        private void LoadStoreItemsIntoGui(IEnumerable<STORE_ITEM> storeItems)
        {
            // current implementation of this method circles around and overwrites listings if more storeItems contains more than NUM_LISTINGS_PER_PAGE
            // todo should we circle around though?
            // also, at the very least, just put in the last 4 items, no need to overwrite the listings - it is wasted work
            // todo, just make this iterate NumItemsPerPage times, write those first amount of items, ignore others, or maybe throw exception?
            int i = 0;
            foreach (STORE_ITEM storeItem in storeItems)
            {
                StoreItemListing sil = new StoreItemListing(storeItem);
                Panel listing = pnlAllListings.Controls["pnlListing" + i] as Panel;
                GetTitleDescriptionTextBoxForListing(i).Text = sil.Title;
                listing.Controls["rtbPrice" + i].Text = sil.FormattedPrice;
                PictureBox pbx = (listing.Controls["pbxItemImage" + i] as PictureBox);
                pbx.Image = new Bitmap(sil.ItemImage, pbx.Size.Width, pbx.Size.Height);
                RefreshQuantityControlLimitsForListing(i);

                i = (i + 1) %
                    NUM_LISTINGS_PER_PAGE; // move to next listing to update, reset to the first listing (index 0) if we move past the last listing
            }
        }

        private RichTextBox GetTitleDescriptionTextBoxForListing(int listingIndex)
        {
            return pnlAllListings.Controls["pnlListing" + listingIndex]
                .Controls["rtbTitleDescription" + listingIndex] as RichTextBox;
        }

        private Label GetStatusInfoLabelForListing(int listingIndex)
        {
            return pnlAllListings.Controls["pnlListing" + listingIndex]
                .Controls["lblStatusInfo" + listingIndex] as Label;
        }

        /// <summary>
        /// Get the store items for the given page number. 
        /// </summary>
        /// <param name="pageNum">The page number to retrieve items for</param>
        /// <returns>An IEnumerable of the store items for the page</returns>
        private IEnumerable<STORE_ITEM> GetStoreItems(int pageNum)
        {
            EnsureStoreItemsRetrieved();

            // get any items from cache before retrieving from the db
            int i = 0;
            bool desiredItemIsCached = (pageNum * NUM_LISTINGS_PER_PAGE + i) < cachedStoreItems.Count;
            while (desiredItemIsCached && i < NUM_LISTINGS_PER_PAGE)
            {
                yield return cachedStoreItems[pageNum * NUM_LISTINGS_PER_PAGE + i];
                i++;
                desiredItemIsCached = (pageNum * NUM_LISTINGS_PER_PAGE + i) < cachedStoreItems.Count;
            }

            // retrieve the rest of the items from the db (unless we already retrieved all items
            // (i.e., i == NUM_LISTINGS_PER_PAGE), and/or until there are no more items left)
            for (; i < NUM_LISTINGS_PER_PAGE && isAnotherItem; i++)
            {
                STORE_ITEM storeItem = allStoreItems.Current;
                cachedStoreItems
                    .Add(storeItem); // cache item before returning in case another method tries to retrieve it already
                isAnotherItem =
                    allStoreItems
                        .MoveNext(); // also, update isAnotherItem before return so that it is accurate regardless of if this loop finishes (which is dependent on how much the calling method iterates)
                yield return storeItem;
            }
        }

        private void EnsureStoreItemsRetrieved()
        {
            if (allStoreItems == null)
            {
                RefreshAllStoreItems();
                isAnotherItem = allStoreItems.MoveNext();
            }
        }

        private void RefreshAllStoreItems(bool includeOutOfStock = false)
        {
            // The order of the store items retrieved here will be the order of the listings as displayed to the user
            // todo use a smarter ordering maybe? not just based on quantity
            allStoreItems = db.STORE_ITEMs.Where(item => includeOutOfStock || item.QuantityAvailable > 0)
                .OrderByDescending(item => item.QuantityAvailable)
                .GetEnumerator();
            // todo check if this is a safe use of enumerator - do we need to dispose of it manually?
        }

        #endregion

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            currentPageIndex++;
            RefreshListingsTab();

            btnPreviousPage.Enabled = true;
            bool IsNoMoreCachedItems =
                (currentPageIndex + 1) * NUM_LISTINGS_PER_PAGE >= cachedStoreItems.Count;
            if (IsNoMoreCachedItems && !isAnotherItem)
            {
                btnNextPage.Enabled = false;
            }
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            currentPageIndex--;
            RefreshListingsTab();

            btnNextPage.Enabled = true;
            if (currentPageIndex == 0)
            {
                btnPreviousPage.Enabled = false;
            }
        }

        private void RefreshLblPageNum()
        {
            lblPageNum.Text = "Page " + currentPageNumDisplay;
        }

        private void RefreshQuantityControlLimitsForListing(int listingIndex)
        {
            STORE_ITEM storeItem = cachedStoreItems[(currentPageIndex * NUM_LISTINGS_PER_PAGE) + listingIndex];
            int totalQtyAvail = storeItem.QuantityAvailable;
            int qtyInCart;
            if (cartItems.Any(item => item.GetStoreItem() == storeItem))
            {
                qtyInCart = cartItems.Where(item => item.GetStoreItem() == storeItem).First().Quantity;
            }
            else
            {
                qtyInCart = 0;
            }

            int remainingQty = totalQtyAvail - qtyInCart;

            NumericUpDown nudControl =
                (pnlAllListings.Controls["pnlListing" + listingIndex].Controls["nudQuantity" + listingIndex] as
                    NumericUpDown);
            nudControl.Maximum = remainingQty;
            nudControl.Value = remainingQty > 0 ? 1 : 0;
            // todo if remainingQty <= 0 disable listing or at least add to cart button?
        }

        #endregion

        #region Cart

        private void RefreshCartTab()
        {
            RefreshCartButtonsEnabledStatus();
            RefreshCartSummary();
            RefreshCartItemsViewControl();
        }

        private void RefreshCartItemsViewControl()
        {
            // this method needs to be called whenever the cart tab comes into view and whenever the cart items change while the user is on the cart page
            dgvCartItems.DataSource = cartItems.ToList();
            dgvCartItems.AutoResizeColumns();
            dgvCartItems.Update();
            dgvCartItems.Refresh();
        }

        private void btnAddToCart0_Click(object sender, EventArgs e)
        {
            DisplayAddToCartConfirmation((sender as Button), GetStatusInfoLabelForListing(0));

            CartItem cartItem = GetCartItemForListing(0);
            AddItemToCart(cartItem);
            RefreshQuantityControlLimitsForListing(0);
        }

        private void btnAddToCart1_Click(object sender, EventArgs e)
        {
            DisplayAddToCartConfirmation((sender as Button), GetStatusInfoLabelForListing(1));

            CartItem cartItem = GetCartItemForListing(1);
            AddItemToCart(cartItem);
            RefreshQuantityControlLimitsForListing(1);
        }

        private void btnAddToCart2_Click(object sender, EventArgs e)
        {
            DisplayAddToCartConfirmation((sender as Button), GetStatusInfoLabelForListing(2));

            CartItem cartItem = GetCartItemForListing(2);
            AddItemToCart(cartItem);
            RefreshQuantityControlLimitsForListing(2);
        }

        private void btnAddToCart3_Click(object sender, EventArgs e)
        {
            DisplayAddToCartConfirmation((sender as Button), GetStatusInfoLabelForListing(3));

            CartItem cartItem = GetCartItemForListing(3);
            AddItemToCart(cartItem);
            RefreshQuantityControlLimitsForListing(3);
        }

        private void AddItemToCart(CartItem cartItem)
        {
            if (cartItems.Any(item =>
                    item.GetStoreItem() ==
                    cartItem.GetStoreItem())) // todo use hashmap from StoreItemId -> CartItem for cartItems for faster lookup? now it is n for each search
            {
                cartItems.Where(item => item.GetStoreItem() == cartItem.GetStoreItem()).First().Quantity +=
                    cartItem.Quantity;
            }
            else
            {
                cartItems.Add(cartItem);
            }
        }

        private async void DisplayAddToCartConfirmation(Button btnAddToCart, Label lblStatusInfo)
        {
            // todo need to stop anything running here if next page is click in middle
            // todo also, if add to cart is clicked, multiple times, need to update timer so that the 5 seconds starts from the latest one

            lblStatusInfo.ForeColor = Color.Green;
            lblStatusInfo.Text = "Item added to cart";
            await Task.Run(() => Thread.Sleep(5000)); // clear text after 5 seconds
            lblStatusInfo.Text = "";
            lblStatusInfo.ForeColor = Color.Black;
        }

        private CartItem GetCartItemForListing(int listingIndex)
        {
            // todo this extracted to a separate method so that we can replace when we use a custom user control for each listing
            // instead of accessing everything based on their names and index like here
            int quantitySelected =
                Convert.ToInt32((pnlAllListings.Controls["pnlListing" + listingIndex]
                    .Controls["nudQuantity" + listingIndex] as NumericUpDown).Value);
            STORE_ITEM storeItem = cachedStoreItems[(currentPageIndex * NUM_LISTINGS_PER_PAGE) + listingIndex];
            return new CartItem(storeItem, quantitySelected);
        }

        // On purchase button click - purchase all items in cart and then empty the cart
        private void btnPurchaseCartItems_Click(object sender, EventArgs e)
        {
            if (cartItems.Count == 0) // shouldn't be possible because purchase button should be disabled
            {
                lblCartSummary.Text = "Please add items to cart before purchasing";
                return;
            }

            CreatePurchase(loggedInCustomer, cartItems.ToList());
            cartItems.Clear();

            lblCartSummary.Text = "Purchase completed";
            RefreshCartItemsViewControl();
            RefreshCartButtonsEnabledStatus();
            //RefreshAllStoreItems();
        }

        private void CreatePurchase(CUSTOMER loggedInCustomer, List<CartItem> cartItems)
        {
            var list = new List<object>();
            cartItems.ForEach(item => list.Add(new
            {
                StoreItemId = item.GetStoreItem().StoreItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }));
            string jsonString = JsonSerializer.Serialize(list);
            db.CREATE_NEW_PURCHASE(loggedInCustomer.CustomerId, jsonString);
        }

        private void btnRemoveItemFromCart_Click(object sender, EventArgs e)
        {
            // todo also allow update quantity?
            RemoveSelectedItemsFromCart(refreshQtyLimits: true);

            RefreshCartTab();
        }

        private void RefreshCartButtonsEnabledStatus()
        {
            // todo this should really be two separate methods maybe because doing 2 separate things (2 buttons)
            if (cartItems.Count > 0)
            {
                btnRemoveItemFromCart.Enabled = true;
                btnPurchaseCartItems.Enabled = true;
            }
            else
            {
                btnRemoveItemFromCart.Enabled = false;
                btnPurchaseCartItems.Enabled = false;
            }
        }

        private void RemoveSelectedItemsFromCart(bool refreshQtyLimits = true)
        {
            foreach (DataGridViewRow selectedItem in dgvCartItems.SelectedRows)
            {
                cartItems.Remove(selectedItem.DataBoundItem as CartItem);
            }

            if (refreshQtyLimits)
            {
                for (int i = 0; i < NUM_LISTINGS_PER_PAGE; i++)
                {
                    // todo this is a bit wasteful because we really only need to update the listing for the items that were removed, not all items. 
                    // move this into the foreach loop maybe and refresh the listings based on the items being removed
                    // OR, maybe only refresh listing qty limits on Listings tab enter?
                    RefreshQuantityControlLimitsForListing(i);
                }
            }
        }

        private void RefreshCartSummary()
        {
            lblCartSummary.Text =
                $"Total Quantity: {cartItems.Sum(item => item.Quantity)}\nPurchase Total: ${cartItems.Sum(item => item.Quantity * item.UnitPrice)}";
        }

        #endregion

        #endregion

        #region Account

        // When a tab is directly selected, refresh it
        private void tc_Balance_Purchases_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == tpPurchases)
            {
                RefreshPurchasesTab();
            }
            else if (e.TabPage == tpAccount)
            {
                RefreshBalanceTab();
            }
        }

        // A tab may be immediately visible without being directly selected, so we refresh a tab when it becomes visible
        private void tc_Balance_Purchases_VisibleChanged(object sender, EventArgs e)
        {
            if (tpPurchases.Visible)
            {
                RefreshPurchasesTab();
            }
            else if (tpAccount.Visible)
            {
                RefreshBalanceTab();
            }
        }

        #region Balance

        private void RefreshBalanceTab()
        {
            RefreshDisplayedBalance();
            // todo can delete if not being used, just created for symmetry/potential future use
        }

        private void btnPayToBalance_Click(object sender, EventArgs e)
        {
            // todo round nud value on validate. see: https://stackoverflow.com/questions/21811303/numericupdown-value-not-rounded-to-decimalplaces

            if (nudPayToBalance.Value <=
                0) // todo shouldn't be possible because button should remain disabled until valid amount entered
            {
                lblAccountBalanceResults.Text = $"Please enter a valid amount";
                lblAccountBalanceResults.ForeColor = Color.Red;
                return;
            }

            loggedInCustomer.Balance += nudPayToBalance.Value;
            db.SubmitChanges();

            RefreshDisplayedBalance();
            lblAccountBalanceResults.ForeColor = Color.Green;
            lblAccountBalanceResults.Text = $"${nudPayToBalance.Value} successfully paid to balance";
            nudPayToBalance.Value = 0;
        }

        private void RefreshDisplayedBalance()
        {
            lblCurrentBalance.Text = $"Current Balance: ${loggedInCustomer.Balance}";
        }

        #endregion

        #region Purchases

        private void RefreshPurchasesTab()
        {
            RefreshPurchasesViewControl();
        }

        private void RefreshPurchasesViewControl()
        {
            // todo implement filters
            dgvPastPurchases.DataSource = db.PURCHASEs.Where(p => p.CUSTOMER == loggedInCustomer)
                .Select(p => new
                {
                    Date = p.PurchaseDateTime, // todo format to date
                    OrderTotal = p.TotalPrice, // todo format as currency (with '$')
                    p.TotalQuantity,
                    Items = // get summary of items in purchase: // todo extract to method or ToString() method in PURCHASE_STORE_ITEMs class?
                        string.Join(", ",
                            p.PURCHASE_STORE_ITEMs.Select(item =>
                                $"({item.Quantity}) {item.STORE_ITEM.Manufacturer + " " + item.STORE_ITEM.ProductName} - ${item.UnitPrice * item.Quantity}"))
                })
                .ToList();
            dgvPastPurchases.AutoResizeColumns();
            dgvPastPurchases.Update();
            dgvPastPurchases.Refresh();
        }

        #endregion

        #endregion
    }
}