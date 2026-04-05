using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
    public class VentItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VentId { get; set; }

        [Required]
        public int ModelId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [ForeignKey(nameof(VentId))]
        public Vent Vent { get; set; } = null!;

        [ForeignKey(nameof(ModelId))]
        public Model Model { get; set; } = null!;
    }
}
