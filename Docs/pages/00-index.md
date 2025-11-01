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
   
   public interface IChocolateDispenser
   {
       int Chocolates { get; set; }
       void Refill(int count);
       bool Dispense(int count);
   }
   
   // Create a mock for IChocolateDispenser
   var mock = Mock.Create<IChocolateDispenser>();
   
   // Setup: Refill increases Chocolates property
   mock.Setup.Method.Refill(With.Any<int>())
      .Callback((int count) => mock.Subject.Chocolates += count);
   
   // Setup: Dispense decreases Chocolates if enough, returns true/false
   mock.Setup.Method.Dispense(With.Any<int>())
   	.Returns((int count) =>
   	{
   		var current = mock.Subject.Chocolates;
   		if (current >= count)
   		{
   			mock.Subject.Chocolates = current - count;
   			return true;
   		}
   		return false;
   	});
   
   // Use the mock
   mock.Subject.Refill(8);
   mock.Subject.Refill(10);
   mock.Subject.Dispense(6);
   var gotChocolates = mock.Subject.Dispense(3);
   mock.Subject.Dispense(4);
   
   // Verify that methods were called as expected
   mock.Verify.Invoked.Refill(With.Any<int>()).Twice();
   mock.Verify.Invoked.Dispense(3).Once();
   
   // Output: "Chocolates left: 5. Did I get my sweet treat? True (If not, I demand a recount!)"
   Console.WriteLine($"Chocolates left: {mock.Subject.Chocolates}. Did I get my sweet treat? {gotChocolates} (If not, I demand a recount!)");
   ```

