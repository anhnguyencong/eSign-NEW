using Microsoft.AspNetCore.Identity;
using System;

namespace ESignature.DAL.Models
{
    public class AppUserRole : IdentityUserRole<Guid>
    {
        public virtual AppUser User { get; set; }

        public virtual AppRole Role { get; set; }
    }
}