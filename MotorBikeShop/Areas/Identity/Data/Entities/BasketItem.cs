using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MotorBikeShop.Areas.Identity.Data.Entities;

/// <summary>
/// Represents a single item inside a shopping basket.
/// </summary>
public class BasketItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BasketId { get; set; }

    /// <summary>
    /// Foreign key referencing the BikeModel.
    /// </summary>
    [Required]
    public int BikeModelId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [ForeignKey(nameof(BasketId))]
    public Basket Basket { get; set; } = null!;

    /// <summary>
    /// Navigation property – the motorbike model in this basket item.
    /// </summary>
    [ForeignKey(nameof(BikeModelId))]
    public BikeModel BikeModel { get; set; } = null!;
}