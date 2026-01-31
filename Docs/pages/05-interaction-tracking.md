# Interaction Tracking

Mockolate tracks all interactions with mocks. You can access and manage these interactions:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense("Dark", 5);
sut.Dispense("Milk", 3);

// Access all interactions
Mock<IChocolateDispenser> mockInstance = ((IMockSubject<IChocolateDispenser>)sut).Mock;
foreach (var interaction in mockInstance.Interactions.Interactions)
{
    Console.WriteLine($"Interaction: {interaction}");
}

// Clear interaction history
sut.SetupMock.ClearAllInteractions();
```

## Mock Monitoring Sessions

Use `MockMonitor.Run()` to track interactions within a specific scope:

```csharp
var sut = Mock.Create<IChocolateDispenser>();
using (var monitor = sut.MonitorMock(out MockMonitor<IChocolateDispenser> monitorInstance))
{
    sut.Dispense("Dark", 5);
    
    // Verify interactions within this session
    monitorInstance.Verify.Invoked.Dispense(It.Is("Dark"), It.Is(5)).Once();
}
```
