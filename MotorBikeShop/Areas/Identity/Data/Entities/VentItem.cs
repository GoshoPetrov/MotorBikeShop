using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
   
    /// <summary>
    /// Represents a single item within an order.
    /// </summary>
    public class VentItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VentId { get; set; }

        /// <summary>
        /// Foreign key referencing the BikeModel.
        /// </summary>
        [Required]
        public int BikeModelId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [ForeignKey(nameof(VentId))]
        public Vent Vent { get; set; } = null!;

        /// <summary>
        /// Navigation property – the motorbike model in this order item.
        /// </summary>
        [ForeignKey(nameof(BikeModelId))]
        public BikeModel BikeModel { get; set; } = null!;
    }
}
