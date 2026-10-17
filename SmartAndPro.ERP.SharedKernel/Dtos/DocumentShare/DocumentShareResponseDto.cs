using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare
{
    public class DocumentShareResponseDto <TId>
    {
        public TId Id { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string DownloadLink { get; set; } = string.Empty;
        public string WhatsAppLink { get; set; } = string.Empty;

    }
}
