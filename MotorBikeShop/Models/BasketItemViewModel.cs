using MotorBikeShop.Areas.Identity.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Models
{
    public class BasketItemViewModel
    {
        public int Id { get; set; }

        public int BikeModelId { get; set; }

        public int Quantity { get; set; }

    }
}
