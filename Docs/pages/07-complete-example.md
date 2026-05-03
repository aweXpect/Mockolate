# A complete example

The following example combines properties, indexers, events, methods, stateful setup, and verification
into a single end-to-end scenario:

```csharp
using Mockolate;

public delegate void ChocolateDispensedDelegate(string type, int amount);
public interface IChocolateDispenser
{
    int this[string type] { get; set; }
    int TotalDispensed { get; set; }
    bool Dispense(string type, int amount);
    event ChocolateDispensedDelegate ChocolateDispensed;
}

// Create a mock of IChocolateDispenser
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

// Setup: Initial stock of 10 for Dark chocolate
sut.Mock.Setup["Dark"].InitializeWith(10);
// Setup: Dispense decreases Dark chocolate if enough, returns true/false
sut.Mock.Setup.Dispense("Dark", It.IsAny<int>())
    .Returns((type, amount) =>
    {
        int current = sut[type];
        if (current >= amount)
        {
            sut[type] = current - amount;
            sut.Mock.Raise.ChocolateDispensed(type, amount);
            return true;
        }
        return false;
    });

// Track dispensed amount via event
int dispensedAmount = 0;
sut.ChocolateDispensed += (type, amount) =>
{
    dispensedAmount += amount;
};

// Act: Try to dispense chocolates
bool gotChoc1 = sut.Dispense("Dark", 4); // true
bool gotChoc2 = sut.Dispense("Dark", 5); // true
bool gotChoc3 = sut.Dispense("Dark", 6); // false

// Verify: Check interactions
sut.Mock.Verify.Dispense("Dark", It.IsAny<int>()).Exactly(3);
```
