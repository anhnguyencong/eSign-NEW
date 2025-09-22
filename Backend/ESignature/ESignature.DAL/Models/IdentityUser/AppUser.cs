using Microsoft.AspNetCore.Identity;
using System;

namespace ESignature.DAL.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
    }
}