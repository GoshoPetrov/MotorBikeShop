using NuGet.ContentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
    public class BasketItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BasketId { get; set; }

        [Required]
        public int ModelId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [ForeignKey(nameof(BasketId))]
        public Basket Basket { get; set; } = null!;

        [ForeignKey(nameof(ModelId))]
        public Model Model { get; set; } = null!;
    }
}
