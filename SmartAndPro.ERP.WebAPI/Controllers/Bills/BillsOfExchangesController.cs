using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Exchange;
using SmartAndPro.ERP.Application.Services.Assets;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Bills.BillOfExchange;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Bills.Exchange;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using SmartAndPro.ERP.WebAPI.Hellper;

namespace SmartAndPro.ERP.WebAPI.Controllers.Bills
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillsOfExchangesController : ControllerBase
    {
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly BillOfExchangeService _service;
        private readonly AssetService _assetService;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ICloudinaryService _cloudinaryService;
        private const string inv = "Bill-";

        public BillsOfExchangesController(
            ServiceBaseFactory serviceBaseFactory,
            IMapper mapper,
            RepositoriesFactory repositoriesFactory,
            ICloudinaryService cloudinaryService)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _service = _serviceBaseFactory.GetService<BillOfExchangeService>();
            _assetService = _serviceBaseFactory.GetService<AssetService>();
            _mapper = mapper;
            _repositoriesFactory = repositoriesFactory;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() 
        {
            var response = await _service.GetAllBills();

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _service.GetBillById(id);

            if (response == null) return NotFound();

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] BillOfExchangePostDto request)
        {
            #region File Handel
            string url = string.Empty;
            if (request.Asset != null) url = await SaveFile.AddFile(request.Asset.Base64);
            Asset? assetsResponse = null;
            if (!string.IsNullOrEmpty(url))
            {
                //request.Asset.Base64 = url;

                Asset asset = new Asset() { URL = url, Base64Url = request.Asset.Base64 };
                assetsResponse = await _assetService.AddAsset(asset) ?? null;
            }
            #endregion


            var billOfExchange = _mapper.Map<BillOfExchange>(request);
            //billOfExchange.Asset = assetsResponse ?? null;
            if (assetsResponse != null)
            {
                billOfExchange.AssetId = assetsResponse.Id;
            }

           var response = _service.AddBillOfExchanges(billOfExchange);

            if (response == null) return BadRequest();

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Add(Guid id, [FromBody] BillOfExchangePutDto request)
        {
            if (id != request.Id)return BadRequest();
            BillOfExchange? existingBill = await _service.GetBillById(id);
            
            if (existingBill == null) return NotFound();
            

            var updateBill = _mapper.Map<BillOfExchange>(request);

            var NumOfRowAffected = await _service.UpdateBillOfExchanges(updateBill);

            return NumOfRowAffected < 1 ? BadRequest() : Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            var bill = await _service.GetBillById(id);
            if (bill == null) return NotFound();

            var result = await _service.DeleteBillOfExchanges(bill);
            if (result == 1) return NoContent();

            return BadRequest("Failed to delete the bill.");
        }

        [HttpPost("/api/billOfExchange/upload-asset")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAsset([FromForm] Guid billOfExchangeId, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var bill = await _service.GetBillById(billOfExchangeId);
            if (bill is null)
                return NotFound(new { message = "Bill of exchange not found." });

            try
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "billOfExchange/asset", cancellationToken);
                var base64 = await ReadFileAsBase64Async(file, cancellationToken);

                Asset? asset = null;
                var assetRepo = _repositoriesFactory.GetRepository<Asset>();
                if (bill.AssetId.HasValue)
                {
                    var assetQuery = await assetRepo.Filter(x => x.Id == bill.AssetId.Value);
                    asset = assetQuery.Data?.FirstOrDefault();
                }

                if (asset is null)
                {
                    asset = new Asset
                    {
                        URL = uploadResult.Url,
                        Base64Url = base64,
                        PublicId = uploadResult.PublicId
                    };
                    await assetRepo.Add(asset);
                    bill.AssetId = asset.Id;
                    await _service.UpdateBillOfExchanges(bill);
                }
                else
                {
                    asset.URL = uploadResult.Url;
                    asset.Base64Url = base64;
                    asset.PublicId = uploadResult.PublicId;
                    await assetRepo.Update(asset);
                }

                return Ok(new { assetUrl = asset.URL, publicId = asset.PublicId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpGet("billNo")]
        public async Task<IActionResult> GetBillNo()
        {
            var newBillNoNum = await GetBillNumber();

            return Ok(new { BillNo = newBillNoNum });
        }

        private async Task<string> GetBillNumber()
        {
            var lastbilleNo = await _service.GetLastBillNo();
            return GenerateNewInvoiceNo(lastbilleNo);
        }

        private string GenerateNewInvoiceNo(string lastbilleNo)
        {
            if (string.IsNullOrEmpty(lastbilleNo)) return inv + 1;

            var digits = new string(lastbilleNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return inv + (oldNum + 1);
        }

        private static async Task<string> ReadFileAsBase64Async(IFormFile file, CancellationToken cancellationToken)
        {
            await using var stream = file.OpenReadStream();
            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
