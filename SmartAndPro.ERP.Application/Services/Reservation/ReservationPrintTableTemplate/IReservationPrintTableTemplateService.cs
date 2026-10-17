using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.Services.Reservation.ReservationPrintTableTemplate
{
    public interface IReservationPrintTableTemplateService : IServiceBase
        <
        SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate,
        ReservationPrintTableTemplatePostDto,
        ReservationPrintTableTemplatePutDto,
        ReservationPrintTableTemplateResponseDto,
        ReservationPrintTableTemplateFilterDto
        >
    {
        Task<OperationResponse<string>> SeedDefaultTemplatesAsync();
    }
}
