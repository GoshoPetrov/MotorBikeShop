using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorBikeShop.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        /// <summary>
        /// Whether the current user is in the Admin role.
        /// </summary>
        bool IsAdmin { get; }
    }
}
