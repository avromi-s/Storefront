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

namespace SemesterProject
{
    public partial class Storefront : Form
    {
        // todo add db trigger or application function to update store_item quantity on purchase made
        // todo add db trigger to update customer balance on purchase made
        // maybe do application logic for above 2, or above 1 (for balance). This way it is clear when viewing code that it's getting updated and how.
        // todo fix currency display accross all displays
        // todo separate different sections of the GUI Store into classes within Storefront class for better organization, instead of just regions
        //      Store > Listings, Store > Cart, & Account > Balance, Account > Purchases
        // todo manage when less than 4 items in store > remove item triggers exception because iterating over all 4 listings, also, other 3 items should be disabled
        private DataClasses1DataContext db;
        private IEnumerator<STORE_ITEM> AllStoreItems;
        private List<STORE_ITEM> CachedStoreItems = new List<STORE_ITEM>();
        private readonly CUSTOMER LoggedInCustomer;
        private BindingList<CartItem> CartItems = new BindingList<CartItem>();
        private bool IsAnotherItem { get; set; } // todo naming

        private readonly int NumItemsPerPage = 4; // todo maybe derive from gui
        private int CurrentPageNum = 0; // 0-indexed for easy use with collections

        private int CurrentPageNumDisplay // 1-indexed for user display
        {
            get { return CurrentPageNum + 1; }
        }

        public Storefront(DataClasses1DataContext db, CUSTOMER loggedInCustomer)
        {
            InitializeComponent();

            this.db = db;
            this.LoggedInCustomer = loggedInCustomer;
        }

        #region Store

        #region Setup

        // On load, set up cart data source
        private void Storefront_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'storeDB_Purchases2.PURCHASE' table. You can move, or remove it, as needed.
            //this.pURCHASETableAdapter1.Fill(db.PURCHASEs.Where(row => row.CustomerId == LoggedInCustomerId);

            RefreshCartItemsViewControl();
            RefreshPastPurchasesViewControl();
            RefreshDisplayedBalance();
            LoadStoreItemsIntoGui(GetStoreItems(CurrentPageNum));
            if (IsAnotherItem)
            {
                btnNextPage.Enabled = true;
            }
        }

        private void RefreshCartItemsViewControl()
        {
            // todo below not showing actual cart items, and doesn't show even the headers unless using ToList(). likely because DataSource type needs to be a list and once we use ToList()
            // then that list is never updated when underlying item is
            // this is likely an issue for past purchase view as well, it likely doesn't show new purchases made except for ones already in the db on gui load
            // solution might be to have a RefreshCartItems method that reruns the below select query with a ToList() call and then calls dgvCartItems.Update() and .Refresh().
            // then, we'll call the RefreshCartItems method whenever cart needs to be updated (?). or maybe on binding complete?
            // (this can maybe also be instead of this bind method)
            dgvCartItems.DataSource = CartItems.Select(item => new
                {
                    item.Manufacturer,
                    item.ProductName,
                    item.UnitPrice,
                    Quantity = item.QuantitySelected,
                    Price = item.UnitPrice * item.QuantitySelected
                })
                .ToList();
        }

        private void RefreshPastPurchasesViewControl()
        {
            // todo implement filters, refresh purchases on purchase made in GUI
            // todo get store items for each purchase and display? or only total and total quantity?
            dgvPastPurchases.DataSource = db.PURCHASEs.Where(p => p.CUSTOMER == LoggedInCustomer)
                .Select(p => new
                {
                    Date = p.PurchaseDateTime, // todo format to date
                    OrderTotal = p.TotalPrice, // todo format as currency (with '$')
                    p.TotalQuantity,
                    Items = // get summary of items in purchase: // todo extract to method or ToString() method in PURCHASE_STORE_ITEMs class?
                        string.Join(", ",
                            p.PURCHASE_STORE_ITEMs.Select(item =>
                                $"({item.Quantity}) {item.STORE_ITEM.Manufacturer + " " + item.STORE_ITEM.ProductName} - {item.UnitPrice * item.Quantity}"))
                })
                .ToList();
            dgvPastPurchases.AutoResizeColumns();
            dgvPastPurchases.Update();
            dgvPastPurchases.Refresh();
        }

        #endregion

        #region ListingLoading

        /// <summary>
        /// Go through each of the storeItems and populate each GUI listing with the item's details.
        /// </summary>
        /// <param name="storeItems">The items to populate the GUI listings with</param>
        private void LoadStoreItemsIntoGui(IEnumerable<STORE_ITEM> storeItems)
        {
            // current implementation of this method circles around and overwrites listings if more storeItems contains more than NumItemsPerPage
            // todo should we circle around though?
            int i = 0;
            foreach (STORE_ITEM storeItem in storeItems)
            {
                StoreItemListing sil = new StoreItemListing(storeItem);
                Panel listing = pnlAllListings.Controls["pnlListing" + i] as Panel;
                // ((PictureBox) listing.Controls["pbxItemImage" + i]) todo set
                listing.Controls["rtbMainItemInfo" + i].Text = sil.Title;
                listing.Controls["rtbMinorItemInfo" + i].Text = sil.FormattedPrice;
                RefreshQuantityControlLimitsForListing(i);

                i = (i + 1) %
                    NumItemsPerPage; // move to next listing to update, reset to the first listing (index 0) if we move past the last listing
            }
        }

