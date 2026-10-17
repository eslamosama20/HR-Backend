namespace SmartAndPro.ERP.SharedKernel.Enums
{
    
    public enum ErrorCodes 
    { 
        Success = 0,
        GeneralFail = -1,
        NoRecordsUpdated = -2,
        Validation = -3,
        Conflict = -4,
        NotFound = -5,
        Unauthorized =-6,
        Forbidden = -7,
        NoContent = -8,
        UnprocessableEntity = -9,
        BadRequest = -10,
        Overbooking = -11,
        InsufficientInventory = -12

    }
}
