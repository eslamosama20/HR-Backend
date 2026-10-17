using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.BulkDocumentShare
{
    public class BulkDocumentSharePostDto<TId>
    {
        public List<TId> Ids { get; set; } = new();
        public string Method { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
    }
}
