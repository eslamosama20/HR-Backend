using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.SharedKernel.Entities.Contracts;

public interface IOperationResponse<T>
{
    T? Data { get; set; }
    
    ErrorCodes ErrorCode { get; set; }
    
    int? TotalCount { get; set; }
    bool IsPaginated { get; set; }
    string? Query { get; set; }
    int? Took { get; set; }
}