using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemesterProject
{
    public class StoreItemListing
    {
        public STORE_ITEM StoreItem { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string FormattedPrice { get; private set; }
        public Image Image { get; private set; }

        public StoreItemListing(STORE_ITEM storeItem)
        {
            this.StoreItem = new STORE_ITEM()
            {
                StoreItemId = storeItem.StoreItemId,
                Manufacturer = storeItem.Manufacturer,
                ProductName = storeItem.ProductName,
                QuantityAvailable = storeItem.QuantityAvailable,
                Price = storeItem.Price,
                ImageUrl = storeItem.ImageUrl
            };
            Title = this.StoreItem.Manufacturer + "\n" + this.StoreItem.ProductName;
            SubTitle = "Other details - store in db... todo"; // todo
            FormattedPrice = "$" + Convert.ToString(this.StoreItem.Price);  // todo
            Image = Image.FromFile(
                "C:\\Users\\jackt\\OneDrive\\Programs\\C#\\Advanced Topics OOP\\SemesterProject - Copy\\SemesterProject\\resources\\StoreItemImages\\SGS24U_titanium_gray.jpg");
        }
    }
}
