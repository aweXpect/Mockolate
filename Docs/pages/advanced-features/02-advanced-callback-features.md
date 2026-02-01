# Advanced callback features

## Conditional callbacks (`When`)

Execute callbacks conditionally based on the zero-based invocation counter using `.When()`:

```csharp
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Do(() => Console.WriteLine("Called!")).When(count => count >= 2);  // The first two calls are skipped
```

## Limit invocations (`Only`)

Control after how many times a callback should no longer be executed:

```csharp
// Execute up to 3 times
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Up to 3 times")).Only(3);

// Executes the callback only once
sut.SetupMock.Property.TotalDispensed
    .Throws(new Exception("This exception is thrown only once")).OnlyOnce();
```

## Repeat invocations (`For`)

Control how many times a callback should be repeated:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("First three times")).For(3)
    .Do(() => Console.WriteLine("Next three times")).For(3);

sut.SetupMock.Property.TotalDispensed
    .Returns(10).For(1)
    .Returns(20).For(2)
    .Returns(30).For(3);
// Reads: 10, 20, 20, 30, 30, 30, 0, 0, 0, 0 …
```

### Repeat `Forever`

If you have a sequence of callbacks, you can mark the last one to repeat indefinitely using `.Forever()` to avoid
repeating the sequence from start:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Returns(true).For(2)      // Returns true the first two times
    .Returns(false).Forever(); // Then always returns false
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

**Note:**
This only applies to callbacks defined via `Do`, not to the other setup callbacks like `Returns` or `Throws`.

## Invocation counter

Access the zero-based invocation counter in callbacks:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do((count, _, _) => Console.WriteLine($"Call #{count}"));

sut.SetupMock.Property.TotalDispensed.OnGet
    .Do((count, value) => Console.WriteLine($"Read #{count}, value: {value}"));
```
