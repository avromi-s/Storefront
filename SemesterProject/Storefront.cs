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
using System.Windows.Forms.VisualStyles;
using SemesterProject.Properties;

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
        private readonly CUSTOMER loggedInCustomer;
        private BindingList<CartItem> cartItems = new BindingList<CartItem>();
        private const int NUM_LISTINGS_PER_PAGE = 4; // todo maybe derive from gui
        private int currentPageIndex = 0; // 0-indexed for easy use with collections
        private int currentPageNumDisplay => currentPageIndex + 1; // 1-indexed for user display
        private Listings listingsData;
        private ListingGui[] listingsGui = new ListingGui[NUM_LISTINGS_PER_PAGE];


        public Storefront(DataClasses1DataContext db, CUSTOMER loggedInCustomer)
        {
            InitializeComponent();

            this.db = db;
            this.loggedInCustomer = loggedInCustomer;
            this.listingsData = new Listings(db, loggedInCustomer, NUM_LISTINGS_PER_PAGE);
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

        #region Classes

        // This class is for providing a convenient wrapper around the listing GUI controls for easy retrieval of a listing's controls
        private class ListingGui
        {
            // todo names below - remove control type from variable name?
            public Panel ListingPanel { get; private set; }
            public PictureBox ItemImagePictureBox { get; private set; }
            public RichTextBox TitleDescriptionRichTextBox { get; private set; }
            public Label PriceLabel { get; private set; }
            public Button AddToCartButton { get; private set; }
            public NumericUpDown QuantityNumericUpDown { get; private set; }
            public Label StatusInfoLabel { get; private set; }
            public Label SelectQuantityLabel { get; private set; }
            public ListingData ListingData { get; private set; }

            public ListingGui(Panel AllListingsPanel, int listingIndex, ListingData listingData, bool disableListing = false)
            {
                ListingPanel = AllListingsPanel.Controls["pnlListing" + listingIndex] as Panel;
                ItemImagePictureBox = ListingPanel.Controls["pbxItemImage" + listingIndex] as PictureBox;
                TitleDescriptionRichTextBox =
                    ListingPanel.Controls["rtbTitleDescription" + listingIndex] as RichTextBox;
                PriceLabel = ListingPanel.Controls["lblPrice" + listingIndex] as Label;
                AddToCartButton = ListingPanel.Controls["btnAddToCart" + listingIndex] as Button;
                QuantityNumericUpDown = ListingPanel.Controls["nudQuantity" + listingIndex] as NumericUpDown;
                StatusInfoLabel = ListingPanel.Controls["lblStatusInfo" + listingIndex] as Label;
                SelectQuantityLabel = ListingPanel.Controls["lblSelectQuantity" + listingIndex] as Label;
                this.ListingData = listingData;

                if (disableListing)
                {
                    DisableListing();
                }
                else
                {
                    EnableListing();  // enable in case this listing was previously disabled on a previous page
                }
            }

            // Enable the current listing. 
            public void EnableListing()
            {
                ItemImagePictureBox.Image = null;
                ListingPanel.Enabled = true;
            }

            // Disable the current listing, such as when no item is being displayed in it 
            public void DisableListing()
            {
                ItemImagePictureBox.Image = new Bitmap(Resources.ImageNotFound, ItemImagePictureBox.Size.Width, ItemImagePictureBox.Size.Height);
                ListingPanel.Enabled = false;
                TitleDescriptionRichTextBox.Text = "";
                PriceLabel.Text = "";
            }

            public async void DisplayAddToCartConfirmation(int displayTimeLengthMs = 5000)
            {
                // todo need to stop anything running here if next page is click in middle
                // todo also, if add to cart is clicked, multiple times, need to update timer so that the 5 seconds starts from the latest one

                StatusInfoLabel.ForeColor = Color.Green;
                StatusInfoLabel.Text = "Item added to cart";
                await Task.Run(() => Thread.Sleep(displayTimeLengthMs)); // clear text after 5 seconds
                StatusInfoLabel.Text = "";
                StatusInfoLabel.ForeColor = Color.Black;
            }

            public void RefreshQuantityControlLimits(BindingList<CartItem> cartItems)
            {
                // todo this should use listinggui objects
                int qtyInCart = 0;
                if (cartItems.Any(item => item.GetStoreItem() == ListingData.StoreItem))
                {
                    qtyInCart = cartItems.First(item => item.GetStoreItem() == ListingData.StoreItem).Quantity;
                }

                int totalQtyAvail = ListingData.StoreItem.QuantityAvailable;
                int remainingQty = totalQtyAvail - qtyInCart;

                QuantityNumericUpDown.Maximum = remainingQty;
                QuantityNumericUpDown.Value = remainingQty > 0 ? 1 : 0;
                // todo if remainingQty <= 0 disable listing or at least add to cart button?
            }
        }

        #endregion

        #region ListingLoading

        private void RefreshListingsTab()
        {
            LoadStoreItemsIntoGui(listingsData.GetListingDataForPage(currentPageIndex));
            RefreshLblPageNum();
            if (listingsData.IsAnotherItem)
            {
                btnNextPage.Enabled = true;
            }
        }

        /// <summary>
        /// Go through each of the items and populate each GUI listing with the item's details.
        /// </summary>
        /// <param name="items">The items to populate the GUI listings with</param>
        private void LoadStoreItemsIntoGui(IEnumerable<ListingData> items)
        {
            // current implementation of this method circles around and overwrites listings if more items contains more than NUM_LISTINGS_PER_PAGE
            // todo should we circle around though?
            // also, at the very least, just put in the last 4 items, no need to overwrite the listings - it is wasted work
            // todo, just make this iterate NumItemsPerPage times, write those first amount of items, ignore others, or maybe throw exception?

            IEnumerator<ListingData> itemsEnumerator = items.GetEnumerator();
            for (int i = 0; i < NUM_LISTINGS_PER_PAGE; i++)
            {
                if (itemsEnumerator.MoveNext())
                {
                    listingsGui[i] = new ListingGui(pnlAllListings, i, itemsEnumerator.Current);
                    listingsGui[i].TitleDescriptionRichTextBox.Text = itemsEnumerator.Current.Title;
                    listingsGui[i].PriceLabel.Text = itemsEnumerator.Current.FormattedPrice;
                    listingsGui[i].ItemImagePictureBox.Image = new Bitmap(itemsEnumerator.Current.ItemImage,
                        listingsGui[i].ItemImagePictureBox.Size.Width, listingsGui[i].ItemImagePictureBox.Size.Height);
                    listingsGui[i].RefreshQuantityControlLimits(cartItems);
                }
                else
                {
                    listingsGui[i] = new ListingGui(pnlAllListings, i, null, true);
                }
            }
        }

        #endregion

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            currentPageIndex++;
            RefreshListingsTab();

            btnPreviousPage.Enabled = true;
            bool isOnLastPage = currentPageIndex >= listingsData.HighestPageIndexRetrieved;
            if (isOnLastPage && !listingsData.IsAnotherItem)
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
            CartItem cartItem = CreateCartItemForListing(0);
            AddItemToCart(cartItem);
            listingsGui[0].RefreshQuantityControlLimits(cartItems);
            listingsGui[0].DisplayAddToCartConfirmation();
        }

        private void btnAddToCart1_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(1);
            AddItemToCart(cartItem);
            listingsGui[1].RefreshQuantityControlLimits(cartItems);
            listingsGui[1].DisplayAddToCartConfirmation();
        }

        private void btnAddToCart2_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(2);
            AddItemToCart(cartItem);
            listingsGui[2].RefreshQuantityControlLimits(cartItems);
            listingsGui[2].DisplayAddToCartConfirmation();
        }

        private void btnAddToCart3_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(3);
            AddItemToCart(cartItem);
            listingsGui[3].RefreshQuantityControlLimits(cartItems);
            listingsGui[3].DisplayAddToCartConfirmation();
        }

        private void AddItemToCart(CartItem cartItem)
        {
            if (cartItems.Any(item =>
                    item.GetStoreItem() ==
                    cartItem.GetStoreItem())) // todo use hashmap from StoreItemId -> CartItem for cartItems for faster lookup? now it is n for each search
            {
                cartItems.First(item => item.GetStoreItem() == cartItem.GetStoreItem()).Quantity += cartItem.Quantity;
            }
            else
            {
                cartItems.Add(cartItem);
            }
        }


        private CartItem CreateCartItemForListing(int listingIndex)
        {
            // todo this extracted to a separate method so that we can replace when we use a custom user control for each listing
            // instead of accessing everything based on their names and index like here
            int quantitySelected = Convert.ToInt32(listingsGui[listingIndex].QuantityNumericUpDown.Value);
            STORE_ITEM storeItem = listingsData
                .GetListingData((currentPageIndex * NUM_LISTINGS_PER_PAGE) + listingIndex).StoreItem;
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
                    listingsGui[i].RefreshQuantityControlLimits(cartItems);
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