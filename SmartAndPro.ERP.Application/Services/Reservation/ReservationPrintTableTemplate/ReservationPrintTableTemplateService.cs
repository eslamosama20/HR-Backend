using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Reflection;
using System.Text.Json;

namespace SmartAndPro.ERP.Application.Services.Reservation.ReservationPrintTableTemplate
{
    public class ReservationPrintTableTemplateService : ServiceBase
        <
        SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate,
        ReservationPrintTableTemplatePostDto,
        ReservationPrintTableTemplatePutDto,
        ReservationPrintTableTemplateResponseDto,
        ReservationPrintTableTemplateFilterDto
        >, IReservationPrintTableTemplateService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public ReservationPrintTableTemplateService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
            : base(repositoriesFactory, appSettings, mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        public override async Task<OperationResponse<List<ReservationPrintTableTemplateResponseDto>>> GetAllAsync(ReservationPrintTableTemplateFilterDto filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
        {
            var result = new OperationResponse<List<ReservationPrintTableTemplateResponseDto>>(ErrorCodes.GeneralFail, null);
            try
            {
                var operation = await _repositoriesFactory
                    .GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate>()
                    .GetAll();
                if (operation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<List<ReservationPrintTableTemplateResponseDto>>(operation.ErrorCode, null);
                var data = operation.Data!
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.CreatedAt)
                    .ToList();
                if (data.Count == 0)
                    return new OperationResponse<List<ReservationPrintTableTemplateResponseDto>>(ErrorCodes.NoContent, null);
                var responseData = _mapper.Map<List<ReservationPrintTableTemplateResponseDto>>(data);
                return new OperationResponse<List<ReservationPrintTableTemplateResponseDto>>(ErrorCodes.Success, responseData);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        public override async Task<OperationResponse<ReservationPrintTableTemplateResponseDto>> UpdateAsync(ReservationPrintTableTemplatePutDto request)
        {
            var result = new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.GeneralFail, null);
            try
            {
                var repository = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate>();

                // Get the existing entity to preserve audit fields
                var existingOperation = await repository.Filter(x => x.Id == request.Id);
                if (existingOperation.ErrorCode != ErrorCodes.Success || existingOperation.Data == null)
                {
                    return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.NotFound, null);
                }

                var existingEntity = await existingOperation.Data.AsNoTracking().FirstOrDefaultAsync();
                if (existingEntity == null)
                {
                    return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.NotFound, null);
                }

                // Create updated entity with new data
                var updatedEntity = new SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate();
                updatedEntity.Id = request.Id;
                updatedEntity.Name = request.Name;
                updatedEntity.Description = request.Description;
                updatedEntity.PageOrientation = Enum.Parse<PageOrientation>(request.PageOrientation, true);
                updatedEntity.DisplayOrder = request.DisplayOrder;

                // Add new columns
                foreach (var columnDto in request.Columns)
                {
                    updatedEntity.AddColumn(new SmartAndPro.ERP.Domains.ValueObjects.PrintTableColumn(
                        columnDto.ColumnName,
                        columnDto.DisplayName,
                        columnDto.DataType,
                        columnDto.OrderIndex,
                        columnDto.IsVisible,
                        columnDto.Width,
                        columnDto.Alignment
                    ));
                }

                // Use the custom repository method that handles deletion and recreation
                var customRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate>()
                    as SmartAndPro.ERP.Infrastructure.Repositories.Reservation.ReservationPrintTableTemplateRepository;

                OperationResponse<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate> operation;

                if (customRepo != null)
                {
                    operation = await customRepo.UpdateWithOwnedEntities(updatedEntity);
                }
                else
                {
                    // Fallback - this shouldn't happen with our factory setup
                    return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.GeneralFail, null);
                }

                if (operation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<ReservationPrintTableTemplateResponseDto>(operation.ErrorCode, null);

                var responseData = _mapper.Map<ReservationPrintTableTemplateResponseDto>(operation.Data);
                return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.Success, responseData);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.GeneralFail, null);
            }
        }

        public override async Task<OperationResponse<ReservationPrintTableTemplateResponseDto>> DeleteAsync(Guid id)
        {
            var result = new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.GeneralFail, null);
            try
            {
                // Use the custom repository method for hard delete
                var customRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate>()
                    as SmartAndPro.ERP.Infrastructure.Repositories.Reservation.ReservationPrintTableTemplateRepository;

                if (customRepo != null)
                {
                    var operation = await customRepo.HardDelete(id);

                    if (operation.ErrorCode == ErrorCodes.Success)
                        return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.NoContent, null);
                    else
                        return new OperationResponse<ReservationPrintTableTemplateResponseDto>(operation.ErrorCode, null);
                }
                else
                {
                    // Fallback - this shouldn't happen with our factory setup
                    return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.GeneralFail, null);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<ReservationPrintTableTemplateResponseDto>(ErrorCodes.GeneralFail, null);
            }
        }

        public async Task<OperationResponse<string>> SeedDefaultTemplatesAsync()
        {
            var result = new OperationResponse<string>(ErrorCodes.GeneralFail, string.Empty);
            try
            {
                // Load JSON file from application data directory
                var assembly = Assembly.GetExecutingAssembly();
                var jsonPath = Path.Combine(
                    Path.GetDirectoryName(assembly.Location)!,
                    "Data",
                    "DefaultReservationPrintTableTemplates.json"
                );

                if (!File.Exists(jsonPath))
                    return new OperationResponse<string>(ErrorCodes.NotFound, string.Empty);

                var jsonContent = await File.ReadAllTextAsync(jsonPath);
                var defaultTemplates = JsonSerializer.Deserialize<List<ReservationPrintTableTemplatePostDto>>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (defaultTemplates == null || defaultTemplates.Count == 0)
                    return new OperationResponse<string>(ErrorCodes.NoContent, string.Empty);

                // Get existing template names to avoid duplicates
                var existingOperation = await _repositoriesFactory
                    .GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate>()
                    .GetAll();

                var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (existingOperation.ErrorCode == ErrorCodes.Success && existingOperation.Data != null)
                {
                    existingNames = new HashSet<string>(
                        await existingOperation.Data.Select(x => x.Name).ToListAsync(),
                        StringComparer.OrdinalIgnoreCase
                    );
                }

                int insertedCount = 0;
                foreach (var templateDto in defaultTemplates)
                {
                    // Skip if template with same name already exists
                    if (existingNames.Contains(templateDto.Name))
                        continue;

                    var addResult = await AddAsync(templateDto);
                    if (addResult.ErrorCode == ErrorCodes.Success)
                        insertedCount++;
                }

                return new OperationResponse<string>(ErrorCodes.Success, insertedCount.ToString());
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<string>(ErrorCodes.GeneralFail, string.Empty);
            }
        }

    }
}
