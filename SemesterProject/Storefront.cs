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
        // todo update label in cart based on items for summary
        // todo add db trigger or application function to update store_item quantity on purchase made
        private DataClasses1DataContext db;
        private IEnumerator<STORE_ITEM> AllStoreItems;
        private List<STORE_ITEM> CachedStoreItems = new List<STORE_ITEM>();
        private readonly CUSTOMER LoggedInCustomer;
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

        public Storefront(DataClasses1DataContext db, CUSTOMER loggedInCustomer)
        {
            InitializeComponent();

            this.db = db;
            this.LoggedInCustomer = loggedInCustomer;
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
            dgvCartItems.Columns["StoreItem"].Visible = false;

            // todo see if we can implement adding an 'item id' column linked to each cartitems' storeitem.storeitemid
            //DataGridViewColumn dgvCol = new DataGridViewColumn();
            //dgvCol.HeaderText = "Item ID";
            //dgvCol.DataPropertyName = "StoreItem.StoreItemId";
            //dgvCartItems.Columns.Add(dgvCol);
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
                IsAnotherItem = AllStoreItems.MoveNext(); // also, update IsAnotherItem before return so that it is accurate regardless of if this loop finishes (which is dependent on how much the calling method iterates)
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

        #region ButtonClickHandlers 

        #region ListingManagement
        private void btnNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum++;
            UpdateLblPageNum();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));

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
            if (CartItems.Any(item => item.StoreItem == storeItem))
            {
                qtyInCart = CartItems.Where(item => item.StoreItem == storeItem).First().QuantitySelected;
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
            if (CartItems.Any(item => item.StoreItem == cartItem.StoreItem))  // todo use hashmap from StoreItemId -> CartItem for CartItems for faster lookup? now it is n for each search
            {
                CartItems.Where(item => item.StoreItem == cartItem.StoreItem).First().QuantitySelected += cartItem.QuantitySelected;
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

        // On purchase button click - purchase all items in cart and then empty the cart
        private void btnPurchaseCartItems_Click(object sender, EventArgs e)
        {
            if (CartItems.Count > 0)
            {
                PURCHASE purchase = new PURCHASE()
                {
                    CUSTOMER = LoggedInCustomer,
                    TotalQuantity = CartItems.Sum(item => item.QuantitySelected),
                    TotalPrice = CartItems.Sum(item => item.UnitPrice * item.QuantitySelected),
                    PurchaseDateTime = DateTime.Now  // todo use db datetime?
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
            }
            else
            {
                // todo, shouldn't be an else - purchase button should be disabled unless there are items in cart
                // and also the remove item button unless an item is selected
            }
        }

        private void btnRemoveItemFromCart_Click(object sender, EventArgs e)
        {
            // todo remove item from CartItems. Allow update quantity?
        }

        private void tpCart_Click(object sender, EventArgs e)
        {
            lblCartSummary.Text = "hi";
        }
        #endregion

        #endregion
    }
}