        /// <summary>
        /// Get the store items for the given page number. 
        /// </summary>
        /// <param name="pageNum">The page number to retrieve items for</param>
        /// <returns>An IEnumerable of the store items for the page</returns>
        private IEnumerable<STORE_ITEM> GetStoreItems(int pageNum)
        {
            if (AllStoreItems == null)
            {
                RetrieveAllStoreItems();
                IsAnotherItem = AllStoreItems.MoveNext();
            }

            // get any items from cache before retrieving from the db
            int i = 0;
            bool desiredItemIsCached = (pageNum * NumItemsPerPage + i) < CachedStoreItems.Count;
            while (desiredItemIsCached && i < NumItemsPerPage)
            {
                yield return CachedStoreItems[pageNum * NumItemsPerPage + i];
                i++;
                desiredItemIsCached = (pageNum * NumItemsPerPage + i) < CachedStoreItems.Count;
            }

            // retrieve the rest of the items from the db (unless we already retrieved all items
            // (i.e., i == NumItemsPerPage), and/or until there are no more items left)
            for (; i < NumItemsPerPage && IsAnotherItem; i++)
            {
                STORE_ITEM storeItem = AllStoreItems.Current;
                CachedStoreItems
                    .Add(storeItem); // cache item before returning in case another method tries to retrieve it already
                IsAnotherItem =
                    AllStoreItems
                        .MoveNext(); // also, update IsAnotherItem before return so that it is accurate regardless of if this loop finishes (which is dependent on how much the calling method iterates)
                yield return storeItem;
            }
        }

        private void RetrieveAllStoreItems()
        {
            // The order retrieved here will be the order of the items as displayed to the user
            // todo use a smarter ordering maybe? not just based on quantity
            AllStoreItems = db.STORE_ITEMs.OrderByDescending(item => item.QuantityAvailable).GetEnumerator();
        }

        #endregion

        #region ListingManagement

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum++;
            RefreshLblPageNum();
            LoadStoreItemsIntoGui(GetStoreItems(CurrentPageNum));

            btnPreviousPage.Enabled = true;
            bool IsNoMoreCachedItems = CurrentPageNum * NumItemsPerPage >= CachedStoreItems.Count / NumItemsPerPage;
            if (IsNoMoreCachedItems && !IsAnotherItem)
            {
                btnNextPage.Enabled = false;
            }
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum--;
            RefreshLblPageNum();
            LoadStoreItemsIntoGui(GetStoreItems(CurrentPageNum));

