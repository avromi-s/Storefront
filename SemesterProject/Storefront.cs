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
        private DataClasses1DataContext db;
        private IEnumerator<STORE_ITEM> AllStoreItems;
        private List<STORE_ITEM> CachedStoreItems = new List<STORE_ITEM>();
        private readonly int LoggedInCustomerId;
        private BindingList<CartItem> CartItems = new BindingList<CartItem>();
        private bool IsAnotherItem { get; set; }  // todo naming

        private readonly int NumItemsPerPage = 4;  // todo maybe derive from gui
        private int CurrentPageNum = 0;  // 0-indexed for easy use with collections
        private int CurrentPageNumDisplay  // 1-indexed for user display
        {
            get
            {
                return CurrentPageNum + 1;
            }
        }

        public Storefront(DataClasses1DataContext db, int loggedInCustomerId)
        {
            InitializeComponent();

            this.db = db;
            this.LoggedInCustomerId = loggedInCustomerId;
            LoadCustomerInfo();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));
            // todo should next page button be disabled if less than 4 items in store? else will throw error
            if (IsAnotherItem)
            {
                btnNextPage.Enabled = true;
            }
        }

        #region Setup
        // On load, set up cart data source
        private void Storefront_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'storeDB_Purchases2.PURCHASE' table. You can move, or remove it, as needed.
            //this.pURCHASETableAdapter1.Fill(db.PURCHASEs.Where(row => row.CustomerId == LoggedInCustomerId);
            dgvCartItems.DataSource = CartItems;
            dgvCartItems.Columns["StoreItemId"].HeaderText = "Item ID";
        }

        private void LoadCustomerInfo()
        {
            // todo
        }
        #endregion

        #region ListingLoading

        /// <summary>
        /// Go through each of the storeItems and populate each GUI listing with the item's details.
        /// </summary>
        /// <param name="storeItems">The items to populate the GUI listings with</param>
        private void LoadStoreItemsIntoGUI(IEnumerable<STORE_ITEM> storeItems)
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
                UpdateQuantityControlForListing(i);

                i = (i + 1) % NumItemsPerPage;  // move to next listing to update, reset to the first listing (index 0) if we move past the last listing
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
                CachedStoreItems.Add(storeItem);  // cache item before returning in case another method tries to retrieve it already
                yield return storeItem;
                IsAnotherItem = AllStoreItems.MoveNext(); // todo move this before yield return?
            }
        }

        private void RetrieveAllStoreItems()
        {
            // The order retrieved here will be the order of the items as displayed to the user
            // todo use a smarter ordering maybe? not just based on quantity
            AllStoreItems = db.STORE_ITEMs.OrderByDescending(item => item.QuantityAvailable).GetEnumerator();
        }

        #endregion

        #region ButtonClickHandlers 

        #region ListingManagement
        private void btnNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum++;
            UpdateLblPageNum();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));

            btnPreviousPage.Enabled = true;
            bool IsNoMoreCachedItems = CurrentPageNum >= CachedStoreItems.Count / NumItemsPerPage;
            if (IsNoMoreCachedItems && !IsAnotherItem)
            {
                btnNextPage.Enabled = false;
            }
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum--;
            UpdateLblPageNum();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));

            btnNextPage.Enabled = true;
            if (CurrentPageNum == 0)
            {
                btnPreviousPage.Enabled = false;
            }
        }

        private void UpdateLblPageNum()
        {
            lblPageNum.Text = "Page " + CurrentPageNumDisplay;
        }

        private RichTextBox GetRichTextBoxForListing(int listingIndex)
        {
            // todo this can be updated to be a checkbox or something non-text that updates on add to cart
            return pnlAllListings.Controls["pnlListing" + listingIndex].Controls["rtbMainItemInfo" + listingIndex] as RichTextBox;
        }

        private void UpdateQuantityControlForListing(int listingIndex)
        {
            STORE_ITEM storeItem = CachedStoreItems[(CurrentPageNum * NumItemsPerPage) + listingIndex];
            int totalQtyAvail = storeItem.QuantityAvailable;
            int qtyInCart;
            if (CartItems.Any(item => item.StoreItemId == storeItem.StoreItemId))
            {
                qtyInCart = CartItems.Where(item => item.StoreItemId == storeItem.StoreItemId).First().QuantitySelected;
            }
            else
            {
                qtyInCart = 0;
            }
            int remainingQty = totalQtyAvail - qtyInCart;

            NumericUpDown nudControl = (pnlAllListings.Controls["pnlListing" + listingIndex].Controls["nudQuantity" + listingIndex] as NumericUpDown);
            nudControl.Maximum = remainingQty;
            nudControl.Value = remainingQty > 0 ? 1 : 0;
            // todo if remainingQty <= 0 disable listing or at least add to cart button?
        }

        #endregion

        #region CartManagement
        private void AddItemToCart(CartItem cartItem, Control controlToUpdate)
        {
            if (CartItems.Any(item => item.StoreItemId == cartItem.StoreItemId))  // todo use hashmap from StoreItemId -> CartItem for CartItems for faster lookup? now it is n for each search
            {
                CartItems.Where(item => item.StoreItemId == cartItem.StoreItemId).First().QuantitySelected += cartItem.QuantitySelected;
            }
            else
            {
                CartItems.Add(cartItem);
            }
            dgvCartItems.Update();
            dgvCartItems.Refresh();
            controlToUpdate.Text += "\nItem added to cart";  // todo this can be updated to be a checkbox or something non-text that updates on add to cart
        }

        private CartItem GetCartItemForListing(int listingIndex)
        {
            // todo this extracted to a separate method so that we can replace when we use a custom user control for each listing
            // instead of accessing everything based on their names and index like here
            int quantitySelected = Convert.ToInt32((pnlAllListings.Controls["pnlListing" + listingIndex].Controls["nudQuantity" + listingIndex] as NumericUpDown).Value);
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
            UpdateQuantityControlForListing(0);
        }

        private void btnAddToCart1_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(1);
            RichTextBox listingrtb = GetRichTextBoxForListing(1);
            AddItemToCart(cartItem, listingrtb);
            UpdateQuantityControlForListing(1);
        }

        private void btnAddToCart2_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(2);
            RichTextBox listingrtb = GetRichTextBoxForListing(2);
            AddItemToCart(cartItem, listingrtb);
            UpdateQuantityControlForListing(2);
        }

        private void btnAddToCart3_Click(object sender, EventArgs e)
        {
            // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
            (sender as Button).BackColor = Color.Green;
            (sender as Button).ForeColor = Color.White;
            CartItem cartItem = GetCartItemForListing(3);
            RichTextBox listingrtb = GetRichTextBoxForListing(3);
            AddItemToCart(cartItem, listingrtb);
            UpdateQuantityControlForListing(3);
        }

        private void btnPurchaseCartItems_Click(object sender, EventArgs e)
        {
            // todo connect to db and add purchases for the loggedInCustomer. use CartItems list
            foreach (CartItem cartItem in CartItems)
            {
                db.PURCHASEs.InsertOnSubmit(new PURCHASE()
                {
                    CustomerId = LoggedInCustomerId,
                    StoreItemId = cartItem.StoreItemId,
                    Quantity = cartItem.QuantitySelected,
                    UnitPrice = cartItem.Price,
                    PurchaseDateTime = DateTime.Now  // todo use db datetime?
                });
            }
            db.SubmitChanges();
            CartItems.Clear();
        }

        private void btnRemoveItemFromCart_Click(object sender, EventArgs e)
        {
            // todo remove item from CartItems. Allow update quantity?
        }
        #endregion

        #endregion

    }
}
