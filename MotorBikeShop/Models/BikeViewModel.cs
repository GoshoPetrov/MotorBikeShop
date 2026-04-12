using MotorBikeShop.Areas.Identity.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Models
{
    public class BikeViewModel
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string Brand { get; set; } = null!;
        public int Year { get; set; }

        public decimal Price { get; set; }


        public string? Description { get; set; }

        public int? InventoryQuantity { get; set; }

        public string ImageUrl { get; set; }
    }
}
