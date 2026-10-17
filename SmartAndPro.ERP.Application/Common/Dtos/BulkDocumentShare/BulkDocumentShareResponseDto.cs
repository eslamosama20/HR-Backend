using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.BulkDocumentShare
{
    public class BulkDocumentShareResponseDto<TId>
    {
        public int Total { get; set; }             
        public int SuccessCount { get; set; }     
        public int FailedCount { get; set; }       
        public List<DocumentShareResponseDto<TId>> Results { get; set; } = new();
    }
}
