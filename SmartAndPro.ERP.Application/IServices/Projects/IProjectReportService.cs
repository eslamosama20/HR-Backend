using System;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.IServices.Projects
{
    public interface IProjectReportService
    {
        Task<byte[]> GenerateAccountStatementPdfAsync(Guid projectId);
        Task<byte[]> GenerateProjectArchivePdfAsync(Guid projectId);
    }
}
