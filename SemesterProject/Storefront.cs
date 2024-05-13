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
        // todo fix issue with loggedInCustomer reference not being updated after purchase made with sp
        // - throws exception on attempted change to balance and displayed balance itself is unupdated after the purchase
        // todo fix currency display accross all displays
        // todo separate different sections of the GUI Store into classes within Storefront class for better organization, instead of just regions
        //      Store > Cart, & Account > Balance, Account > Purchases
        // todo username & likely also password not case sensitive
        // todo make balance label red or green based on positive or negative balance
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

        #region DB

        /*private void RefreshDbObjects()
        {
            db.Refresh(RefreshMode.OverwriteCurrentValues);
        }*/

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
            private bool listingEnabled;

            public ListingGui(Panel AllListingsPanel, int listingIndex, ListingData listingData,
                bool disableListing = false)
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
                    EnableListing(); // enable in case this listing was previously disabled on a previous page
                }
            }

            // Enable the current listing. 
            private void EnableListing()
            {
                this.listingEnabled = true;
                ItemImagePictureBox.Image = null;
                ListingPanel.Enabled = true;
            }

            // Disable the current listing, such as when no item is being displayed in it 
            private void DisableListing()
            {
                this.listingEnabled = false;
                ItemImagePictureBox.Image = new Bitmap(Resources.ImageNotFound, ItemImagePictureBox.Size.Width,
                    ItemImagePictureBox.Size.Height);
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

            // Refresh/update the numeric limits for the quantity NumericUpDown control based on how many of
            // this item is presently in the cart
            public void RefreshQuantityControlLimits(BindingList<CartItem> cartItems)
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
                    // todo if remainingQty <= 0 disable listing or at least add to cart button?
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
                    listingsGui[i].TitleDescriptionRichTextBox.Text = listingDataEnumerator.Current.Title;
                    listingsGui[i].PriceLabel.Text = listingDataEnumerator.Current.FormattedPrice;
                    listingsGui[i].ItemImagePictureBox.Image = new Bitmap(listingDataEnumerator.Current.ItemImage,
                        listingsGui[i].ItemImagePictureBox.Size.Width, listingsGui[i].ItemImagePictureBox.Size.Height);
                    listingsGui[i].RefreshQuantityControlLimits(cartItems);
                }
                else
                {
                    listingsGui[i] = new ListingGui(pnlAllListings, i, null, disableListing: true);
                }
            }
        }

        #endregion

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
            btnNextPage.Enabled =
                currentPageIndex < listingsData.HighestPageIndexRetrieved || listingsData.IsAnotherItem;
        }

        private void RefreshLblPageNum()
        {
            lblPageNum.Text = "Page " + currentPageNumDisplay;
        }

        #endregion

        #region Cart

        private void RefreshCartTab()
        {
            RefreshBtnPurchaseCartItems();
            RefreshBtnRemoveItemFromCart();
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
            RefreshBtnPurchaseCartItems();
            RefreshBtnRemoveItemFromCart();
            //RefreshAllStoreItems();
        }

        private void btnRemoveItemFromCart_Click(object sender, EventArgs e)
        {
            RemoveSelectedItemsFromCart();
            RefreshCartTab();
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
            listingsData.RefreshListingsFromDb();
        }

        private void RemoveSelectedItemsFromCart()
        {
            foreach (DataGridViewRow selectedItem in dgvCartItems.SelectedRows)
            {
                cartItems.Remove(selectedItem.DataBoundItem as CartItem);
            }
        }

        private CartItem CreateCartItemForListing(int listingIndex)
        {
            int quantitySelected = Convert.ToInt32(listingsGui[listingIndex].QuantityNumericUpDown.Value);
            STORE_ITEM storeItem = listingsGui[listingIndex].ListingData.StoreItem; //listingsData.GetListingData((currentPageIndex * NUM_LISTINGS_PER_PAGE) + listingIndex).StoreItem;
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
                    Items = string.Join(", ",
                        p.PURCHASE_STORE_ITEMs.Select(item =>
                            $"({item.Quantity}) {item.STORE_ITEM.Manufacturer + " " + item.STORE_ITEM.ProductName} - ${item.UnitPrice * item.Quantity}")) // get summary of items in purchase
                })
                .ToList();
            dgvPastPurchases.AutoResizeColumns();
            dgvPastPurchases.Update();
            dgvPastPurchases.Refresh();
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