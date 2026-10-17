using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Assets;
using SmartAndPro.ERP.Application.Services.Bills.BillOfSale;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.WebAPI.Hellper;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Sale;
using SmartAndPro.ERP.Domains.Entities.Bills.Sales;

namespace SmartAndPro.ERP.WebAPI.Controllers.Bills
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillsOfSalesController : Controller
    {
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly BillOfSaleService _service;
        private readonly AssetService _assetService;
        private readonly IMapper _mapper;
        private const string inv = "Bill-";

        public BillsOfSalesController(ServiceBaseFactory serviceBaseFactory, IMapper mapper)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _service = _serviceBaseFactory.GetService<BillOfSaleService>();
            _assetService = _serviceBaseFactory.GetService<AssetService>();
            _mapper = mapper;
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
        public async Task<IActionResult> Add([FromBody] BillOfSalePostDto request)
        {
            #region File Handel
            string url = string.Empty;
            if (request.Asset != null) url = await SaveFile.AddFile(request.Asset.Base64);
            Asset? assetsResponse = null;
            if (!string.IsNullOrEmpty(url))
            {
                //request.Asset.Base64 = url;

                Asset asset = new Asset() { URL = url };
                assetsResponse = await _assetService.AddAsset(asset) ?? null;
            }
            #endregion


            var BillOfSale = _mapper.Map<BillOfSales>(request);
            //BillOfSale.Asset = assetsResponse ?? null;
            if (assetsResponse != null)
            {
                BillOfSale.AssetId = assetsResponse.Id;
            }
            var response = _service.AddBillOfSales(BillOfSale);

            if (response == null) return BadRequest();

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BillOfSalePutDto request)
        {
            if (id != request.Id) return BadRequest();
            BillOfSales? existingBill = await _service.GetBillById(id);

            if (existingBill == null) return NotFound();

            existingBill.BillAccounts.Clear();


            var updateBill = _mapper.Map<BillOfSales>(request);

            var NumOfRowAffected = await _service.UpdateBillOfSales(updateBill);

            return NumOfRowAffected < 1 ? BadRequest() : Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            var bill = await _service.GetBillById(id);
            if (bill == null) return NotFound();

            var result = await _service.DeleteBillOfSales(bill);
            if (result == 1) return NoContent();

            return BadRequest("Failed to delete the bill.");
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
    }
}
