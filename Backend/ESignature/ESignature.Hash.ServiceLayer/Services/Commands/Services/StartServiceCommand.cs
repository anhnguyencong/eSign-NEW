using ESignature.Core.BaseDtos;
using ESignature.HashServiceLayer.Services.OnStartup;
using MediatR;

namespace ESignature.HashServiceLayer.Services.Commands.Services
{
    public class StartServiceCommand : IRequest<ResponseDto<bool>>
    {
    }

    public class StartServiceCommandHandler : IRequestHandler<StartServiceCommand, ResponseDto<bool>>
    {
        private readonly ServiceData _service;

        public StartServiceCommandHandler(ServiceData service)
        {
            _service = service;
        }

        public async Task<ResponseDto<bool>> Handle(StartServiceCommand request, CancellationToken cancellationToken)
        {
            _service.IsStop = false;
            var response = new ResponseDto<bool>
            {
                Result = true
            };
            return await Task.FromResult(response);
        }
    }
}