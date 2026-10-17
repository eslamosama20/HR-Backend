using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.FingerprintDeviceSettings;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace SmartAndPro.ERP.Application.Services;


public class FingerprintDeviceSettingsService
    : ServiceBase
    <
        FingerprintDeviceSettings,
        FingerprintDeviceSettingsPostDto,
        FingerprintDeviceSettingsPutDto,
        FingerprintDeviceSettingsResponseDto,
        ParamFilter
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public FingerprintDeviceSettingsService(RepositoriesFactory repositoriesFactory, AppSettings appSettings,
        IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<FingerprintDeviceSettingsResponseDto>> AddAsync(
        FingerprintDeviceSettingsPostDto fingerprintDeviceSettings)
    {
        var isExistsQuery = await _repositoriesFactory.GetRepository<FingerprintDeviceSettings>().GetAll();
        var isExists = isExistsQuery.Data.Any();
        if (isExists)
        {
            throw new Exception("FingerprintDeviceSettings already exists");
        }

        var result = await base.AddAsync(fingerprintDeviceSettings);
        return result;
    }

}

