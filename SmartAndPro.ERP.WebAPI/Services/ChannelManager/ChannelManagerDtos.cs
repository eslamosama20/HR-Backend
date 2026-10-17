using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.WebAPI.Services.ChannelManager;

public class ChannelServiceResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public int ProcessedCount { get; set; }
}

public class ChannexAvailabilityPayload
{
    [JsonPropertyName("property_id")]
    public string PropertyId { get; set; } = string.Empty;

    [JsonPropertyName("availability")]
    public List<ChannexAvailabilityEntry> Availability { get; set; } = new();
}

public class ChannexAvailabilityEntry
{
    [JsonPropertyName("room_type_id")]
    public string RoomTypeId { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd

    [JsonPropertyName("availability")]
    public int Availability { get; set; }
}

public class ChannexRatePayload
{
    [JsonPropertyName("property_id")]
    public string PropertyId { get; set; } = string.Empty;

    [JsonPropertyName("rates")]
    public List<ChannexRateEntry> Rates { get; set; } = new();
}

public class ChannexRateEntry
{
    [JsonPropertyName("rate_plan_id")]
    public string RatePlanId { get; set; } = string.Empty;

    [JsonPropertyName("room_type_id")]
    public string RoomTypeId { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";
}

public class ChannexWebhookPayload
{
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty; // booking.created, booking.modified, booking.cancelled

    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("property_id")]
    public string PropertyId { get; set; } = string.Empty;

    [JsonPropertyName("booking")]
    public ChannexBookingData Booking { get; set; } = new();
}

public class ChannexBookingData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("ota_name")]
    public string OtaName { get; set; } = string.Empty;

    [JsonPropertyName("ota_reference")]
    public string OtaReference { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // new, modified, cancelled

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("customer")]
    public ChannexCustomer Customer { get; set; } = new();

    [JsonPropertyName("checkin_date")]
    public string CheckInDate { get; set; } = string.Empty; // yyyy-MM-dd

    [JsonPropertyName("checkout_date")]
    public string CheckOutDate { get; set; } = string.Empty; // yyyy-MM-dd

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("commission_amount")]
    public decimal CommissionAmount { get; set; }

    [JsonPropertyName("tax_amount")]
    public decimal TaxAmount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";

    [JsonPropertyName("payment_model")]
    public string PaymentModel { get; set; } = "PayAtHotel"; // Prepaid, VCC, PayAtHotel

    [JsonPropertyName("vcc_card")]
    public ChannexVccCard? VccCard { get; set; }

    [JsonPropertyName("rooms")]
    public List<ChannexBookingRoom> Rooms { get; set; } = new();
}

public class ChannexCustomer
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }
}

public class ChannexVccCard
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; } = string.Empty;

    [JsonPropertyName("cvv")]
    public string Cvv { get; set; } = string.Empty;

    [JsonPropertyName("expiry_month")]
    public int ExpiryMonth { get; set; }

    [JsonPropertyName("expiry_year")]
    public int ExpiryYear { get; set; }

    [JsonPropertyName("activation_date")]
    public string? ActivationDate { get; set; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }
}

public class ChannexBookingRoom
{
    [JsonPropertyName("room_type_id")]
    public string RoomTypeId { get; set; } = string.Empty;

    [JsonPropertyName("rate_plan_id")]
    public string RatePlanId { get; set; } = string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
