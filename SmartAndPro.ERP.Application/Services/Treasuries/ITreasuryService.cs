using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.Services.Treasuries
{
    public interface ITreasuryService : IServiceBase
        <
        Treasury,
        TreasuryPostDto,
        TreasuryPutDto,
        TreasuryResponseDto,
        TreasuryFiltersDto
        >
    {
        Task<OperationResponse<List<TreasuryResponseDto>>> GetAllFrontalAsync();
        Task<OperationResponse<TreasuryResponseDto>> MakeFrontalAsync(Guid treasuryId);
        Task<TreasuryImportResultDto> ImportFromExcelAsync(Stream fileStream);
    }
}
