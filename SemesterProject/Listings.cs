using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SemesterProject
{
    // This class is used for accessing the ListingData of all listings in a given user-session
    internal class Listings : IDisposable
    {
        private DataClasses1DataContext db;
        private CUSTOMER loggedInCustomer;
        private readonly int NUM_LISTINGS_PER_PAGE;

        // We iterate through the store items via the allStoreItems iterator, and cache the values
        // (wrapped in ListingData objects) in cachedListingData as each item is retrieved.
        // After iterating through all listings, cachedListingData will contain all listings.
        private IEnumerator<STORE_ITEM> allStoreItems;

        private List<ListingData> cachedListingData = new List<ListingData>();

        public bool AllListingsWereRetrieved { get; private set; }
        public int HighestPageIndexRetrieved { get; private set; }

        public Listings(DataClasses1DataContext db, CUSTOMER loggedInCustomer, int numListingsPerPage)
        {
            this.db = db;
            this.loggedInCustomer = loggedInCustomer;
            NUM_LISTINGS_PER_PAGE = numListingsPerPage;
            RefreshAllStoreItems();
        }


        // Return the ListingData for each listing in the provided pageIndex.
        // If the provided pageIndex is >= the last page index, this method returns ListingData only
        // for the listings up until the last one (inclusive).
        public IEnumerable<ListingData> GetListingDataForPage(int pageIndex)
        {
            HighestPageIndexRetrieved = pageIndex > HighestPageIndexRetrieved ? pageIndex : HighestPageIndexRetrieved;

            var numListingsBeforeThisPage = pageIndex * NUM_LISTINGS_PER_PAGE;
            for (var i = 0; i < NUM_LISTINGS_PER_PAGE; i++)
            {
                var listingIndex = numListingsBeforeThisPage + i;
                if (listingIndex >= cachedListingData.Count && AllListingsWereRetrieved) yield break;

                yield return GetListingData(listingIndex);
            }
        }

        // Retrieve the ListingData for the provided listingIndex.
        // Throws an ArgumentOutOfRangeException if the provided listingIndex is out of range
        // (i.e., there are not (listingIndex + 1) listings in this user-session)
        public ListingData GetListingData(int listingIndex)
        {
            // get item from cache before retrieving it from the db
            // if listing isn't already cached, retrieve the listings from the db (via iteration) until 
            // we have retrieved the listing for the provided listingIndex or until there are no more items left
            var desiredItemIsCached = listingIndex < cachedListingData.Count;
            while (!desiredItemIsCached)
            {
                if (AllListingsWereRetrieved) throw new ArgumentOutOfRangeException();

                var storeItem = allStoreItems.Current;
                var listingData = new ListingData(storeItem);
                cachedListingData.Add(listingData);
                AllListingsWereRetrieved = !allStoreItems.MoveNext();
                desiredItemIsCached = listingIndex < cachedListingData.Count;
            }

            return cachedListingData[listingIndex];
        }

        private void RefreshAllStoreItems(bool includeOutOfStock = false)
        {
            allStoreItems?.Dispose();
            // The order of the store items retrieved here will be the order of the listings as displayed to the user
            allStoreItems = db.STORE_ITEMs.Where(item => includeOutOfStock || item.QuantityAvailable > 0)
                .OrderByDescending(item => item.QuantityAvailable)
                .GetEnumerator();
            AllListingsWereRetrieved = !allStoreItems.MoveNext();
        }

        public void RefreshListingsFromDb()
        {
            db.Refresh(RefreshMode.OverwriteCurrentValues, db.STORE_ITEMs);
            RefreshAllStoreItems();
            cachedListingData.Clear();
            HighestPageIndexRetrieved = -1; // reset so that if the last page is now out of range, it isn't potentially revisited
        }

        // This class implements IDisposable as the STORE_ITEMs retrieved from the db must be disposed
        public void Dispose()
        {
            allStoreItems?.Dispose();
        }
    }
}