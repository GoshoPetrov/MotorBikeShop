using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MotorBikeShop.Areas.Identity.Data.Entities;

/// <summary>
/// Represents a comment left by a user on a bike model's detail page.
/// </summary>
public class Comment
{
    /// <summary>
    /// Primary key – unique identifier for the comment.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key referencing the bike model this comment belongs to.
    /// </summary>
    [Required]
    public int BikeModelId { get; set; }

    /// <summary>
    /// Foreign key referencing the user who authored this comment.
    /// </summary>
    [Required]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// The text content of the comment. Max 500 characters.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = null!;

    /// <summary>
    /// UTC timestamp of when the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ──────────────────────────────────────────────

    /// <summary>
    /// The bike model this comment belongs to.
    /// </summary>
    [ForeignKey(nameof(BikeModelId))]
    public BikeModel BikeModel { get; set; } = null!;

    /// <summary>
    /// The user who authored this comment.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public MotorBikeShopUser User { get; set; } = null!;
}