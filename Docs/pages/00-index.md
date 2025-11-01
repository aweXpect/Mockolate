# Mockolate

<img align="right" width="200" src="https://raw.githubusercontent.com/aweXpect/Mockolate/main/Docs/logo_256x256.png" alt="Mockolate logo" />

[![Nuget](https://img.shields.io/nuget/v/Mockolate)](https://www.nuget.org/packages/Mockolate)

**Mockolate** is a modern, strongly-typed mocking library for .NET, powered by source generators. It enables fast,
compile-time validated mocks for interfaces and classes, supporting .NET Standard 2.0, .NET 8, .NET 10, and .NET
Framework 4.8.

- **Source generator-based**: No runtime proxy generation, fast and reliable.
- **Strongly-typed**: Setup and verify mocks with full IntelliSense and compile-time safety.
- **AOT compatible**: Works with NativeAOT and trimming.

## Getting Started

1. Install the [`Mockolate`](https://www.nuget.org/packages/Mockolate) nuget package
   ```ps
   dotnet add package Mockolate
   ```

2. Create and use the mock
   ```csharp
   using Mockolate;

   // Create a mock for IChocolateDispenser
   var mock = Mock.Create<IChocolateDispenser>();
   
   // Setup: Initial stock of 10 for Dark chocolate
   mock.Setup.Indexer("Dark").InitializeWith(10);
   // Setup: Dispense decreases Dark chocolate if enough, returns true/false
   mock.Setup.Method.Dispense("Dark", With.Any<int>())
       .Returns((type, amount) =>
       {
           var current = mock.Subject[type];
           if (current >= amount)
           {
               mock.Subject[type] = current - amount;
               mock.Raise.ChocolateDispensed(type, amount);
               return true;
           }
           return false;
       });
   
   // Track dispensed amount via event
   int dispensedAmount = 0;
   mock.Subject.ChocolateDispensed += (type, amount) => dispensedAmount += amount;
   
   // Act: Try to dispense chocolates
   bool gotChoc1 = mock.Subject.Dispense("Dark", 4); // true
   bool gotChoc2 = mock.Subject.Dispense("Dark", 5); // true
   bool gotChoc3 = mock.Subject.Dispense("Dark", 6); // false
   
   // Verify: Check interactions
   mock.Verify.Invoked.Dispense("Dark", With.Any<int>()).Exactly(3);
   
   // Output: "Dispensed events: 9. Got chocolate? True, True, False"
   Console.WriteLine($"Dispensed amount: {dispensedAmount}. Got chocolate? {gotChoc1}, {gotChoc2}, {gotChoc3}");

   public delegate void ChocolateDispensedDelegate(string type, int amount);
   public interface IChocolateDispenser
   {
       int this[string type] { get; set; }
       int TotalDispensed { get; set; }
       bool Dispense(string type, int amount);
       event ChocolateDispensedDelegate ChocolateDispensed;
   }
   ```
