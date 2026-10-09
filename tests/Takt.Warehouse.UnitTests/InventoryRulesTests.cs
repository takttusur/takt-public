using Takt.Warehouse.API.Domain;

namespace Takt.Warehouse.UnitTests;

public sealed class InventoryRulesTests
{
    [Test]
    public void QuantityMustBePositive()
    {
        Assert.That(InventoryRules.IsPositiveQuantity(0), Is.False);
        Assert.That(InventoryRules.IsPositiveQuantity(-1), Is.False);
        Assert.That(InventoryRules.IsPositiveQuantity(1), Is.True);
    }

    [Test]
    public void CannotIssueMoreThanAvailable()
    {
        Assert.That(InventoryRules.CanIssue(1, 2), Is.False);
        Assert.That(InventoryRules.CanIssue(2, 1), Is.True);
    }

    [Test]
    public void ReturnCannotExceedOutstandingIssue()
    {
        Assert.That(InventoryRules.CanReturn(issuedQuantity: 2, alreadyReturnedQuantity: 1, returnQuantity: 2), Is.False);
        Assert.That(InventoryRules.CanReturn(issuedQuantity: 2, alreadyReturnedQuantity: 1, returnQuantity: 1), Is.True);
    }

    [Test]
    public void TransferCannotExceedStock()
    {
        Assert.That(InventoryRules.CanTransfer(0, 1), Is.False);
        Assert.That(InventoryRules.CanTransfer(3, 2), Is.True);
    }
}
