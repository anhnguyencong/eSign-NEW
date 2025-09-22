using ESignature.Api.Messages;
using ESignature.Core.BaseDtos;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Authentications;
using ESignature.ServiceLayer.Services.Commands;
using ESignature.ServiceLayer.Services.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ESignature.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemaConstants.ValidateTokenSchema)]
    [Route("api/[controller]")]
    public class FileController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMessagePublisher _publisher;
        public FileController(IMediator mediator, IMessagePublisher publisher)
        {
            _mediator = mediator;
            _publisher = publisher;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(ResponseDto<bool>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Upload(string position, string pageSign, [FromForm] UploadFileCommand command)
        {
            var page = pageSign;
            if (!string.IsNullOrEmpty(position) && (string.IsNullOrEmpty(pageSign) || pageSign == "0"))
                page = "1";
            command.TokenKey = CurrentTokenKey;
            command.PageSign = page;
            command.VisiblePosition = position;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<IList<JobDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetFiles()
        {
            var command = new FileQueryCommand { TokenKey = CurrentTokenKey };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("Download/{id}")]
        [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Download(Guid id)
        {
            var command = new DownloadFileQueryCommand
            {
                Id = id,
                TokenKey = CurrentTokenKey
            };
            var result = await _mediator.Send(command);
            if (result.Result == null)
            {
                return NotFound();
            }
            return File(result.Result, "application/pdf", $"{id}.pdf");
        }
    }
}