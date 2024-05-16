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
    internal class AllListingsData : IDisposable
    {
        private DataClasses1DataContext db;
        private readonly int NUM_LISTINGS_PER_PAGE;

        // We iterate through the store items via the allStoreItems iterator, and save the values
        // (wrapped in ListingData objects) in savedListingData as each item is retrieved.
        // After iterating through all listings, savedListingData will contain all listings and can be 
        // re-retrieved from there.
        private IEnumerator<STORE_ITEM> allStoreItems;

        private List<ListingData> savedListingData = new List<ListingData>();

        public bool AllListingsWereRetrieved { get; private set; }
        public int HighestPageIndexRetrieved { get; private set; }

        public AllListingsData(DataClasses1DataContext db, int numListingsPerPage)
        {
            this.db = db;
            NUM_LISTINGS_PER_PAGE = numListingsPerPage;
            ResetStoreItemEnumerator();
        }


        // Return the ListingData for each listing in the provided pageIndex.
        // If the provided pageIndex is the last page index, this method returns ListingData only
        // for the listings up until the last listing (inclusive). If the provided pageIndex is > than
        // the last page index, this method returns nothing.
        public IEnumerable<ListingData> GetListingDataForPage(int pageIndex)
        {
            HighestPageIndexRetrieved = pageIndex > HighestPageIndexRetrieved ? pageIndex : HighestPageIndexRetrieved;

            var numListingsBeforeThisPage = pageIndex * NUM_LISTINGS_PER_PAGE;
            for (var i = 0; i < NUM_LISTINGS_PER_PAGE; i++)
            {
                var listingIndex = numListingsBeforeThisPage + i;
                if (listingIndex >= savedListingData.Count && AllListingsWereRetrieved) yield break;

                yield return GetListingData(listingIndex);
            }
        }

        // Retrieve the ListingData for the provided listingIndex.
        // Throws an ArgumentOutOfRangeException if the provided listingIndex is out of range
        // (i.e., there are not (listingIndex + 1) listings in this user-session)
        public ListingData GetListingData(int listingIndex)
        {
            // Get item from saved listings before retrieving it from the db
            // If the listing isn't already saved, retrieve the listings from the db (via iteration) until 
            // we have retrieved the listing for the provided listingIndex or until there are no more items left
            var desiredItemIsCached = listingIndex < savedListingData.Count;
            while (!desiredItemIsCached)
            {
                if (AllListingsWereRetrieved) throw new ArgumentOutOfRangeException();

                var storeItem = allStoreItems.Current;
                var listingData = new ListingData(storeItem);
                savedListingData.Add(listingData);
                AllListingsWereRetrieved = !allStoreItems.MoveNext();
                desiredItemIsCached = listingIndex < savedListingData.Count;
            }

            return savedListingData[listingIndex];
        }

        private void ResetStoreItemEnumerator(bool includeOutOfStock = false)
        {
            allStoreItems?.Dispose();
            // The order of the store items retrieved here will be the order of the listings as displayed to the user
            allStoreItems = db.STORE_ITEMs.Where(item => includeOutOfStock || item.QuantityAvailable > 0)
                .OrderByDescending(item => item.QuantityAvailable)
                .GetEnumerator();
            AllListingsWereRetrieved = !allStoreItems.MoveNext();
        }

        // Refresh/update the listings' data based on the latest db data.
        public void RefreshListingsFromDb()
        {
            db.Refresh(RefreshMode.OverwriteCurrentValues, db.STORE_ITEMs);
            ResetStoreItemEnumerator();
            savedListingData.Clear();
            HighestPageIndexRetrieved = -1; // reset so that if the last page is now out of range, it isn't potentially revisited
        }

        // This class implements IDisposable as the STORE_ITEMs retrieved from the db must be disposed
        public void Dispose()
        {
            allStoreItems?.Dispose();
        }
    }
}