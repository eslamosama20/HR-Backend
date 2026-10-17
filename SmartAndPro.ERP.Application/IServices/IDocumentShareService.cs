using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.IServices
{
    public interface IDocumentShareService
    {
        Task ShareDocumentAsync(byte[] document, string method, string receiver, string? subject = null, string? body = null, string? attachmentName = null, string? senderDisplayName = null);
    }
}
