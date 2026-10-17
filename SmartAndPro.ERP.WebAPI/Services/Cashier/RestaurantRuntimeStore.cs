namespace SmartAndPro.ERP.WebAPI.Services.Cashier;

public interface IRestaurantRuntimeStore
{
    RestaurantTableState OpenTable(int tableId, string? tableName, int sessionId, Guid storeDetailsId, Guid cashierUserGuid, string cashierUserName);
    RestaurantTableState? GetTable(int tableId);
    RestaurantTableState CloseTable(int tableId, Guid cashierUserGuid, string cashierUserName);
}

public sealed class RestaurantRuntimeStore : IRestaurantRuntimeStore
{
    private readonly object _sync = new();
    private readonly Dictionary<int, RestaurantTableState> _tables = new();

    public RestaurantTableState OpenTable(int tableId, string? tableName, int sessionId, Guid storeDetailsId, Guid cashierUserGuid, string cashierUserName)
    {
        lock (_sync)
        {
            if (_tables.TryGetValue(tableId, out var existing) && existing.Status == RestaurantTableStatus.Open)
            {
                return Clone(existing);
            }

            var now = DateTime.UtcNow;
            var state = new RestaurantTableState
            {
                TableId = tableId,
                TableName = string.IsNullOrWhiteSpace(tableName) ? $"Table {tableId}" : tableName.Trim(),
                SessionId = sessionId,
                StoreDetailsId = storeDetailsId,
                OpenedAtUtc = now,
                OpenedByUserGuid = cashierUserGuid,
                OpenedByUserName = string.IsNullOrWhiteSpace(cashierUserName) ? "Cashier" : cashierUserName.Trim(),
                LastActionAtUtc = now,
                Status = RestaurantTableStatus.Open,
                HasDraftInvoice = true
            };

            _tables[tableId] = state;
            return Clone(state);
        }
    }

    public RestaurantTableState? GetTable(int tableId)
    {
        lock (_sync)
        {
            return _tables.TryGetValue(tableId, out var table) ? Clone(table) : null;
        }
    }

    public RestaurantTableState CloseTable(int tableId, Guid cashierUserGuid, string cashierUserName)
    {
        lock (_sync)
        {
            if (!_tables.TryGetValue(tableId, out var table))
            {
                throw new InvalidOperationException("Table is not open");
            }

            table.Status = RestaurantTableStatus.Available;
            table.LastActionAtUtc = DateTime.UtcNow;
            table.ClosedAtUtc = DateTime.UtcNow;
            table.ClosedByUserGuid = cashierUserGuid;
            table.ClosedByUserName = string.IsNullOrWhiteSpace(cashierUserName) ? "Cashier" : cashierUserName.Trim();
            table.HasDraftInvoice = false;

            return Clone(table);
        }
    }

    private static RestaurantTableState Clone(RestaurantTableState source) =>
        new()
        {
            TableId = source.TableId,
            TableName = source.TableName,
            SessionId = source.SessionId,
            StoreDetailsId = source.StoreDetailsId,
            OpenedAtUtc = source.OpenedAtUtc,
            OpenedByUserGuid = source.OpenedByUserGuid,
            OpenedByUserName = source.OpenedByUserName,
            LastActionAtUtc = source.LastActionAtUtc,
            ClosedAtUtc = source.ClosedAtUtc,
            ClosedByUserGuid = source.ClosedByUserGuid,
            ClosedByUserName = source.ClosedByUserName,
            Status = source.Status,
            HasDraftInvoice = source.HasDraftInvoice
        };
}

public enum RestaurantTableStatus
{
    Available = 0,
    Open = 1
}

public sealed class RestaurantTableState
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public Guid StoreDetailsId { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public Guid OpenedByUserGuid { get; set; }
    public string OpenedByUserName { get; set; } = string.Empty;
    public DateTime LastActionAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public Guid? ClosedByUserGuid { get; set; }
    public string? ClosedByUserName { get; set; }
    public RestaurantTableStatus Status { get; set; }
    public bool HasDraftInvoice { get; set; }
}
