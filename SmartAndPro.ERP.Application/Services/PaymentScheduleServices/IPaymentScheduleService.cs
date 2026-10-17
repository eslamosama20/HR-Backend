using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.PaymentScheduleServices
{
    public interface IPaymentScheduleService
    {
        Task<PaymentScheduleResponseDto> CreateAsync(PaymentSchedulePostDto dto, Guid userId);
        Task<PaymentScheduleResponseDto> UpdateAsync(PaymentSchedulePutDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<PaymentScheduleResponseDto> GetByIdAsync(Guid id);
        Task<List<PaymentScheduleResponseDto>> GetByInvoiceAsync(short invoiceTypeId, int sourceInvoiceId);
        Task<List<PaymentScheduleResponseDto>> GetAllAsync();
    }
}
