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

2. Create a mock
   ```csharp
   using Mockolate;
   
   var mock = Mock.Create<IMyInterface>();
   ```

