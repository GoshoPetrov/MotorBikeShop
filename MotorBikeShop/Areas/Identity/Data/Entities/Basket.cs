using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Areas.Identity.Data.Entities;

/// <summary>
/// Represents a shopping cart for a user.
/// </summary>
public class Basket
{
    /// <summary>
    /// Primary key – unique identifier for the basket.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key referencing the User who owns this basket.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property – the user who owns this basket.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public MotorBikeShopUser User { get; set; } = null!;

    /// <summary>
    /// Navigation property – collection of items in the basket.
    /// One basket can contain multiple items.
    /// </summary>
    public ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();
}