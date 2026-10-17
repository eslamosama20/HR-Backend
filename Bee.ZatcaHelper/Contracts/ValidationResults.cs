namespace Bee.ZatcaHelper.Contracts;

public class ValidationResults
{
    public InfoMessages[] InfoMessages { get; set; }
    public WarningMessage[] WarningMessages { get; set; }
    public ErrorMessage[] ErrorMessages { get; set; }
    public string? Status { get; set; } 
}

public class InfoMessages
{
    public string Type { get; set; }
    public string Code { get; set; }
    public string Category { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
}

public class ErrorMessage
{
    public string Type { get; set; }
    public string Code { get; set; }
    public string Category { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
}

public class WarningMessage
{
    public string Type { get; set; }
    public string Code { get; set; }
    public string Category { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
}
