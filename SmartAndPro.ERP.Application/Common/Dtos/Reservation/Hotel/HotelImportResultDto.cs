namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Hotel;

public class HotelImportResultDto
{
    public int Imported { get; set; }
    public int Updated  { get; set; }
    public List<HotelImportFailureDto> Failed { get; set; } = new();
}

public class HotelImportFailureDto
{
    public int    Row    { get; set; }
    public string Name   { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
