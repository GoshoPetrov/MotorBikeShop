using MotorBikeShop.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Models
{
    public class BasketViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public MotorBikeShopUser User { get; set; } = null!;
        public ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
