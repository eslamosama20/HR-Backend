using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Exchange;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Bills.Exchange;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using BillOfExchanges = SmartAndPro.ERP.Domains.Entities.Bills.Exchange.BillOfExchange;

namespace SmartAndPro.ERP.Application.Services.Bills.BillOfExchange
{
    public class BillOfExchangeService : ServiceBase
        <
        BillOfExchanges,
        BillOfExchangePostDto,
        BillOfExchangePutDto,
        BillOfExchangeResponseDto,
        BillOfExchangeFilterDto
        >,
        IBillOfExchangeService
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;
        public BillOfExchangeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) : base(repositoriesFactory, appSettings, mapper)
        {
            _mapper = mapper;
            _appSettings = appSettings;
            _repositoriesFactory = repositoriesFactory;
        }


        public async Task<List<BillOfExchanges>> GetAllBills()
        {
            var allBills = await _repositoriesFactory.GetRepository<BillOfExchanges>().GetAll();

            return allBills.Data.ToList();

        }

        public async Task<BillOfExchanges> GetBillById(Guid id)
        {
            var allBills = await _repositoriesFactory.GetRepository<BillOfExchanges>().GetAll();
            var bill = await allBills.Data.FirstOrDefaultAsync(d=>d.Id == id);
            return bill;

        }

        public async Task<int> AddBillOfExchanges(BillOfExchanges billOfExchanges)
        {
            try
            {
                var resultQuery = await _repositoriesFactory.GetRepository<BillOfExchanges>().Add(billOfExchanges);
                var result = resultQuery.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateBillOfExchanges(BillOfExchanges billOfExchange)
        {
            try
            {
                billOfExchange.BillAccounts = null;
                billOfExchange.Asset = null;
                var resultQuery = await _repositoriesFactory.GetRepository<BillOfExchanges>().Update(billOfExchange);
                var result = resultQuery.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> DeleteBillOfExchanges(BillOfExchanges billOfExchange)
        {
            try
            {
                var resultQuery = await _repositoriesFactory.GetRepository<BillOfExchanges>().Delete(billOfExchange);
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
            var invoicesQuery = await _repositoriesFactory.GetRepository<BillOfExchanges>().GetAll();
            var invoices = await invoicesQuery.Data.ToListAsync();
            if (!invoices.Any()) return await Task.FromResult(string.Empty);

            var invoice = invoices.OrderBy(i => i.Id).Last();
            return invoice.BillNo;
        }
    }
}
