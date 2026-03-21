# Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, use
array syntax to provide constructor arguments:

```csharp
// Create a mock for an interface
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

// Create a mock for a class
MyChocolateDispenser classMock = MyChocolateDispenser.CreateMock();

// For classes without a default constructor:
MyChocolateDispenserWithCtor classWithArgsMock = MyChocolateDispenserWithCtor.CreateMock(["Dark", 42]);
```

You can specify up to eight additional interfaces that the mock also implements (beyond the first type):

```csharp
// return type is a MyChocolateDispenser that also implements ILemonadeDispenser
MyChocolateDispenser sut = MyChocolateDispenser.CreateMock().Implementing<ILemonadeDispenser>();
```

**Notes:**

- Only the first generic type can be a class; additional types must be interfaces.
- Sealed classes cannot be mocked and will throw a `MockException`.

## Customizing mock behavior

You can control the default behavior of the mock by providing a `MockBehavior`:

```csharp
IChocolateDispenser strictMock = IChocolateDispenser.CreateMock(MockBehavior.Default.ThrowingWhenNotSetup());

// For classes with constructor parameters and custom behavior:
MyChocolateDispenser classMock = MyChocolateDispenser.CreateMock(
    ["Dark", 42],
    new MockBehavior { ThrowWhenNotSetup = true }
);
```

### `MockBehavior` options

- `SkipBaseClass` (bool):
  - If `false` (default), the mock will call the base class implementation and use its return values as default
    values, if no explicit setup is defined.
  - If `true`, the mock will not call any base class implementations.
- `ThrowWhenNotSetup` (bool):
  - If `false` (default), the mock will return a default value (see `DefaultValue`), when no matching setup is found.
  - If `true`, the mock will throw an exception when no matching setup is found.
- `DefaultValue` (IDefaultValueGenerator):
  - Customizes how default values are generated for methods/properties that are not set up.
  - The default implementation provides sensible defaults for the most common use cases:
    - Empty collections for collection types (e.g., `IEnumerable<T>`, `List<T>`, etc.)
    - Empty string for `string`
    - Completed tasks for `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>`
    - Tuples with recursively defaulted values
    - `null` for other reference types
  - You can add custom default value factories for specific types using `.WithDefaultValueFor<T>()`:
    ```csharp
    var behavior = MockBehavior.Default
        .WithDefaultValueFor<string>(() => "default")
        .WithDefaultValueFor<int>(() => 42);
    IChocolateDispenser sut = IChocolateDispenser.CreateMock(behavior);
    ```
    This is useful when you want mocks to return specific default values for certain types instead of the standard
    defaults.
- `Initialize<T>(params Action<IMockSetup<T>>[] setups)`:
  - Automatically initialize all mocks of type T with the given setups when they are created.
  - The callback can optionally receive an additional counter parameter, allowing you to differentiate between multiple
    automatically created instances.
    For example, when initializing `IDbConnection` mocks, you can use the counter to assign different database names or
    connection strings to each mock so they can be verified independently.
- `UseConstructorParametersFor<T>(object?[])`:
  - Configures constructor parameters to use when creating mocks of type `T`, unless explicit parameters are provided
    during mock creation via array syntax.

## Using a shared behavior

You can create multiple mocks with a shared `MockBehavior` by reusing the same instance:

```csharp
MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

IChocolateDispenser sut1 = IChocolateDispenser.CreateMock(behavior);
ILemonadeDispenser sut2 = ILemonadeDispenser.CreateMock(behavior);
```

Using the same `MockBehavior` instance allows you to create multiple mocks with identical, centrally configured
behavior. This is especially useful when you need consistent mock setups across multiple tests or for different types.

## Wrapping existing instances

You can wrap an existing instance with mock tracking using `.Wrapping()`. This allows you to track interactions with
a real object:

```csharp
var realDispenser = new MyChocolateDispenser();
IChocolateDispenser wrappedDispenser = IChocolateDispenser.CreateMock().Wrapping(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense("Dark", 5);

// But you can still verify interactions
wrappedDispenser.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5)).Once();
```

**Notes:**

- Only interface types can be wrapped with `.Wrapping()`.
- All calls are forwarded to the wrapped instance.
- You can still set up custom behavior that overrides the wrapped instance's behavior.
- You cannot override protected members of the wrapped instance.
- Verification works the same as with regular mocks.
