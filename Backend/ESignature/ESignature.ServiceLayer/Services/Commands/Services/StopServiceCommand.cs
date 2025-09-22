using ESignature.Core.BaseDtos;
using ESignature.ServiceLayer.Services.OnStartup;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Services.Commands.Services
{
    public class StopServiceCommand : IRequest<ResponseDto<bool>>
    {
    }

    public class StopServiceCommandHandler : IRequestHandler<StopServiceCommand, ResponseDto<bool>>
    {
        private readonly ServiceData _service;

        public StopServiceCommandHandler(ServiceData service)
        {
            _service = service;
        }

        public async Task<ResponseDto<bool>> Handle(StopServiceCommand request, CancellationToken cancellationToken)
        {
            _service.IsStop = true;
            var response = new ResponseDto<bool>
            {
                Result = true
            };
            return await Task.FromResult(response);
        }
    }
}