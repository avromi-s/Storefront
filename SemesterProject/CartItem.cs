using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemesterProject
{
    public class CartItem
    {
        public int StoreItemId { get; set; }
        public string Manufacturer { get; set; }
        public string ProductName { get; set; }
        public int QuantitySelected { get; set; }

        public CartItem(STORE_ITEM storeItem, int quantitySelected)
        {
            this.StoreItemId = storeItem.StoreItemId;
            this.Manufacturer = storeItem.Manufacturer;
            this.ProductName = storeItem.ProductName;
            this.QuantitySelected = quantitySelected;
        }

        public CartItem()
        {
        }
    }
}
