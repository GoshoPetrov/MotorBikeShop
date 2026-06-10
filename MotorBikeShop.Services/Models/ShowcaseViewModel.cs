using MotorBikeShop.Models;

namespace MotorBikeShop.Models;

/// <summary>
/// View model for the Showcase page — contains the list of bikes plus paging and sorting metadata.
/// </summary>
public class ShowcaseViewModel
{
    /// <summary>
    /// Bikes on the current page.
    /// </summary>
    public IReadOnlyList<BikeViewModel> Bikes { get; init; } = Array.Empty<BikeViewModel>();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of bikes per page.
    /// </summary>
    public int PageSize { get; init; } = 6;

    /// <summary>
    /// Total number of matching items (before paging).
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Current sort column: "Name", "Brand", "Price", or "Year".
    /// </summary>
    public string SortBy { get; init; } = "Name";

    /// <summary>
    /// Whether the sort direction is ascending.
    /// </summary>
    public bool Ascending { get; init; } = true;

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// 1-based index of the first item on this page.
    /// </summary>
    public int FirstItemIndex => (PageNumber - 1) * PageSize + 1;

    /// <summary>
    /// 1-based index of the last item on this page.
    /// </summary>
    public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalItems);
}