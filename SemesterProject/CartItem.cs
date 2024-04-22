using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemesterProject
{
    public class CartItem
    {
        public STORE_ITEM StoreItem { get; set; }
        public string Manufacturer { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int QuantitySelected { get; set; }

        public CartItem(STORE_ITEM storeItem, int quantitySelected)
        {
            this.StoreItem = storeItem;
            this.Manufacturer = storeItem.Manufacturer;
            this.ProductName = storeItem.ProductName;
            this.UnitPrice = storeItem.Price;
            this.QuantitySelected = quantitySelected;
        }

        public CartItem()
        {
        }
    }
}
