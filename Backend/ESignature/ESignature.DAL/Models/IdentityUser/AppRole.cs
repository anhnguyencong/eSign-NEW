using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;  

namespace ESignature.DAL.Models
{
    public class AppRole : IdentityRole<Guid>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public virtual ICollection<AppUserRole> UserRoles { get; set; }

        public virtual ICollection<AppRoleClaim> RoleClaims { get; set; }
    }
}