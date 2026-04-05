using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a motorbike model that is sold in the shop.
    /// </summary>
    public class BikeModel
    {
        /// <summary>
        /// Primary key – unique identifier for each motorbike model.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the motorbike model (e.g., CBR600RR).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Brand/manufacturer of the motorbike (e.g., Honda, Yamaha).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = null!;

        /// <summary>
        /// Production year of the motorbike.
        /// </summary>
        [Range(1900, 2100)]
        public int Year { get; set; }

        /// <summary>
        /// Price of the motorbike.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        /// <summary>
        /// Optional description/details about the motorbike.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property – one-to-one relationship with Inventory.
        /// Each model has exactly one inventory record.
        /// </summary>
        public Inventory? Inventory { get; set; }
    }
}
