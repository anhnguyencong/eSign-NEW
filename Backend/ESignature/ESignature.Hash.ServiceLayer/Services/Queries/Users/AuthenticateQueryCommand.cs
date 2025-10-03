using ESignature.Core.BaseDtos;
using ESignature.DAL.Models;
using ESignature.HashServiceLayer.Services.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace ESignature.HashServiceLayer.Services.Queries.Users
{
    public class AuthenticateQueryCommand : IRequest<ResponseDto<TokenDto>>
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthenticateQueryCommandHandler : IRequestHandler<AuthenticateQueryCommand, ResponseDto<TokenDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthenticateQueryCommandHandler(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ResponseDto<TokenDto>> Handle(AuthenticateQueryCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseDto<TokenDto>();
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null)
            {
                var signinResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, true);
                
                if (signinResult.Succeeded)
                {
                    var token = await CreateJwtToken(user);
                    response.Result = token;
                }
                else
                {
                    if (signinResult.IsLockedOut)
                    {
                        response.Errors.Add(new ErrorDto
                        {
                            Code = 403,
                            Message = "Your account has been locked for security purposes."
                        });
                    }
                    else
                    {
                        response.Errors.Add(new ErrorDto
                        {
                            Code = 403,
                            Message = "Your user name or password is not correct. Please try again."
                        });
                    }
                }
            }
            else
            {
                response.Errors.Add(new ErrorDto
                {
                    Code = 403,
                    Message = "Your user name or password is not correct. Please try again."
                });
            }
            return response;
        }

        public Task<TokenDto> CreateJwtToken(AppUser user)
        {
            var model = new TokenDto
            {
                UserInfo = GetUserInfo(user)
            };

            return Task.FromResult(model);
        }

        private UserInfoDto GetUserInfo(AppUser user)
        {
            var result = new UserInfoDto
            {
                FullName = user.FullName
            };
            return result;
        }
    }
}