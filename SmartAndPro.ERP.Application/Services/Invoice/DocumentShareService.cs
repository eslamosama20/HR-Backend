using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.Invoice
{
    public class DocumentShareService : IDocumentShareService
    {
        private readonly IEnumerable<ShareHandler> _handlers;

        public DocumentShareService(IEnumerable<ShareHandler> handlers)
        {
            _handlers = handlers;
        }


        public async Task ShareDocumentAsync(byte[] document, string method, string receiver, string? subject = null, string? body = null, string? attachmentName = null, string? senderDisplayName = null)
        {
            if (document == null || document.Length == 0)
                throw new InvalidOperationException("Document could not be generated.");

            var handler = _handlers.FirstOrDefault(h => h.Name.Equals(method, 
                StringComparison.OrdinalIgnoreCase));

            if (handler == null)
                throw new NotSupportedException($"Share method '{method}' is not supported.");

            await handler.Share(document, receiver, subject, body, attachmentName, senderDisplayName);
        }
    }
}
