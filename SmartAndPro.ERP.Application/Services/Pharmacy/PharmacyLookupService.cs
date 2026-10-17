using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Application.DTOs.Pharmacy;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;

namespace SmartAndPro.ERP.Application.Services.Pharmacy
{
    public class PharmacyLookupService : IPharmacyLookupService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ILogger<PharmacyLookupService> _logger;

        public PharmacyLookupService(
            RepositoriesFactory repositoriesFactory,
            ILogger<PharmacyLookupService> logger)
        {
            _repositoriesFactory = repositoriesFactory;
            _logger = logger;
        }

        public async Task<List<StoreLookupDto>> GetStoresAsync(StoresQueryDto query)
        {
            try
            {
                // Store lookup queries existing store module - kept as mock until store integration
                var stores = new List<StoreLookupDto>
                {
                    new StoreLookupDto { Id = "WH-01", Name = "المخزن الرئيسي", BranchId = query.BranchId },
                    new StoreLookupDto { Id = "WH-02", Name = "مخزن ملحق", BranchId = query.BranchId }
                };

                return await Task.FromResult(stores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stores");
                throw;
            }
        }

        public async Task<List<InsuranceCompanyLookupDto>> SearchInsuranceCompaniesAsync(InsuranceCompaniesQueryDto query)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<PharmacyInsuranceCompany>();
                var result = await repo.GetAll();
                var q = result.Data ?? Enumerable.Empty<PharmacyInsuranceCompany>().AsQueryable();

                if (!string.IsNullOrEmpty(query.Term))
                {
                    var term = query.Term.ToLower();
                    q = q.Where(c => c.Name.ToLower().Contains(term));
                }

                var companies = await q
                    .Take(query.Take)
                    .Select(c => new InsuranceCompanyLookupDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        DefaultCoveragePercentage = c.DefaultCoveragePercentage
                    })
                    .ToListAsync();

                return companies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching insurance companies");
                throw;
            }
        }
    }
}
