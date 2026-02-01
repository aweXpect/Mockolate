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

1. Check prerequisites:  
   Although Mockolate supports multiple .NET Standard 2.0 compatible frameworks (including .NET Framework 4.8), you must
   have the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed to build and compile
   Mockolate. This is required because Mockolate
   leverages [C# 14 extension members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods#declare-extension-members).

2. Install the [`Mockolate`](https://www.nuget.org/packages/Mockolate) nuget package
   ```ps
   dotnet add package Mockolate
   ```

3. Create and use the mock
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
   
   // Create a mock for IChocolateDispenser
   var sut = Mock.Create<IChocolateDispenser>();
   
   // Setup: Initial stock of 10 for Dark chocolate
   sut.SetupMock.Indexer(It.Is("Dark")).InitializeWith(10);
   // Setup: Dispense decreases Dark chocolate if enough, returns true/false
   sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
       .Returns((type, amount) =>
       {
           var current = sut[type];
           if (current >= amount)
           {
               sut[type] = current - amount;
               sut.RaiseOnMock.ChocolateDispensed(type, amount);
               return true;
           }
           return false;
       });
   
   // Track dispensed amount via event
   int dispensedAmount = 0;
   sut.ChocolateDispensed += (type, amount)
   {
       dispensedAmount += amount;
   }
   
   // Act: Try to dispense chocolates
   bool gotChoc1 = sut.Dispense("Dark", 4); // true
   bool gotChoc2 = sut.Dispense("Dark", 5); // true
   bool gotChoc3 = sut.Dispense("Dark", 6); // false
   
   // Verify: Check interactions
   sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Exactly(3);
   
   // Output: "Dispensed amount: 9. Got chocolate? True, True, False"
   Console.WriteLine($"Dispensed amount: {dispensedAmount}. Got chocolate? {gotChoc1}, {gotChoc2}, {gotChoc3}");
   ```
