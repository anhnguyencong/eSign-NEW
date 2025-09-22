using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Services.Dtos;
using ESignature.ServiceLayer.Services.OnStartup;
using ESignature.ServiceLayer.Settings;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands
{
    public class SeedDataCommand : IRequest<bool> { }

    public class SeedDataCommandHandler : IRequestHandler<SeedDataCommand, bool>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApiSourceData _apiSourceData;

        public SeedDataCommandHandler(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager,
            IWebHostEnvironment webHostEnvironment, ApiSourceData apiSourceData)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _apiSourceData = apiSourceData;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<bool> Handle(SeedDataCommand request, CancellationToken cancellationToken)
        {
            await SeedSigners();
            await SeedApplications();
            await SeedRoles();
            await SeedUsers();

            // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
            await SeedListBranches();
            return true;
        }

        // danh sách fullname branch kí để kiểm soát việc xuống dòng của tên khi kí
        private async Task SeedListBranches()
        {
            try
            {
                using (var apps = new StreamReader(string.Format("{0}/config/{1}", _webHostEnvironment.WebRootPath, "listbranch.json")))
                {
                    var jsonString = await apps.ReadToEndAsync();
                    var d = JsonConvert.DeserializeObject<BranchesSetting>(jsonString);
                    _apiSourceData.SetBranchesSetting(d.Branches);
                }
            }
            catch
            {
                _apiSourceData.SetBranchesSetting(new List<Branch>());
            }

        }
        private async Task SeedSigners()
        {
            using (var apps = new StreamReader(string.Format("{0}/config/{1}", _webHostEnvironment.WebRootPath, "signer.json")))
            {
                var jsonString = await apps.ReadToEndAsync();
                var d = JsonConvert.DeserializeObject<SignerSetting>(jsonString);
                _apiSourceData.SetSigners(d.Signers);
            }
        }

        private async Task SeedApplications()
        {
            using (var apps = new StreamReader(string.Format("{0}/config/{1}", _webHostEnvironment.WebRootPath, "app.json")))
            {
                var jsonString = await apps.ReadToEndAsync();
                var d = JsonConvert.DeserializeObject<ApiSourceDto>(jsonString);
                _apiSourceData.SetSources(d.Apps);
            }
        }

        private async Task SeedUsers()
        {
            //admin
            var admin = new AppUser
            {
                Email = "admin@gic.com.vn",
                UserName = "admin@gic.com.vn",
                FullName = "System Admin",
                EmailConfirmed = true
            };
            await CreateUser(admin, RoleConstants.Admin);

            //super admin user
            var superAdmin = new AppUser
            {
                UserName = DalConstants.SuperAdminEmail,
                Email = DalConstants.SuperAdminEmail,
                FullName = "Super Admin",
                EmailConfirmed = true
            };
            await CreateUser(superAdmin, RoleConstants.SuperAdmin);
        }

        private async Task CreateUser(AppUser model, string roleName)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user != null)
                return;

            string password = DalConstants.DefaultPassword;
            if (roleName == RoleConstants.SuperAdmin)
            {
                password = DalConstants.SuperAdminPassword;
            }

            var createUser = await _userManager.CreateAsync(model, password);
            if (createUser.Succeeded)
            {
                await _userManager.AddToRoleAsync(model, roleName);
                await _userManager.AddClaimAsync(model, new Claim(ClaimTypes.Role, roleName));
            }
        }

        private async Task SeedRoles()
        {
            if (await _roleManager.Roles.AnyAsync() == false)
            {
                //creating roles
                var roles = new List<AppRole>
                {
                    new AppRole {Name = RoleConstants.SuperAdmin, Title = "Product Admin"},
                    new AppRole {Name = RoleConstants.Admin, Title = "System Admin"}
                };

                foreach (var t in roles)
                {
                    var roleExist = await _roleManager.RoleExistsAsync(t.Name);
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(t);
                    }
                }
            }
        }
    }
}