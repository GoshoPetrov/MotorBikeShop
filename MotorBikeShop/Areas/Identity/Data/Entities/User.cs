using NuGet.ContentModel;
using System.ComponentModel.DataAnnotations;

namespace MotorBikeShop.Areas.Identity.Data.Entities
{
    /// <summary>
    /// Represents a user (customer) of the motorbike shop.
    /// It has 1:1 relationship with MotorBikeShopUser which is 
    /// managed by ASP.Net Identity framework
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key – unique identifier for the user. 1:1 With MotorBikeShopUser
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Username used for login.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Email address of the user.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Hashed password (never store plain text passwords).
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Navigation property – one-to-one relationship with Basket.
        /// Each user has one shopping basket.
        /// </summary>
        public Basket? Basket { get; set; }

        /// <summary>
        /// Navigation property – one-to-many relationship with Vent (orders).
        /// A user can have multiple orders.
        /// </summary>
        public ICollection<Vent> Vents { get; set; } = new List<Vent>();
    }
}
