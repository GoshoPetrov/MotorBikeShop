using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
}

