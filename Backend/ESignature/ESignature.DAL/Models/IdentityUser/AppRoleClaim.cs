using Microsoft.AspNetCore.Identity;
using System;

namespace ESignature.DAL.Models
{
    public class AppRoleClaim : IdentityRoleClaim<Guid>
    {
        public virtual AppRole Role { get; set; }
    }
}