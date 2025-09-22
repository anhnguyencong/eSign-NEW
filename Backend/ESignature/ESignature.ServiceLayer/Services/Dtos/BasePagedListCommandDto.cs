using System;

namespace ESignature.ServiceLayer.Services.Dtos
{
    public class BasePagedListCommandDto
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}