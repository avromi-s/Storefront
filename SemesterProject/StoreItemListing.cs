using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SemesterProject.Properties;

namespace SemesterProject
{
    public class StoreItemListing
    {
        private readonly string STORE_ITEM_IMAGES_PATH = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\img\\StoreItemImages\\";
        public STORE_ITEM StoreItem { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string FormattedPrice { get; private set; }
        public Image ItemImage { get; private set; }

        public StoreItemListing(STORE_ITEM storeItem)
        {
            this.StoreItem = new STORE_ITEM()
            {
                StoreItemId = storeItem.StoreItemId,
                Manufacturer = storeItem.Manufacturer,
                ProductName = storeItem.ProductName,
                QuantityAvailable = storeItem.QuantityAvailable,
                Price = storeItem.Price,
                ImagePath = storeItem.ImagePath
            };
            Title = this.StoreItem.Manufacturer + "\n" + this.StoreItem.ProductName;
            SubTitle = "Other details - store in db... todo"; // todo
            FormattedPrice = "$" + Convert.ToString(this.StoreItem.Price); // todo
            try
            {
                ItemImage = Image.FromFile(STORE_ITEM_IMAGES_PATH + storeItem.ImagePath);
            }

            catch (Exception e)
            {
                ItemImage = Resources.ImageNotFound;
                Console.WriteLine("Unable to load ItemImage file from ImagePath when constructing StoreItemListing: " + e);
            }
        }
    }
}