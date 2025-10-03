using ESignature.Core.BaseDtos;
using ESignature.DAL;
using ESignature.HashServiceLayer.Authentications;
using ESignature.HashServiceLayer.Services.Commands.Services;
using ESignature.HashServiceLayer.Services.OnStartup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace ESignature.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemaConstants.ValidateTokenSchema, Roles = DalConstants.RoleAdmin)]
    [Route("api/[controller]")]
    public class ServiceController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ServiceData _service;

        public ServiceController(IMediator mediator, ServiceData service)
        {
            _mediator = mediator;
            _service = service;
        }

        [HttpGet]
        [Route("Status")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public IActionResult GetStatusAsync()
        {
            var result = new ResponseDto<bool>
            {
                Result = _service.IsStop
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("Start")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> StartServiceAsync()
        {
            var command = new StartServiceCommand();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost]
        [Route("Stop")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> StopServiceAsync()
        {
            var command = new StopServiceCommand();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}