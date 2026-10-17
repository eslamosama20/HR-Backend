namespace SmartAndPro.ERP.SharedKernel.Entities.Contracts;

public class PaginatedResult<T>(int totalCount, T data)
{
    public int TotalCount { get; set; } = totalCount;
    public T Data { get; set; } = data;
}