using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SemesterProject
{
    // This class represents a collection of all store-items in use in a user-session
    class Listings // todo better name
    {
        private DataClasses1DataContext db;
        private CUSTOMER loggedInCustomer;
        private readonly int NUM_LISTINGS_PER_PAGE;
        private IEnumerator<STORE_ITEM> allStoreItems;

        private List<ListingData>
            cachedStoreItems = new List<ListingData>(); // todo naming inconsistent with allstoreitems

        public bool IsAnotherItem { get; private set; } // todo naming
        public int HighestPageIndexRetrieved { get; private set; }


        public Listings(DataClasses1DataContext db, CUSTOMER loggedInCustomer, int numListingsPerPage)
        {
            this.db = db;
            this.loggedInCustomer = loggedInCustomer;
            this.NUM_LISTINGS_PER_PAGE = numListingsPerPage;
            EnsureStoreItemsRetrieved();
        }


        // Return the ListingData for each listing in the provided pageIndex.
        // If the provided pageIndex is >= the last page index, this method returns ListingData only
        // for the listings up until the last one (inclusive).
        public IEnumerable<ListingData> GetListingDataForPage(int pageIndex)
        {
            HighestPageIndexRetrieved = pageIndex > HighestPageIndexRetrieved ? pageIndex : HighestPageIndexRetrieved;

            int numListingsBeforeThisPage = pageIndex * NUM_LISTINGS_PER_PAGE;
            for (int i = 0; i < NUM_LISTINGS_PER_PAGE; i++)
            {
                int listingIndex = numListingsBeforeThisPage + i;
                if (listingIndex >= cachedStoreItems.Count && !IsAnotherItem)
                {
                    yield break;
                }
                yield return GetListingData(listingIndex);
            }
        }

        // Retrieve the ListingData for the provided listingIndex.
        // Throws an ArgumentOutOfRangeException if the provided listingIndex is out of range
        // (i.e., there are not (listingIndex + 1) listings in allStoreItems)
        public ListingData GetListingData(int listingIndex) // todo naming confusing with getlistingSdata
        {
            EnsureStoreItemsRetrieved();

            // get item from cache before retrieving it from the db
            // if listing isn't already cached, retrieve the listings from the db (via iteration) until 
            // we have retrieved the listing for the provided listingIndex or until there are no more items left
            bool desiredItemIsCached = listingIndex < cachedStoreItems.Count;
            while (!desiredItemIsCached)
            {
                if (!IsAnotherItem)
                {
                    throw new ArgumentOutOfRangeException(); // todo, or return null?
                }

                STORE_ITEM storeItem = allStoreItems.Current;
                ListingData listingData = new ListingData(storeItem);
                cachedStoreItems.Add(listingData);
                IsAnotherItem = allStoreItems.MoveNext();
                desiredItemIsCached = listingIndex < cachedStoreItems.Count;
            }

            return cachedStoreItems[listingIndex];
        }

        private void EnsureStoreItemsRetrieved()
        {
            if (allStoreItems == null)
            {
                RefreshAllStoreItems();
                IsAnotherItem = allStoreItems.MoveNext();
            }
        }

        private void RefreshAllStoreItems(bool includeOutOfStock = false)
        {
            // The order of the store items retrieved here will be the order of the listings as displayed to the user
            // todo use a smarter ordering maybe? not just based on quantity
            allStoreItems = db.STORE_ITEMs.Where(item => includeOutOfStock || item.QuantityAvailable > 0)
                .OrderByDescending(item => item.QuantityAvailable)
                .GetEnumerator();
            // todo check if this is a safe use of enumerator - do we need to dispose of it manually?

            // IsAnotherItem = allStoreItems.MoveNext(); // todo uncomment?
        }
    }
}