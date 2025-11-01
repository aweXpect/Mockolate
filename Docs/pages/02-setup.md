# Setup

Set up return values or behaviors for methods and properties on your mock. Control how the mock responds to calls in your tests.

## Method setup
```csharp
mock.Setup.AddUser(With.Any<string>())
    .Returns(name => new User(Guid.NewGuid(), name));
```

- Use `.Callback(…)` to run code when the method is called.
- Use `.Returns(…)` to specify the value to return. You can provide a direct value or a callback to generate values on demand.
- Use `.Throws(…)` to specify an exception to throw when the method is executed.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of return values.

**Argument Matching**

Mockolate provides flexible argument matching for method setups and verifications:

- `With.Any<T>()`: Matches any value of type `T`.
- `With.Matching<T>(predicate)`: Matches values based on a predicate.
- `With.Ref<T>(…)`/`With.Out<T>(…)`: Matches and sets ref or out parameters.

```csharp
mock.Setup.AddUser(With.Matching<string>(name => name.StartsWith("A")))
    .Returns(new User(Guid.NewGuid(), "Alicia"));

mock.Setup.TryDelete(With.Any<Guid>(), With.Out<User?>(() => new User(id, "Alice")))
    .Returns(true);
```

## Property Setup

Set up property getters and setters to control or verify property access on your mocks. Supports auto-properties and indexers.

**Initialization**  
You can initialize properties and they will work like normal properties (setter changes the value, getter returns the last set value).

```csharp
mock.Setup.Property.MyProperty.InitializeWith(42);
```

**Returns / Throws**  
Alternatively you can set up the properties similar to methods with `Returns` and `Throws`.
```csharp
mock.Setup.Property.MyProperty
	.Returns(1)
	.Returns(2)
	.Throws(new Exception("Error"))
	.Returns(4);
```

**Callbacks**  
Callbacks can be registered on the setter or getter.
```csharp
mock.Setup.Property.MyProperty.OnGet(() => Console.WriteLine("MyProperty was read!"));
mock.Setup.Property.MyProperty.OnSet(value => Console.WriteLine($"Set MyProperty to {value}!"));
```

**Indexers**  
Indexers are supported as well.
```csharp
mock.Setup.Indexer(With.Any<int>())
	.InitializeWith(index => index*index)
	.OnGet(index => Console.WriteLine($"Indexer this[{index}] was read"));
```
