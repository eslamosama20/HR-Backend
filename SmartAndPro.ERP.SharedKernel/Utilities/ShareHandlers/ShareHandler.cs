using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers
{
    public abstract class ShareHandler
    {
        public abstract string Name {  get; }

        public abstract Task Share(byte[] pdfBytes, string receiver);
        public virtual Task Share(byte[] pdfBytes, string receiver, string? subject, string? body, string? attachmentName)
        {
            return Share(pdfBytes, receiver);
        }

        public virtual Task Share(byte[] pdfBytes, string receiver, string? subject, string? body, string? attachmentName, string? senderDisplayName)
        {
            return Share(pdfBytes, receiver, subject, body, attachmentName);
        }
    }
}