            btnNextPage.Enabled = true;
            if (CurrentPageNum == 0)
            {
                btnPreviousPage.Enabled = false;
            }
        }

        private void RefreshLblPageNum()
        {
            lblPageNum.Text = "Page " + CurrentPageNumDisplay;
        }

        private RichTextBox GetRichTextBoxForListing(int listingIndex)
        {
            // todo this can be updated to be a checkbox or something non-text that updates on add to cart
            return pnlAllListings.Controls["pnlListing" + listingIndex]
                .Controls["rtbMainItemInfo" + listingIndex] as RichTextBox;
        }

        private void RefreshQuantityControlLimitsForListing(int listingIndex)
        {
            STORE_ITEM storeItem = CachedStoreItems[(CurrentPageNum * NumItemsPerPage) + listingIndex];
            int totalQtyAvail = storeItem.QuantityAvailable;
            int qtyInCart;
            if (CartItems.Any(item => item.StoreItem == storeItem))
            {
                qtyInCart = CartItems.Where(item => item.StoreItem == storeItem).First().QuantitySelected;
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

        #region CartManagement

        private void AddItemToCart(CartItem cartItem, Control controlToUpdate)
        {
            if (CartItems.Any(item =>
                    item.StoreItem ==
                    cartItem.StoreItem)) // todo use hashmap from StoreItemId -> CartItem for CartItems for faster lookup? now it is n for each search
            {
                CartItems.Where(item => item.StoreItem == cartItem.StoreItem).First().QuantitySelected +=
                    cartItem.QuantitySelected;
            }
            else
            {
                CartItems.Add(cartItem);
            }

            dgvCartItems.Update();
            dgvCartItems.Refresh();
            controlToUpdate.Text +=
                "\nItem added to cart"; // todo this can be updated to be a checkbox or something non-text that updates on add to cart
        }

        private CartItem GetCartItemForListing(int listingIndex)
        {
            // todo this extracted to a separate method so that we can replace when we use a custom user control for each listing
            // instead of accessing everything based on their names and index like here
            int quantitySelected =
                Convert.ToInt32((pnlAllListings.Controls["pnlListing" + listingIndex]
                    .Controls["nudQuantity" + listingIndex] as NumericUpDown).Value);
            STORE_ITEM storeItem = CachedStoreItems[(CurrentPageNum * NumItemsPerPage) + listingIndex];
            return new CartItem(storeItem, quantitySelected);
        }

        private void btnAddToCart0_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(0);
            RichTextBox listingrtb = GetRichTextBoxForListing(0);
            AddItemToCart(cartItem, listingrtb);
            RefreshQuantityControlLimitsForListing(0);
        }

        private void btnAddToCart1_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(1);
            RichTextBox listingrtb = GetRichTextBoxForListing(1);
            AddItemToCart(cartItem, listingrtb);
            RefreshQuantityControlLimitsForListing(1);
        }

        private void btnAddToCart2_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(2);
            RichTextBox listingrtb = GetRichTextBoxForListing(2);
            AddItemToCart(cartItem, listingrtb);
            RefreshQuantityControlLimitsForListing(2);
        }

        private void btnAddToCart3_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(3);
            RichTextBox listingrtb = GetRichTextBoxForListing(3);
            AddItemToCart(cartItem, listingrtb);
            RefreshQuantityControlLimitsForListing(3);
        }

        // On purchase button click - purchase all items in cart and then empty the cart
        private void btnPurchaseCartItems_Click(object sender, EventArgs e)
        {
            if (CartItems.Count == 0) // shouldn't be possible because purchase button should be disabled
            {
                lblCartSummary.Text = "Please add items to cart before purchasing";
                return;
            }

            // Create new purchase and linked purchase_store_items and insert into db  (// todo extract to separate method for modularity, readability)
            PURCHASE purchase = new PURCHASE()
            {
                CUSTOMER = LoggedInCustomer,
                TotalQuantity = CartItems.Sum(item => item.QuantitySelected),
                TotalPrice = CartItems.Sum(item => item.UnitPrice * item.QuantitySelected),
                PurchaseDateTime = DateTime.Now // todo use db datetime?
            };
            db.PURCHASEs.InsertOnSubmit(purchase);

            CartItems.Select(item => new PURCHASE_STORE_ITEM()
                {
                    PURCHASE = purchase,
                    STORE_ITEM = item.StoreItem,
                    Quantity = item.QuantitySelected,
                    UnitPrice = item.UnitPrice
                })
                .ToList()
                .ForEach(item => db.PURCHASE_STORE_ITEMs.InsertOnSubmit(item));
            db.SubmitChanges();

            CartItems.Clear();
            lblCartSummary.Text = "Purchase completed";
            RefreshCartButtonsEnabledStatus();
        }

        private void btnRemoveItemFromCart_Click(object sender, EventArgs e)
        {
            // todo also allow update quantity?
            RemoveSelectedItemsFromCart(refreshQtyLimits: true);

            RefreshCartButtonsEnabledStatus();
            RefreshCartSummary();
        }

        private void tpCart_Enter(object sender, EventArgs e)
        {
            RefreshCartButtonsEnabledStatus();
            RefreshCartSummary();
        }

        private void RefreshCartButtonsEnabledStatus()
        {
            // todo this should really be two separate methods maybe because doing 2 separate things (2 buttons)
            if (CartItems.Count > 0)
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

        private void RefreshCartSummary()
        {
            lblCartSummary.Text =
                $"Total Quantity: {CartItems.Sum(item => item.QuantitySelected)}\nPurchase Total: ${CartItems.Sum(item => item.QuantitySelected * item.UnitPrice)}";
        }

        private void RemoveSelectedItemsFromCart(bool refreshQtyLimits = true)
        {
            foreach (DataGridViewRow selectedItem in dgvCartItems.SelectedRows)
            {
                CartItems.Remove(selectedItem.DataBoundItem as CartItem);
            }

            if (refreshQtyLimits)
            {
                for (int i = 0; i < NumItemsPerPage; i++)
                {
                    // todo this is a bit wasteful because we really only need to update the listing for the items that were removed, not all items. 
                    // move this into the foreach loop maybe and refresh the listings based on the items being removed
                    // OR, maybe only refresh listing qty limits on Listings tab enter?
                    RefreshQuantityControlLimitsForListing(i);
                }
            }
        }

        #endregion

        #endregion

        #region Account

        #region Balance

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

            LoggedInCustomer.Balance += nudPayToBalance.Value;
            db.SubmitChanges();

            RefreshDisplayedBalance();
            lblAccountBalanceResults.ForeColor = Color.Green;
            lblAccountBalanceResults.Text = $"${nudPayToBalance.Value} successfully paid to balance";
            nudPayToBalance.Value = 0;
        }

        private void RefreshDisplayedBalance()
        {
            lblCurrentBalance.Text = $"Current Balance: ${LoggedInCustomer.Balance}";
        }


        #endregion

        #region Purchases

        // Purchases tab may be immediately visible without being directly selected, so we refresh the purchases view when it becomes visible
        private void tc_Balance_Purchases_VisibleChanged(object sender, EventArgs e)
        {
            if (tpPurchases.Visible)
            {
                RefreshPastPurchasesViewControl();
            }
        }

        // When purchases tab is directly selected, refresh purchases view
        private void tc_Balance_Purchases_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == tpPurchases)
            {
                RefreshPastPurchasesViewControl();
            }
        }

        #endregion

        #endregion
    }
}