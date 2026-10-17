using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notifications
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }           
        public string Title { get; set; }    
        public string Content { get; set; } 
        public string? Link { get; set; }             
        public NotificationType NotificationType { get; set; } 
        public DateTime CreatedAt { get; set; }      

        public bool IsRead { get; set; } = false;
        public bool IsIgnored { get; set; } = false;
        public Guid? UserNotificationId { get; set; }
    }
}
