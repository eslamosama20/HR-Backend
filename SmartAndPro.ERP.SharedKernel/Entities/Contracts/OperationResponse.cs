using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.SharedKernel.Entities.Contracts
{
    public class OperationResponse<T> : IOperationResponse<T>
    {
        public OperationResponse() { }
        public OperationResponse(ErrorCodes ErrorCode, T? data , int? totalCount = null, bool isPaginated = false, string? message = null, string? query = null, int? took = null)
        {
            this.ErrorCode = ErrorCode;
            this.Data = data;
            this.TotalCount = totalCount;
            this.IsPaginated = isPaginated;
            this.Message = message;
            this.Query = query;
            this.Took = took;
        }


        public T? Data { get; set; }
        // Optional pagination properties
        
        public string? Message { get; set; }
        public int? TotalCount { get; set; } = null;
        public bool IsPaginated { get; set; } = false;
        
        public ErrorCodes ErrorCode { get; set; }
        public string? Query { get; set; }
        public int? Took { get; set; }
    }
}
