using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents stock information for a motorbike model.
    /// </summary>
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key referencing the BikeModel.
        /// </summary>
        [Required]
        public int BikeModelId { get; set; }

        /// <summary>
        /// Quantity of this model available in stock.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Navigation property – the motorbike model this inventory belongs to.
        /// </summary>
        [ForeignKey(nameof(BikeModelId))]
        public BikeModel BikeModel { get; set; } = null!;
    }
}
