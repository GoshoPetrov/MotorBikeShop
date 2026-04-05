using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ModelId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [ForeignKey(nameof(ModelId))]
        public Model Model { get; set; } = null!;
    }
}
