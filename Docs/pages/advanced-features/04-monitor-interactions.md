# Monitor interactions

Mockolate tracks all interactions with mocks on the mock object. To only track interactions within a given scope, you
can use a `MockMonitor<T>`:

```csharp
var sut = IChocolateDispenser.CreateMock();
var monitor = sut.Mock.Monitor();

sut.Dispense("Dark", 1); // Not monitored
using (monitor.Run())
{
    sut.Dispense("Dark", 2); // Monitored
}
sut.Dispense("Dark", 3); // Not monitored

// Verifications on the monitor only count interactions during the lifetime scope of the `IDisposable`
monitor.Verify.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```

## Clear all interactions

For simpler scenarios you can directly clear all recorded interactions on a mock using `ClearAllInteractions` on the
setup:

```csharp
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

sut.Dispense("Dark", 1);
// Clears all previously recorded interactions
sut.Mock.ClearAllInteractions();
sut.Dispense("Dark", 2);

sut.Mock.Verify.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```
