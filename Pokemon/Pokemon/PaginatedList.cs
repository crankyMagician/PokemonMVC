using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Pokemon
{

    // A paginated list of items of type T
    public class PaginatedList<T> : List<T>
    {
        // The index of the current page
        public int PageIndex { get; private set; }

        // The total number of pages
        public int TotalPages { get; private set; }

        // Constructs a new paginated list
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            // Set the current page index and the total number of pages
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Add the items to the list
            this.AddRange(items);
        }

        // Indicates whether there is a previous page
        public bool HasPreviousPage => PageIndex > 1;

        // Indicates whether there is a next page
        public bool HasNextPage => PageIndex < TotalPages;

        // Creates a new paginated list asynchronously
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            // Get the total number of items in the source
            var count = await source.CountAsync();

            // Get the items for the current page
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Return a new paginated list
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

}

