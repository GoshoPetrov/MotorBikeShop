using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MotorBikeShop.Areas.Identity.Data.Entities;

namespace MotorBikeShop.Areas.Identity.Data;

// Add profile data for application users by adding properties to the MotorBikeShopUser class
public class MotorBikeShopUser : IdentityUser
{
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

    /// <summary>
    /// Navigation property – one-to-many relationship with Comment.
    /// A user can write many comments.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

