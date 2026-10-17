namespace SmartAndPro.ERP.Domains.Common.Models;

public interface IEntityOwnershipPolicy
{
     bool IsEntityAccessibleByUser( Entity entity, Guid userId);
}