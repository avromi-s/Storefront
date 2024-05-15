using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemesterProject
{
    // The purpose of this class is to provide a pre-made object with the correct fields/columns for displaying the
    // store items in the customer's cart
    public class CartItem
    {
        private STORE_ITEM StoreItem { get; }

        // Since public properties are automatically displayed as a column in the DataGridView we use to
        // display the CartItems, we only make properties public if we want them to be displayed to the end user.
        // Private properties will be automatically hidden from user and can be retrieved from a 
        // dedicated method.
        public string Manufacturer => StoreItem.Manufacturer;
        public string ProductName => StoreItem.ProductName;
        public string UnitPrice => "$" + StoreItem.Price.ToString("0.00"); // stored as a string so that we can format it, the decimal UnitPrice can be retrieved via the StoreItem
        public string Price => "$" + (StoreItem.Price * Quantity).ToString("0.00");
        public int Quantity { get; set; } // this is updated as items are added to the cart

        public CartItem(STORE_ITEM storeItem, int quantity)
        {
            this.StoreItem = storeItem;
            this.Quantity = quantity;
        }

        public STORE_ITEM GetStoreItem()
        {
            return this.StoreItem;
        }
    }
}