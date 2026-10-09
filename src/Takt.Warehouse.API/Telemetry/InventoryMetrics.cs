using System.Diagnostics.Metrics;

namespace Takt.Warehouse.API.Telemetry;

public interface IInventoryMetrics
{
    void TrackIssue(bool success);
    void TrackReturn(bool success);
    void TrackPendingOutbox(int count);
    void TrackEventProcessingFailure();
}

public sealed class InventoryMetrics : IInventoryMetrics
{
    public const string MeterName = "Takt.Warehouse.Inventory";
    private static readonly Meter Meter = new(MeterName);
    private readonly Counter<long> _issueCounter = Meter.CreateCounter<long>("inventory_issue_total");
    private readonly Counter<long> _returnCounter = Meter.CreateCounter<long>("inventory_return_total");
    private readonly Counter<long> _issueFailedCounter = Meter.CreateCounter<long>("inventory_issue_failed_total");
    private readonly Counter<long> _eventFailedCounter = Meter.CreateCounter<long>("inventory_event_processing_failed_total");
    private readonly ObservableGauge<int> _outboxPendingGauge;
    private int _pendingOutbox;

    public InventoryMetrics()
    {
        _outboxPendingGauge = Meter.CreateObservableGauge("inventory_outbox_pending", () => _pendingOutbox);
    }

    public void TrackIssue(bool success)
    {
        if (success)
        {
            _issueCounter.Add(1);
            return;
        }

        _issueFailedCounter.Add(1);
    }

    public void TrackReturn(bool success)
    {
        if (success)
        {
            _returnCounter.Add(1);
        }
    }

    public void TrackPendingOutbox(int count) => _pendingOutbox = count;

    public void TrackEventProcessingFailure() => _eventFailedCounter.Add(1);
}
