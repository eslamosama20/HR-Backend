using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.UserNotifications
{
    public class UserNotificationResponseDto
    {
        public Guid Id { get; set; }
        public bool IsRead { get; set; }
        public bool IsIgnored { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
