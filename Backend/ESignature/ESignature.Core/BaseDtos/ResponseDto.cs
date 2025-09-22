using System.Collections.Generic;
using System.Linq;

namespace ESignature.Core.BaseDtos
{
    public sealed class ResponseDto<T>
    {
        public ResponseDto()
        {
            Errors = new List<ErrorDto>();
        }

        public T Result { get; set; }

        public bool Success => !Errors.Any();

        public List<ErrorDto> Errors { get; set; }
    }

    public sealed class ErrorDto
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}