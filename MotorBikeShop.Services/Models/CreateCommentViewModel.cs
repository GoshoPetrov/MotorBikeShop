using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Models;

/// <summary>
/// View model for creating a new comment on a bike detail page.
/// </summary>
public class CreateCommentViewModel
{
    /// <summary>
    /// The bike model this comment belongs to.
    /// </summary>
    [Required]
    public int BikeModelId { get; set; }

    /// <summary>
    /// The text content of the comment.
    /// </summary>
    [Required(ErrorMessage = "Comment cannot be empty.")]
    [StringLength(500, ErrorMessage = "Comment must be at most 500 characters.")]
    public string Content { get; set; } = null!;
}