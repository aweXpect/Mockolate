# Advanced callback features

## Conditional callbacks

Execute callbacks conditionally based on the zero-based invocation counter using `.When()`:

```csharp
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Do(() => Console.WriteLine("Called!"))
    .When(count => count >= 2);  // The first two calls are skipped
```

## Frequency control

Control how many times a callback executes:

```csharp
// Execute up to 3 times
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Up to 3 times"))
    .Only(3);

// Executes the callback only once
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Only once"))
    .OnlyOnce();
```

## Parallel callbacks

When you specify multiple callbacks, they are executed sequentially by default. You can change this behavior to always
run specific callbacks in parallel using `.InParallel()`:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => { Console.WriteLine("Runs every second iteration"); })
    .Do(() => { Console.WriteLine("Runs always in parallel"); }).InParallel()
    .Do(() => { Console.WriteLine("Runs every other iteration"); });
```

## Invocation counter

Access the zero-based invocation counter in callbacks:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do((count, _, _) => Console.WriteLine($"Call #{count}"));

sut.SetupMock.Property.TotalDispensed.OnGet
    .Do((count, value) => Console.WriteLine($"Read #{count}, value: {value}"));
```
