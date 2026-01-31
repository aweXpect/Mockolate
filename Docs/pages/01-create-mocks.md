# Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, use
`BaseClass.WithConstructorParameters(…)` to provide constructor arguments:

```csharp
// Create a mock for an interface
var sut = Mock.Create<IChocolateDispenser>();

// Create a mock for a class
var classMock = Mock.Create<MyChocolateDispenser>();

// For classes without a default constructor:
var classWithArgsMock = Mock.Create<MyChocolateDispenserWithCtor>(
    BaseClass.WithConstructorParameters("Dark", 42)
);

// Specify up to 8 additional interfaces for the mock:
var sut2 = Mock.Create<MyChocolateDispenser, ILemonadeDispenser>();
```

**Notes:**

- Only the first generic type can be a class; additional types must be interfaces.
- Sealed classes cannot be mocked and will throw a `MockException`.

## Customizing mock behavior

You can control the default behavior of the mock by providing a `MockBehavior`:

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(new MockBehavior { ThrowWhenNotSetup = true });

// For classes with constructor parameters and custom behavior:
var classMock = Mock.Create<MyChocolateDispenser>(
    BaseClass.WithConstructorParameters("Dark", 42),
    new MockBehavior { ThrowWhenNotSetup = true }
);
```

**`MockBehavior` options**

- `ThrowWhenNotSetup` (bool):
	- If `false` (default), the mock will return a default value (see `DefaultValue`).
	- If `true`, the mock will throw an exception when a method or property is called without a setup.
- `SkipBaseClass` (bool):
	- If `false` (default), the mock will call the base class implementation and use its return values as default
	  values, if no explicit setup is defined.
	- If `true`, the mock will not call any base class implementations.
- `Initialize<T>(params Action<IMockSetup<T>>[] setups)`:
	- Automatically initialize all mocks of type T with the given setups when they are created.
	- The callback can optionally receive an additional counter parameter which allows to differentiate between multiple
	  instances. This is useful when you want to ensure that you can distinguish between different automatically created
	  instances.
- `DefaultValue` (IDefaultValueGenerator):
	- Customizes how default values are generated for methods/properties that are not set up.
	- The default implementation provides sensible defaults for the most common use cases:
		- Empty collections for collection types (e.g., `IEnumerable<T>`, `List<T>`, etc.)
		- Empty string for `string`
		- Completed tasks for `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>`
		- Tuples with recursively defaulted values
		- `null` for other reference types
	- You can provide custom default values for specific types using `.WithDefaultValueFor<T>()`:
	  ```csharp
	  var behavior = MockBehavior.Default
	      .WithDefaultValueFor<string>(() => "default")
	      .WithDefaultValueFor<int>(() => 42);
	  var sut = Mock.Create<IChocolateDispenser>(behavior);
	  ```
	  This is useful when you want mocks to return specific default values for certain types instead of the standard
	  defaults (e.g., `null`, `0`, empty strings).
- `.UseConstructorParametersFor<T>(object?[])`:
	- Configures constructor parameters to use when creating mocks of type `T`, unless explicit parameters are provided
	  during mock
	  creation via `BaseClass.WithConstructorParameters(…)`.

## Using a factory for shared behavior

Use `Mock.Factory` to create multiple mocks with a shared behavior:

```csharp
var behavior = new MockBehavior { ThrowWhenNotSetup = true };
var factory = new Mock.Factory(behavior);

var sut1 = factory.Create<IChocolateDispenser>();
var sut2 = factory.Create<ILemonadeDispenser>();
```

Using a factory allows you to create multiple mocks with identical, centrally configured behavior. This is especially
useful when you need consistent mock setups across multiple tests or for different types.

## Wrapping existing instances

You can wrap an existing instance with mock tracking using `Mock.Wrap<T>()`. This allows you to track interactions with
a real object:

```csharp
var realDispenser = new ChocolateDispenser();
var wrappedDispenser = Mock.Wrap<IChocolateDispenser>(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense("Dark", 5);

// But you can still verify interactions
wrappedDispenser.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(5)).Once();
```

**Notes:**

- Only interface types can be wrapped with `Mock.Wrap<T>()`.
- All calls are forwarded to the wrapped instance.
- You can still set up custom behavior that overrides the wrapped instance's behavior.
- Verification works the same as with regular mocks.
