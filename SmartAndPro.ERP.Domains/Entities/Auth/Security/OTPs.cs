using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Security
{
    public class OTPs
    {
        public string OTPReferenceKey { get; set; }
        public Guid OTPReference { get; set; }
        public string OTPPayload { get; set; }
        public short NumberOfFailedTrails { get; set; }
        public DateTime ExpiryDate { get; set; }
        [ForeignKey("OTPStatuses")]
        public short? StatusID { get; set; }
        [ForeignKey("OTPType")]
        public short? OtpTypeID { get; set; }
        public OTPType OTPType { get; set; }
        public OTPStatuses OTPStatuses { get; set; }
    }
}
