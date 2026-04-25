using MotorBikeShop.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Models
{
    public class BasketViewModel
    {
        public int Id { get; set; }

        public BasketItemViewModel[] Items { get; set; } = [];

    }
}
