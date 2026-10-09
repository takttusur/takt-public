namespace Takt.Warehouse.API.Domain;

public enum MovementType
{
    Receipt = 1,
    Issue = 2,
    Return = 3,
    TransferOut = 4,
    TransferIn = 5,
    Adjustment = 6
}
