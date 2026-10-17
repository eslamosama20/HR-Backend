using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Assets;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Assets
{
    public class AssetService : ServiceBase
        <
        Asset,
        AssetPostDto,
        AssetPutDto,
        AssetResponseDto,
        AssetFilterDto
        >, IAssetService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public AssetService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) : base(repositoriesFactory, appSettings, mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        public async Task<Asset> AddAsset(Asset asset)
        {
            var assetRepo = _repositoriesFactory.GetRepository<Asset>();
            var response = await assetRepo.Add(asset);

            if (response == null) return null;

            return asset;
        }
    }
}
