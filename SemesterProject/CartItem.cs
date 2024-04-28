using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemesterProject
{
    // The purpose of this class is to provide an easy-to-use object for displaying the
    // Store Items in the customer's cart
    public class CartItem
    {
        // Since public properties are automatically displayed as a column in the DataGridView we use to
        // display the CartItems, we only make properties public if we want them to be displayed to the end user.
        // Private properties will be automatically hidden from user and can be retrieved from a 
        // dedicated method.
        public string Manufacturer { get; }
        public string ProductName { get; }
        public decimal UnitPrice { get; }
        public int Quantity { get; set; }
        public decimal Price => UnitPrice * Quantity;

        private STORE_ITEM StoreItem { get; }


        public CartItem(STORE_ITEM storeItem, int quantity)
        {
            this.StoreItem = storeItem;
            this.Manufacturer = storeItem.Manufacturer;
            this.ProductName = storeItem.ProductName;
            this.UnitPrice = storeItem.Price;
            this.Quantity = quantity;
        }

        public CartItem()
        {
        }

        public STORE_ITEM GetStoreItem()
        {
            return this.StoreItem;
        }
    }
}