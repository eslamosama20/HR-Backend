using SmartAndPro.ERP.Application.Common.Dtos.Assets;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Assets;

namespace SmartAndPro.ERP.Application.Services.Assets
{
    public interface IAssetService: IServiceBase
        <
        Asset,
        AssetPostDto,
        AssetPutDto,
        AssetResponseDto,
        AssetFilterDto
        >
    {
    }
}
