using Microsoft.AspNetCore.Identity;
using System;

namespace ESignature.DAL.Models
{
    public class AppUserLogin : IdentityUserLogin<Guid>
    {
        public virtual AppUser User { get; set; }
    }
}