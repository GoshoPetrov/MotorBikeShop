using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Areas.Identity.Data.Entities;

/// <summary>
/// Represents a completed order (sale) made by a user.
/// </summary>
public class Vent
{
    /// <summary>
    /// Primary key – unique identifier for the order.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key referencing the User who made the order.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Date and time when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total price of the entire order.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Navigation property – the user who made the order.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public MotorBikeShopUser User { get; set; } = null!;

    /// <summary>
    /// Navigation property – collection of items included in the order.
    /// One order can contain multiple items.
    /// </summary>
    public ICollection<VentItem> Items { get; set; } = new List<VentItem>();
}