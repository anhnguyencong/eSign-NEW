using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ESignature.Api.Controllers
{
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {
        protected string CurrentTokenKey
        {
            get
            {
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        protected string CurrentAppName
        {
            get
            {
                return User.FindFirst(ClaimTypes.Name)?.Value;
            }
        }
    }
}