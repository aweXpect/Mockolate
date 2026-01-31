# Monitor interactions

Mockolate tracks all interactions with mocks on the mock object. To only track interactions within a given scope, you
can use a `MockMonitor<T>`:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense("Dark", 1); // Not monitored
var monitorScope = sut.MonitorMock(out var monitor);
using (monitorScope)
{
    sut.Dispense("Dark", 2); // Monitored
}
sut.Dispense("Dark", 3); // Not monitored

// Verifications on the monitor only count interactions during the lifetime scope of the `IDisposable`
monitor.Verify.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```

## Clear all interactions

For simpler scenarios you can directly clear all recorded interactions on a mock using `ClearAllInteractions` on the
setup:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense("Dark", 1);
// Clears all previously recorded interactions
sut.SetupMock.ClearAllInteractions();
sut.Dispense("Dark", 2);

sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```
