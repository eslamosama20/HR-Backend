using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.SystemSetting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.SystemSetting;


public class SystemSettingService
    : ServiceBase
    <
        Domains.Entities.SystemSetting,
        SystemSettingPostDto,
        SystemSettingPutDto,
        SystemSettingResponseDto,
        SystemSettingFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public SystemSettingService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    public async Task<string> GenerateReservationSerialAsync()
    {
        var systemSettingResponse = await _repositoriesFactory
                                          .GetRepository<Domains.Entities.SystemSetting>()
                                          .GetAll();  

        var systemSetting = systemSettingResponse.Data?.FirstOrDefault();
        if (systemSetting == null)
            throw new Exception("System settings not found.");

        long currentNumber = systemSetting.ReservationSerialStartNumber ?? 1;

        string serial = string.IsNullOrEmpty(systemSetting.ReservationSerialPrefix)
            ? currentNumber.ToString()
            : $"{systemSetting.ReservationSerialPrefix}{currentNumber}";

        systemSetting.ReservationSerialStartNumber = currentNumber + 1;

        await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().Update(systemSetting);

        return serial;
    }

    public async Task<string> PreviewReservationSerialAsync()
    {
        var repo = _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>();
        var systemSettingResponse = await repo.GetAll();
        var systemSetting = systemSettingResponse.Data?.FirstOrDefault();

        if (systemSetting == null)
            throw new Exception("System settings not found.");

        long currentNumber = systemSetting.ReservationSerialStartNumber ?? 1;

        string serial = string.IsNullOrEmpty(systemSetting.ReservationSerialPrefix)
            ? currentNumber.ToString()
            : $"{systemSetting.ReservationSerialPrefix}{currentNumber}";

        return serial; 
    }

}


