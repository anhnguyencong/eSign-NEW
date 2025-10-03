using ESignature.Core.BaseDtos;
using ESignature.HashServiceLayer.Services.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace ESignature.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(ResponseDto<JwtTokenDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login([FromBody] AuthenticateQueryCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}