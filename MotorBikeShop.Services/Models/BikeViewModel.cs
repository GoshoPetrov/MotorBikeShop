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

        /// <summary>
        /// Comments on this bike, ordered by newest first.
        /// </summary>
        public IReadOnlyList<CommentViewModel> Comments { get; set; } = Array.Empty<CommentViewModel>();
    }
}
