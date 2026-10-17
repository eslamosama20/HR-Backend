namespace SmartAndPro.ERP.Domains.Entities.Return.Abstraction
{
    public class BaseEntity<T>
    {
        public T Id { get; set; }

        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
