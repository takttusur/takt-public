namespace Takt.Warehouse.API.Domain;

public static class InventoryRules
{
    public static bool IsPositiveQuantity(int quantity) => quantity > 0;

    public static bool CanIssue(int availableQuantity, int requestedQuantity) =>
        requestedQuantity > 0 && availableQuantity >= requestedQuantity;

    public static bool CanReturn(int issuedQuantity, int alreadyReturnedQuantity, int returnQuantity) =>
        returnQuantity > 0 && alreadyReturnedQuantity + returnQuantity <= issuedQuantity;

    public static bool CanTransfer(int availableQuantity, int requestedQuantity) =>
        CanIssue(availableQuantity, requestedQuantity);
}
