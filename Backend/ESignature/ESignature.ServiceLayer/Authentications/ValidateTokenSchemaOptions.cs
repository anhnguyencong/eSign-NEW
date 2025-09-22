using ESignature.DAL;
using ESignature.ServiceLayer.Services.OnStartup;
using ESignature.ServiceLayer.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Authentications
{
    public static class AuthenticationSchemaConstants
    {
        public const string ValidateTokenSchema = "ValidateToken";
    }

    public class ValidateTokenSchemaOptions : AuthenticationSchemeOptions
    {
    }

    public class ValidateTokenSchemaOptionsHandler : AuthenticationHandler<ValidateTokenSchemaOptions>
    {
        private readonly ApiSourceData _apiSourceData;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isIPAuthentication;

        public ValidateTokenSchemaOptionsHandler(IOptionsMonitor<ValidateTokenSchemaOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ApiSourceData apiSourceData,
            IOptions<ESignatureSetting> esoptions,
            IHttpContextAccessor httpContextAccessor) : base(options, logger, encoder, clock)
        {
            _apiSourceData = apiSourceData;
            _httpContextAccessor = httpContextAccessor;
            _isIPAuthentication = esoptions.Value.IPAddressAuthentication;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var tokenKey = "ES-Token";
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            // Validate header
            if (!Request.Headers.ContainsKey(tokenKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Header Not Found"));
            }

            var key = Request.Headers[tokenKey].ToString();
            if (!string.IsNullOrEmpty(key))
            {
                var app = _apiSourceData.Sources.SingleOrDefault(q => q.Key == key);
                if (app == null)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Client certificate required"));
                }
                else
                {
                    if (_isIPAuthentication && app.IpAddress != ipAddress)
                    {
                        return Task.FromResult(AuthenticateResult.Fail("IP address rejected"));
                    }
                }

                string roleName = DalConstants.RoleUser;
                if (app.Name.ToLower().Contains("admin"))
                {
                    roleName = DalConstants.RoleAdmin;
                }

                var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, app.Key.ToString()),
                    new Claim(ClaimTypes.Name, app.Name),
                    new Claim(ClaimTypes.Role, roleName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, nameof(ValidateTokenSchemaOptionsHandler));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Model is Empty"));
        }
    }

    public class TokenModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}