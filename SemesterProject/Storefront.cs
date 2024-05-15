using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
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
    public partial class Storefront : Form, IDisposable
    {
        private DataClasses1DataContext db;
        private readonly CUSTOMER loggedInCustomer;
        private BindingList<CartItem> cartItems = new BindingList<CartItem>();
        private readonly int NUM_LISTINGS_PER_PAGE;
        private int currentPageIndex = 0; // 0-indexed for easy use with collections
        private int currentPageNumDisplay => currentPageIndex + 1; // 1-indexed for user display
        private Listings listingsData;
        private ListingGui[] listingsGui;


        public Storefront(DataClasses1DataContext db, CUSTOMER loggedInCustomer)
        {
            InitializeComponent();

            this.db = db;
            this.loggedInCustomer = loggedInCustomer;
            this.NUM_LISTINGS_PER_PAGE = pnlAllListings.Controls.Count;
            this.listingsData = new Listings(db, loggedInCustomer, NUM_LISTINGS_PER_PAGE);
            this.listingsGui = new ListingGui[NUM_LISTINGS_PER_PAGE];
        }

        #region DB

        private void RefreshCustomerObject()
        {
            db.Refresh(RefreshMode.OverwriteCurrentValues, loggedInCustomer); // note: needed to set 'MultipleActiveResultSets=true' in app.config in order for this to work
        }

        #endregion

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
            public Panel ListingPanel { get; private set; }
            public PictureBox ItemImagePictureBox { get; private set; }
            public RichTextBox TitleDescriptionRichTextBox { get; private set; }
            public Label PriceLabel { get; private set; }
            public Button AddToCartButton { get; private set; }
            public NumericUpDown QuantityNumericUpDown { get; private set; }
            public Label StatusInfoLabel { get; private set; }
            public Label SelectQuantityLabel { get; private set; }
            public ListingData ListingData { get; private set; }

            private bool listingEnabled;

            public ListingGui(Panel AllListingsPanel, int listingIndex, ListingData listingData)
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

                EnableListing(); // enable in case this listing was previously disabled on a previous page
            }

            // Enable the current listing. 
            public void EnableListing()
            {
                this.listingEnabled = true;
                ItemImagePictureBox.Image = null;
                ListingPanel.Enabled = true;
            }

            // Disable the current listing, such as when no item is being displayed in it 
            public void DisableListing()
            {
                this.listingEnabled = false;
                ItemImagePictureBox.Enabled = false;
                ListingPanel.Enabled = false;
                TitleDescriptionRichTextBox.Text = "";
                PriceLabel.Text = "";
            }

            public async void DisplayAddToCartConfirmation(int displayTimeLengthMs = 5000)
            {
                StatusInfoLabel.ForeColor = Color.Green;
                StatusInfoLabel.Text = "Item added to cart";

                await Task.Delay(displayTimeLengthMs); // clear text after displayTimeLengthMs
                StatusInfoLabel.Text = "";
                StatusInfoLabel.ForeColor = Color.Black;
            }

            public void RefreshStatusInfoLabel()
            {
                StatusInfoLabel.Text = "";
            }

            // Refresh/update the numeric limits for the quantity NumericUpDown control based on how many of
            // this item is presently in the cart
            public void RefreshUserOptions(BindingList<CartItem> cartItems)
            {
                if (listingEnabled)
                {
                    int qtyInCart = 0;
                    if (cartItems.Any(item => item.GetStoreItem() == ListingData.StoreItem))
                    {
                        qtyInCart = cartItems.First(item => item.GetStoreItem() == ListingData.StoreItem).Quantity;
                    }

                    int remainingQty = ListingData.StoreItem.QuantityAvailable - qtyInCart;

                    QuantityNumericUpDown.Maximum = remainingQty;
                    QuantityNumericUpDown.Value = remainingQty > 0 ? 1 : 0;
                    AddToCartButton.Enabled = remainingQty > 0;
                }
            }
        }

        #endregion

        #region ListingLoading

        private void RefreshListingsTab()
        {
            LoadStoreItemsIntoGui(listingsData.GetListingDataForPage(currentPageIndex));
            RefreshLblPageNum();
            RefreshBtnPreviousPage();
            RefreshBtnNextPage();
        }

        /// <summary>
        /// Go through each of the listingData and populate each GUI listing with the item's details.
        /// If less there are less items in listingData than NUM_LISTINGS_PER_PAGE, the remaining listings on the page
        /// are disabled.
        /// </summary>
        /// <param name="listingData">The listingData to populate the GUI listings with</param>
        private void LoadStoreItemsIntoGui(IEnumerable<ListingData> listingData)
        {
            IEnumerator<ListingData> listingDataEnumerator = listingData.GetEnumerator();
            for (int i = 0; i < NUM_LISTINGS_PER_PAGE; i++)
            {
                if (listingDataEnumerator.MoveNext())
                {
                    listingsGui[i] = new ListingGui(pnlAllListings, i, listingDataEnumerator.Current);
                    // todo move all below (or at least the method calls?) into the ListingGui constructor?
                    listingsGui[i].TitleDescriptionRichTextBox.Text = listingDataEnumerator.Current.Title;
                    listingsGui[i].PriceLabel.Text = listingDataEnumerator.Current.FormattedPrice;
                    listingsGui[i].ItemImagePictureBox.Image = new Bitmap(listingDataEnumerator.Current.ItemImage,
                        listingsGui[i].ItemImagePictureBox.Size.Width, listingsGui[i].ItemImagePictureBox.Size.Height);
                    listingsGui[i].RefreshUserOptions(cartItems);
                    listingsGui[i].RefreshStatusInfoLabel();
                }
                else
                {
                    listingsGui[i] = new ListingGui(pnlAllListings, i, null);
                    listingsGui[i].DisableListing();
                }
            }

            listingDataEnumerator.Dispose();
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            currentPageIndex++;
            RefreshListingsTab();
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            currentPageIndex--;
            RefreshListingsTab();
        }

        private void RefreshBtnPreviousPage()
        {
            btnPreviousPage.Enabled = currentPageIndex > 0;
        }

        private void RefreshBtnNextPage()
        {
            btnNextPage.Enabled = currentPageIndex < listingsData.HighestPageIndexRetrieved || !listingsData.AllListingsWereRetrieved;
        }

        private void RefreshLblPageNum()
        {
            lblPageNum.Text = "Page " + currentPageNumDisplay;
        }

        #endregion

        #endregion

        #region Cart

        private void RefreshCartTab()
        {
            RefreshBtnPurchaseCartItems();
            RefreshBtnRemoveItemFromCart();
            RefreshCartSummary();
            RefreshCartItemsView();
        }

        private void RefreshCartItemsView()
        {
            // this method needs to be called whenever the cart tab comes into view and whenever the cart items change while the user is on the cart page
            dgvCartItems.DataSource = cartItems.ToList();
            dgvCartItems.AutoResizeColumns();
            dgvCartItems.Update();
            dgvCartItems.Refresh();
        }

        private void btnAddToCart0_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(listingsGui[0]);
            AddItemToCart(cartItem);
            listingsGui[0].RefreshUserOptions(cartItems);
            listingsGui[0].DisplayAddToCartConfirmation();
        }

        private void btnAddToCart1_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(listingsGui[1]);
            AddItemToCart(cartItem);
            listingsGui[1].RefreshUserOptions(cartItems);
            listingsGui[1].DisplayAddToCartConfirmation();
        }

        private void btnAddToCart2_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(listingsGui[2]);
            AddItemToCart(cartItem);
            listingsGui[2].RefreshUserOptions(cartItems);
            listingsGui[2].DisplayAddToCartConfirmation();
        }

        private void btnAddToCart3_Click(object sender, EventArgs e)
        {
            CartItem cartItem = CreateCartItemForListing(listingsGui[3]);
            AddItemToCart(cartItem);
            listingsGui[3].RefreshUserOptions(cartItems);
            listingsGui[3].DisplayAddToCartConfirmation();
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

            lblCartSummary.ForeColor = Color.Green;
            lblCartSummary.Text = "Purchase completed";
            RefreshCartItemsView();
            RefreshBtnPurchaseCartItems();
            RefreshBtnRemoveItemFromCart();
        }

        private void btnRemoveItemFromCart_Click(object sender, EventArgs e)
        {
            RemoveSelectedItemsFromCart();
            RefreshCartTab();
        }

        private void AddItemToCart(CartItem cartItem)
        {
            if (cartItems.Any(item => item.GetStoreItem() == cartItem.GetStoreItem()))
            {
                cartItems.First(item => item.GetStoreItem() == cartItem.GetStoreItem()).Quantity += cartItem.Quantity;
            }
            else
            {
                cartItems.Add(cartItem);
            }
        }

        private async void CreatePurchase(CUSTOMER loggedInCustomer, List<CartItem> cartItems)
        {
            var list = new List<object>();
            cartItems.ForEach(item => list.Add(new
            {
                StoreItemId = item.GetStoreItem().StoreItemId,
                Quantity = item.Quantity,
                UnitPrice = item.GetStoreItem().Price
            }));
            string jsonString = JsonSerializer.Serialize(list);
            await Task.Run(() =>
            {
                db.CREATE_NEW_PURCHASE(loggedInCustomer.CustomerId, jsonString);
                // run asynchronously so as not to slow down GUI
            });

            listingsData.RefreshListingsFromDb();
            RefreshCustomerObject(); // so that balance is updated
            currentPageIndex = 0; // reset so that if the last page is now out of range, it isn't potentially revisited
        }

        private void RemoveSelectedItemsFromCart()
        {
            foreach (DataGridViewRow selectedItem in dgvCartItems.SelectedRows)
            {
                cartItems.Remove(selectedItem.DataBoundItem as CartItem);
            }
        }

        private CartItem CreateCartItemForListing(ListingGui listingGui)
        {
            STORE_ITEM storeItem = listingGui.ListingData.StoreItem;
            int quantitySelected = Convert.ToInt32(listingGui.QuantityNumericUpDown.Value);
            return new CartItem(storeItem, quantitySelected);
        }

        private void RefreshBtnPurchaseCartItems()
        {
            btnPurchaseCartItems.Enabled = cartItems.Count > 0;
        }

        private void RefreshBtnRemoveItemFromCart()
        {
            btnRemoveItemFromCart.Enabled = cartItems.Count > 0;
        }

        private void RefreshCartSummary()
        {
            lblCartSummary.ForeColor = Color.Black;
            lblCartSummary.Text = $"Total Quantity: {cartItems.Sum(item => item.Quantity)}\nPurchase Total: ${cartItems.Sum(item => item.Quantity * item.GetStoreItem().Price).ToString("0.00")}";
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

        private void btnPayToBalance_Click(object sender, EventArgs e)
        {
            if (nudPayToBalance.Value <= 0) // this shouldn't really be possible because button should remain disabled until valid amount entered
            {
                lblAccountBalanceResults.Text = $"Please enter a valid amount";
                lblAccountBalanceResults.ForeColor = Color.Red;
                return;
            }

            PayToCustomerBalance(nudPayToBalance.Value);
            RefreshDisplayedBalance();
            lblAccountBalanceResults.ForeColor = Color.Green;
            lblAccountBalanceResults.Text = $"${nudPayToBalance.Value} successfully paid to balance";
            nudPayToBalance.Value = 0;
        }

        private void PayToCustomerBalance(decimal amount)
        {
            loggedInCustomer.Balance += amount;
            db.SubmitChanges();
        }

        private void RefreshBalanceTab()
        {
            RefreshDisplayedBalance();
        }

        private void RefreshDisplayedBalance()
        {
            decimal balance = loggedInCustomer.Balance;
            if (balance > 0)
            {
                lblCurrentBalance.ForeColor = Color.Green;
            }
            else if (balance == 0)
            {
                lblCurrentBalance.ForeColor = Color.Black;
            }
            else
            {
                lblCurrentBalance.ForeColor = Color.Red;
            }

            lblCurrentBalance.Text = $"Current Balance: ${balance.ToString("0.00")}";
        }

        #endregion

        #region Purchases

        private void RefreshPurchasesTab()
        {
            RefreshPurchasesView(false);
            RefreshPurchasesFilters();
            RefreshPurchasesSummary();
        }

        private void PurchasesFilter_ValueChanged(object sender, EventArgs e)
        {
            FilterPurchases();
        }

        private void FilterPurchases()
        {
            RefreshPurchasesView(true);
            RefreshPurchasesSummary();
        }

        private void RefreshPurchasesView(bool applyUserFilters = false)
        {
            dgvPastPurchases.DataSource = GetPurchasesForCustomer(applyUserFilters)
                .Select(p => new
                {
                    Date = p.PurchaseDateTime.Date,
                    OrderTotal = "$" + p.TotalPrice.ToString("0.00"),
                    p.TotalQuantity,
                    Items = string.Join(", ",
                        p.PURCHASE_STORE_ITEMs.Select(item =>
                            $"({item.Quantity}) {item.STORE_ITEM.Manufacturer + " " + item.STORE_ITEM.ProductName} - ${item.UnitPrice * item.Quantity}")) // get summary of items in purchase
                })
                .ToList();
            dgvPastPurchases.AutoResizeColumns();
            dgvPastPurchases.Update();
            dgvPastPurchases.Refresh();
        }

        private void RefreshPurchasesSummary()
        {
            var purchases = GetPurchasesForCustomer();
            int numPurchases = purchases.Count();
            int totalUnits = purchases.Sum(p => p.TotalQuantity);
            decimal purchasesTotal = purchases.Sum(p => p.TotalPrice);
            lblPurchasesSummary.Text = $"{numPurchases} Purchase{(numPurchases > 1 ? "s" : "")}\n" +
                                       $"{totalUnits} Unit{(numPurchases > 1 ? "s" : "")}\n" +
                                       $"${purchasesTotal.ToString("0.00")} Total";
        }

        // Set the initial values for the filters based on this customer's purchases
        private void RefreshPurchasesFilters()
        {
            var purchases = GetPurchasesForCustomer(false).ToList();

            // date filters:
            dtpPurchasesFromDate.MinDate = purchases.Min(p => p.PurchaseDateTime);
            dtpPurchasesFromDate.MaxDate = purchases.Max(p => p.PurchaseDateTime);
            dtpPurchasesFromDate.Value = dtpPurchasesFromDate.MinDate;
            dtpPurchasesToDate.MinDate = dtpPurchasesFromDate.MinDate;
            dtpPurchasesToDate.MaxDate = dtpPurchasesFromDate.MaxDate;
            dtpPurchasesToDate.Value = dtpPurchasesToDate.MaxDate;

            // total price filters:
            nudPurchasesPriceFrom.Minimum = purchases.Min(p => p.TotalPrice);
            nudPurchasesPriceFrom.Maximum = purchases.Max(p => p.TotalPrice);
            nudPurchasesPriceFrom.Value = nudPurchasesPriceFrom.Minimum;
            nudPurchasesPriceTo.Minimum = nudPurchasesPriceFrom.Minimum;
            nudPurchasesPriceTo.Maximum = nudPurchasesPriceFrom.Maximum;
            nudPurchasesPriceTo.Value = nudPurchasesPriceTo.Maximum;
        }

        private IEnumerable<PURCHASE> GetPurchasesForCustomer(bool applyUserFilters = false)
        {
            var purchases = db.PURCHASEs.Where(p => p.CUSTOMER == loggedInCustomer);
            if (applyUserFilters)
            {
                purchases = purchases.Where(p =>
                    p.PurchaseDateTime.Date >= dtpPurchasesFromDate.Value.Date &&
                    p.PurchaseDateTime.Date <= dtpPurchasesToDate.Value.Date &&
                    p.TotalPrice >= nudPurchasesPriceFrom.Value &&
                    p.TotalPrice <= nudPurchasesPriceTo.Value);
            }

            return purchases;
        }

        #endregion

        #endregion

        // This class implements IDisposable as the listingsData must be disposed
        public void Dispose()
        {
            listingsData.Dispose();
        }
    }
}