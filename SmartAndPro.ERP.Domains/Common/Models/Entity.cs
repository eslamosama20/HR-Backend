namespace SmartAndPro.ERP.Domains.Common.Models;

public abstract class Entity
{
    private int? _requestedHashCode;
    public virtual Guid Id { get; set; }

    public virtual bool IsActive { get; set; } = true;
    
    public virtual Guid CreatedBy { get; set; }
    
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    private bool IsTransient()
    {
        return this.Id == Guid.Empty;
    }
    
    public virtual void SoftDelete()
    {
        this.IsActive = false;
    }
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Entity))
            return false;
        if (Object.ReferenceEquals(this, obj))
            return true;
        if (this.GetType() != obj.GetType())
            return false;
        var item = (Entity)obj;
        if (item.IsTransient() || this.IsTransient())
            return false;
        else
            return item.Id == this.Id;
    }

    public override int GetHashCode()
    {
        if (IsTransient()) return base.GetHashCode();
        if (!_requestedHashCode.HasValue)
            _requestedHashCode = this.Id.GetHashCode() ^ 31;
        // XOR for random distribution. See:
        // https://learn.microsoft.com/archive/blogs/ericlippert/guidelines-and-rules-for-gethashcode
        return _requestedHashCode.Value;

    }
    public static bool operator ==(Entity? left, Entity? right)
    {
        return Object.Equals(left, null) ? Object.Equals(right, null) : left.Equals(right);
    }
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
