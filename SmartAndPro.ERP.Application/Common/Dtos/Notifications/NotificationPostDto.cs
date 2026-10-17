using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notifications
{
    public class NotificationPostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Link { get; set; }
        public NotificationType NotificationType { get; set; } 


    }
}
