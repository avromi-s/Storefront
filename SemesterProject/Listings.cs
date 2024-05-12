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

        public Listings(DataClasses1DataContext db, CUSTOMER loggedInCustomer, int numListingsPerPage)
        {
            this.db = db;
            this.loggedInCustomer = loggedInCustomer;
            this.NUM_LISTINGS_PER_PAGE = numListingsPerPage;
            EnsureStoreItemsRetrieved();
        }


        // Return StoreItemWrappers around the store_items for the provided page
        public IEnumerable<ListingData> GetListingsData(int pageIndex) // todo naming confusing with getlistingSdata
        {
            for (int i = 0; IsAnotherItem && i < NUM_LISTINGS_PER_PAGE; i++)
            {
                yield return GetListingData((pageIndex * NUM_LISTINGS_PER_PAGE) + i);
            }
        }

        // Retrieve the ListingData for the provided listingIndex.
        // Throws an ArgumentOutOfRangeException if the provided listingIndex is out of range
        // (i.e., there are not (listingIndex + 1) listings)
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

        /*/// <summary>
        /// Get the store items for the given page number.
        /// </summary>
        /// <param name="pageIndex">The page number to retrieve items for</param>
        /// <returns>An IEnumerable of the store items for the page</returns>
        private IEnumerable<STORE_ITEM> GetStoreItems(int pageIndex)
        {
            EnsureStoreItemsRetrieved();

            // get any items from cache before retrieving from the db
            int i = 0;
            bool desiredItemIsCached = (pageIndex * NUM_LISTINGS_PER_PAGE + i) < cachedStoreItems.Count;
            while (desiredItemIsCached && i < NUM_LISTINGS_PER_PAGE)
            {
                yield return cachedStoreItems[pageIndex * NUM_LISTINGS_PER_PAGE + i];
                i++;
                desiredItemIsCached = (pageIndex * NUM_LISTINGS_PER_PAGE + i) < cachedStoreItems.Count;
            }

            // retrieve the rest of the items from the db (unless we already retrieved all items
            // (i.e., i == NUM_LISTINGS_PER_PAGE), and/or until there are no more items left)
            for (; i < NUM_LISTINGS_PER_PAGE && IsAnotherItem; i++)
            {
                STORE_ITEM storeItem = allStoreItems.Current;
                cachedStoreItems
                    .Add(storeItem); // cache item before returning in case another method tries to retrieve it already
                IsAnotherItem =
                    allStoreItems
                        .MoveNext(); // also, update IsAnotherItem before return so that it is accurate regardless of if this loop finishes (which is dependent on how much the calling method iterates)
                yield return storeItem;
            }
        }*/

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