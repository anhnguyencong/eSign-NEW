using ESignature.Core.BaseDtos;
using ESignature.Core.Infrastructure.Collections;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Authentications;
using ESignature.ServiceLayer.Services.Commands.Jobs;
using ESignature.ServiceLayer.Services.Dtos;
using ESignature.ServiceLayer.Services.OnStartup;
using ESignature.ServiceLayer.Services.Queries.Jobs;
using ESignature.ServiceLayer.Services.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ESignature.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemaConstants.ValidateTokenSchema, Roles = DalConstants.RoleAdmin)]
    [Route("api/[controller]")]
    public class JobController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ApiSourceData _apiSource;

        public JobController(IMediator mediator, ApiSourceData apiSource)
        {
            _mediator = mediator;
            _apiSource = apiSource;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<JobMonitorDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] JobQueryCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        [Route("BatchIds")]
        [ProducesResponseType(typeof(ResponseDto<IList<DropListDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBatchIdsAsync([FromQuery] JobBatchIdQueryCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        [Route("SourceName")]
        [ProducesResponseType(typeof(ResponseDto<IList<DropListDto>>), (int)HttpStatusCode.OK)]
        public IActionResult GetSourceName()
        {
            var listSource = new List<DropListDto>();
            var sources = _apiSource.Sources.Where(q => !q.Name.ToLower().Contains("admin"));
            var sources1 = _apiSource.Sources;
            foreach (var item in sources)
            {
                listSource.Add(new DropListDto { Key = item.Key, Value = item.Name });
            }
            var result = new ResponseDto<IList<DropListDto>>();
            result.Result = listSource;
            return Ok(result);
        }

        [HttpPost]
        [Route("Priority")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SetPriorityAsync([FromBody] UpdateJobPriorityCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost]
        [Route("Retry/{id}")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RetryAsync(string id)
        {
            var command = new RetryJobCommand { JobId = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost]
        [Route("RetryByBatchId")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RetryJobByBatchIdAsync([FromBody] RetryJobByBatchIdCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost]
        [Route("RetryCallback/{id}")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RetryCallbackAsync(string id)
        {
            var command = new RetryCallbackCommand { JobId = id };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost]
        [Route("RetryCallbackByBatchId")]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RetryCallbackByBatchIdAsync([FromBody] RetryCallbackByBatchIdCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}