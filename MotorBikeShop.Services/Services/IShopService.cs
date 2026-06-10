using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorBikeShop.Services
{
    public interface IShopService
    {
        Task ConfirmPurchase();
        Task IncreaseBasketItemQuantity(int itemId, int delta = 1);
        Task<BasketItemViewModel> RemoveFromBasket(int itemId);
        Task<BasketViewModel> GetBasket();
        Task AddItemToBasket(int bikeModelId);
        Task<BikeViewModel> UpdateBike(BikeViewModel bikeViewModel);
        Task<BikeViewModel> GetBike(int id);
        Task<BikeViewModel> DeleteBike(int id);
        Task<BikeViewModel> GetBikeModelDetails(int id);
        Task<List<BikeViewModel>> GetBikeModelIndex();
        Task<BikeViewModel> GetShowcaseDetail(int id);

        /// <summary>
        /// Get the showcase with paging, sorting, and optional search filtering.
        /// </summary>
        /// <param name="searchString">Optional search term to filter by name or brand.</param>
        /// <param name="sortBy">Sort column: "Name", "Brand", "Price", or "Year". Defaults to "Name".</param>
        /// <param name="ascending">Whether sort is ascending.</param>
        /// <param name="pageNumber">Page number (1-based). Defaults to 1.</param>
        /// <param name="pageSize">Items per page. Defaults to 6, max 100.</param>
        Task<ShowcaseViewModel> GetShowcaseAsync(
            string? searchString,
            string? sortBy,
            bool ascending,
            int pageNumber,
            int pageSize);

        Task<List<BikeViewModel>> GetShowcase(string csv);
        Task<List<BikeViewModel>> BikeInventory();

        Task<List<BikeModel>> SearchForBikes(string term);

        Task<string> ExportBikesCsv();
        Task<bool> ImportBikesCsv(string csv);

        // ── Comments ──────────────────────────────────────────────────────────

        /// <summary>
        /// Updates a single field of a bike (and optionally its stock).
        /// Only the specified field is changed; all other fields remain untouched.
        /// </summary>
        /// <param name="bikeId">The ID of the bike to update.</param>
        /// <param name="field">Field name: "Name", "Brand", "Year", "Price", or "Stock".</param>
        /// <param name="value">The new value as a string (parsed server-side).</param>
        /// <returns>The updated BikeViewModel.</returns>
        /// <exception cref="ShopException">If the bike is not found or parsing fails.</exception>
        Task<BikeViewModel> UpdateBikeFieldAsync(int bikeId, string field, string value);

        /// <summary>
        /// Gets all comments for a specific bike, ordered by newest first.
        /// </summary>
        Task<List<CommentViewModel>> GetCommentsForBikeAsync(int bikeModelId, CancellationToken ct = default);

        /// <summary>
        /// Adds a comment from the current user on a bike.
        /// Throws ShopException if the bike does not exist.
        /// </summary>
        Task<CommentViewModel> AddCommentAsync(int bikeModelId, string content, CancellationToken ct = default);

        /// <summary>
        /// Deletes a comment by id.
        /// - A user can delete only their own comment.
        /// - An admin can delete any comment.
        /// Throws ShopException if the comment is not found.
        /// Throws UnauthorizedAccessException if the current user is neither the author nor an admin.
        /// </summary>
        Task DeleteCommentAsync(int commentId, CancellationToken ct = default);
    }
}
