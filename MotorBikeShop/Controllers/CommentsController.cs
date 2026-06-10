using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorBikeShop.Models;
using MotorBikeShop.Services;

namespace MotorBikeShop.Controllers;

/// <summary>
/// Handles comment creation and deletion on bike detail pages.
/// All actions require authentication.
/// </summary>
[Authorize]
public class CommentsController : Controller
{
    private readonly IShopService _shopService;

    public CommentsController(IShopService shopService)
    {
        _shopService = shopService;
    }

    /// <summary>
    /// POST: /Comments/Create
    /// Adds a new comment to a bike.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCommentViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid comment.";
            return RedirectToAction("Details", "Showcase", new { id = model.BikeModelId });
        }

        try
        {
            await _shopService.AddCommentAsync(model.BikeModelId, model.Content, ct);
        }
        catch (ShopException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Showcase", new { id = model.BikeModelId });
    }

    /// <summary>
    /// POST: /Comments/Delete/{id}
    /// Deletes a comment. Only the author or an admin can delete.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int bikeModelId, CancellationToken ct)
    {
        try
        {
            await _shopService.DeleteCommentAsync(id, ct);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You do not have permission to delete this comment.";
        }
        catch (ShopException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Showcase", new { id = bikeModelId });
    }
}