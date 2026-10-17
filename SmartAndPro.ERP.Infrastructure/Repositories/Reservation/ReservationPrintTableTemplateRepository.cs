using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Infrastructure.Repositories.Reservation
{
    public class ReservationPrintTableTemplateRepository : RepositoryBase<ReservationPrintTableTemplate, ERPContext>
    {
        private readonly ERPContext _context;
        private readonly ILogger<ReservationPrintTableTemplate> _logger;

        public ReservationPrintTableTemplateRepository(ERPContext context) : base(context)
        {
            _context = context;
            _logger = new LoggerFactory().CreateLogger<ReservationPrintTableTemplate>();
        }

        public async Task<OperationResponse<ReservationPrintTableTemplate>> UpdateWithOwnedEntities(ReservationPrintTableTemplate entity)
        {
            var response = new OperationResponse<ReservationPrintTableTemplate>(ErrorCodes.GeneralFail, entity);
            try
            {
                // Step 1: Delete all existing owned entities (PrintTableColumn) first
                // This is the key to avoiding the foreign key modification error
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM PrintTableColumn WHERE ReservationPrintTableTemplateId = {0}",
                    entity.Id);

                // Step 2: Update the main entity properties
                var existingEntity = await _context.ReservationPrintTableTemplates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (existingEntity == null)
                {
                    response.ErrorCode = ErrorCodes.NotFound;
                    return response;
                }

                // Step 3: Update the main entity using ExecuteSqlInterpolated for better null handling
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE ReservationPrintTableTemplates SET Name = {entity.Name}, Description = {entity.Description}, PageOrientation = {entity.PageOrientation}, DisplayOrder = {entity.DisplayOrder} WHERE Id = {entity.Id}");

                // Step 4: Insert new owned entities (PrintTableColumn)
                foreach (var column in entity.Columns)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"INSERT INTO PrintTableColumn 
                          (ReservationPrintTableTemplateId, ColumnName, DisplayName, DataType, OrderIndex, IsVisible, Width, Alignment) 
                          VALUES ({entity.Id}, {column.ColumnName}, {column.DisplayName}, {column.DataType}, {column.OrderIndex}, {column.IsVisible}, {column.Width}, {column.Alignment})");
                }
                response.Data = entity;
                response.ErrorCode = ErrorCodes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, entity);
                LogService.LogError(ex);
                response.ErrorCode = ErrorCodes.GeneralFail;
            }

            return response;
        }

        public async Task<OperationResponse<ReservationPrintTableTemplate>> HardDelete(Guid id)
        {
            var response = new OperationResponse<ReservationPrintTableTemplate>(ErrorCodes.GeneralFail, null);
            try
            {
                // Step 1: Delete all owned entities (PrintTableColumn) first
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM PrintTableColumn WHERE ReservationPrintTableTemplateId = {0}", id);

                // Step 2: Delete the main entity
                var deletedRows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM ReservationPrintTableTemplates WHERE Id = {0}", id);

                if (deletedRows > 0)
                {
                    response.ErrorCode = ErrorCodes.Success;
                }
                else
                {
                    response.ErrorCode = ErrorCodes.NotFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, id);
                LogService.LogError(ex);
                response.ErrorCode = ErrorCodes.GeneralFail;
            }

            return response;
        }
    }
}