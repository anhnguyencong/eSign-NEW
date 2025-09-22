using Microsoft.AspNetCore.Identity;
using System;

namespace ESignature.DAL.Models
{
    public class AppUserToken : IdentityUserToken<Guid>
    {
        public virtual AppUser User { get; set; }
    }
}