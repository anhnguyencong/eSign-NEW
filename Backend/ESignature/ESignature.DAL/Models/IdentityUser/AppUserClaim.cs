using Microsoft.AspNetCore.Identity;
using System;

namespace ESignature.DAL.Models
{
    public class AppUserClaim : IdentityUserClaim<Guid>
    {
        public virtual AppUser User { get; set; }
    }
}