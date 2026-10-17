using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Sale;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Bills.Sale;
using SmartAndPro.ERP.Domains.Entities.Bills.Sales;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Bills.BillOfSale
{
    public class BillOfSaleService : ServiceBase
        <
            BillOfSales,
            BillOfSalePostDto,
            BillOfSalePutDto,
            BillOfSaleResponseDto,
            BillOfSaleFilterDto
        >,
        IBillOfSaleService
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;

        public BillOfSaleService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) 
            : base(repositoriesFactory, appSettings, mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }
        public async Task<List<BillOfSales>> GetAllBills()
        {
            var allBills = await _repositoriesFactory.GetRepository<BillOfSales>().GetAll();

            return allBills.Data.ToList();

        }

        public async Task<BillOfSales> GetBillById(Guid id)
        {
            var allBills = await _repositoriesFactory.GetRepository<BillOfSales>().GetAll();
            var bill = await allBills.Data.FirstOrDefaultAsync(d => d.Id == id);
            return bill;

        }

        public async Task<int> AddBillOfSales(BillOfSales BillOfSales)
        {
            try
            {
                var resultQuery = await _repositoriesFactory.GetRepository<BillOfSales>().Add(BillOfSales);
                var result = resultQuery.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateBillOfSales(BillOfSales billOfExchange)
        {
            try
            {
                billOfExchange.BillAccounts = null;
                billOfExchange.Asset = null;
                var resultQuery = await _repositoriesFactory.GetRepository<BillOfSales>().Update(billOfExchange);
                var result = resultQuery.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> DeleteBillOfSales(BillOfSales billOfExchange)
        {
            try
            {
                var resultQuery = await _repositoriesFactory.GetRepository<BillOfSales>().Delete(billOfExchange);
                var result = resultQuery.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<string> GetLastBillNo()
        {
            var billsQuery = await _repositoriesFactory.GetRepository<BillOfSales>().GetAll();
            var bills = await billsQuery.Data.ToListAsync();
            if (!bills.Any()) return await Task.FromResult(string.Empty);

            var invoice = bills.OrderBy(i => i.Id).Last();
            return invoice.BillNo;
        }
    }
}
