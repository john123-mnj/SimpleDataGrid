# Performance Guidelines for SimpleDataGrid

This document outlines the performance characteristics and best practices for using `SimpleDataGrid` effectively, especially with large datasets.

## Time Complexity of Operations

| Operation        | Time Complexity (Worst Case) |
| :--------------- | :--------------------------- |
| `SetSource`      | O(N)                         |
| `AddFilter`      | O(N)                         |
| `SetFilter`      | O(N)                         |
| `RemoveFilter`   | O(N)                         |
| `ClearFilters`   | O(N)                         |
| `SetSearch`      | O(N * M)                     |
| `SetSearchAll`   | O(N * M)                     |
| `ClearSearch`    | O(N)                         |
| `SetSort`        | O(N log N)                   |
| `ClearSort`      | O(N)                         |
| `NextPage`       | O(P)                         |
| `PreviousPage`   | O(P)                         |
| `GoToPage`       | O(P)                         |
| `SetPageSize`    | O(P)                         |

*   **N**: Total number of items in the source collection.
*   **M**: Number of search selectors (properties being searched).
*   **P**: Page size (number of items per page).

## Guidance for Datasets

### Small Datasets (< 1,000 items)

For small datasets, `SimpleDataGrid` performs exceptionally well. All operations are fast, and users will experience a highly responsive UI. There are generally no specific performance optimizations needed for this scale.

### Medium Datasets (1,000 - 100,000 items)

`SimpleDataGrid` is designed to handle medium-sized datasets efficiently. However, some considerations can further improve performance:

*   **Filtering and Searching:** While `O(N)` and `O(N*M)` operations are acceptable, frequent changes to filters or search terms can lead to noticeable delays. Utilize the `debounceMilliseconds` parameter in `SetSearch` to prevent excessive re-filtering on every keystroke.
*   **Page Size:** A larger page size reduces the number of page changes but increases the rendering time for each page. A smaller page size makes page transitions faster but requires more frequent paging. Experiment with page sizes (e.g., 25, 50, 100) to find the optimal balance for your application and user experience.
*   **Complex Filters/Selectors:** Avoid overly complex filter predicates or search selectors that involve heavy computations, as these will directly impact the `O(N)` or `O(N*M)` operations.

### Large Datasets (> 100,000 items)

For very large datasets, `SimpleDataGrid` might experience performance limitations due to its in-memory processing nature. Consider the following alternatives and strategies:

*   **Server-Side Pagination/Filtering/Sorting:** For truly massive datasets, it is highly recommended to implement server-side processing. This means that filtering, sorting, and pagination operations are performed by a backend service (e.g., a database) and only the data for the current page is sent to the client. `SimpleDataGrid` can still be used as the UI component, but its `PagedCollection` would need to be adapted to fetch data from the server.
*   **Data Virtualization:** WPF's built-in UI virtualization for `DataGrid` can help with rendering large numbers of rows by only rendering the visible rows. Ensure your `DataGrid` is configured to use UI virtualization.
*   **Optimized Data Structures:** If in-memory processing is unavoidable, consider using more optimized data structures for your source collection if possible, though `IReadOnlyList<T>` is generally efficient.

## Memory Usage Characteristics

`SimpleDataGrid` keeps the entire filtered and sorted dataset in memory (`_filtered` collection). For very large datasets, this can lead to significant memory consumption. If memory becomes a concern, server-side processing or on-demand loading of data chunks should be considered.

## Benchmarks (Future Work)

Specific benchmarks for various dataset sizes and operation types will be added in future updates to provide more concrete performance metrics.
