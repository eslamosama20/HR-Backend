using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.RequestForQuotations;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.RequestForQuotations
{
    public class RequestForQuotationService : ServiceBase
        <
        RequestForQuotation,
        RequestForQuotationPostDto,
        RequestForQuotationPutDto,
        RequestForQuotationResponseDto,
        RequestForQuotationFilterDto
        >
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        public RequestForQuotationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory) : base(repositoriesFactory, appSettings, mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
        }
    


        public override async Task<OperationResponse<RequestForQuotationResponseDto>> UpdateAsync(RequestForQuotationPutDto dto)
        {
            var result = new OperationResponse<RequestForQuotationResponseDto>(ErrorCodes.GeneralFail, null);

            try
            {
                var repository = _repositoriesFactory.GetRepository<RequestForQuotation>();
                var itemRepository = _repositoriesFactory.GetRepository<RequestForQuotationItem>();


                var existingQuotationOperation = await repository.Filter(x => x.Id == dto.Id);

                if (existingQuotationOperation.ErrorCode != ErrorCodes.Success || existingQuotationOperation.Data == null)
                {
                    return new OperationResponse<RequestForQuotationResponseDto>(ErrorCodes.NotFound, null);
                }
                var existingQuotation = await existingQuotationOperation.Data
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync();

                if (existingQuotation == null)
                {
                    return new OperationResponse<RequestForQuotationResponseDto>(ErrorCodes.NotFound, null);
                }

                foreach (var itemDto in dto.Items)
                {
                    if (itemDto.Quantity <= 0)
                    {
                        return new OperationResponse<RequestForQuotationResponseDto>(
                            ErrorCodes.Validation,
                            null,
                            message:$"Quantity of the item :{itemDto.ItemName} must be more than 0"
                        );
                    }
                    if (itemDto.Id != Guid.Empty)
                    {

                        var existingItem = existingQuotation.Items
                        .FirstOrDefault(i => i.Id == itemDto.Id);

                        if (existingItem != null)
                        {
                            existingItem.ItemName = itemDto.ItemName;
                            existingItem.Quantity = itemDto.Quantity;

                            await itemRepository.Update(existingItem);

                        }
                    }
                    else
                    {
                        if (existingQuotation.Items.Any(i => i.ItemName.Equals(itemDto.ItemName, StringComparison.OrdinalIgnoreCase)))
                        {
                            return new OperationResponse<RequestForQuotationResponseDto>(
                                ErrorCodes.Validation,
                                null,
                                message: $"The item : {itemDto.ItemName} is already exists on the current request with id {existingQuotation.Id}");
                        }
                        var newItem = new RequestForQuotationItem
                        {
                            ItemName = itemDto.ItemName,
                            Quantity = itemDto.Quantity,
                            RequestForQuotationId = existingQuotation.Id
                        };

                        await itemRepository.Add(newItem);
                        existingQuotation.Items.Add(newItem);
                    }
                }

                    
             
                

                var updateOperation = await repository.Update(existingQuotation);
                        if (updateOperation.ErrorCode != ErrorCodes.Success)
                            return new OperationResponse<RequestForQuotationResponseDto>(updateOperation.ErrorCode, null);


                        var responseDto = _mapper.Map<RequestForQuotationResponseDto>(existingQuotation);
                        result = new OperationResponse<RequestForQuotationResponseDto>(ErrorCodes.Success, responseDto);
                    
                
            }

            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return result;
        }
    }
}
