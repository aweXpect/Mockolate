# Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, provide constructor
arguments as an array to `CreateMock([…])`:

```csharp
// Create a mock of an interface
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

// Create a mock of a class
MyChocolateDispenser classMock = MyChocolateDispenser.CreateMock();

// For classes without a default constructor:
MyChocolateDispenserWithCtor classWithArgsMock = MyChocolateDispenserWithCtor.CreateMock(["Dark", 42]);
```

## Customizing mock behavior

You can control the default behavior of the mock by providing a `MockBehavior`:

```csharp
IChocolateDispenser strictMock = IChocolateDispenser.CreateMock(MockBehavior.Default.ThrowingWhenNotSetup());

// For classes with constructor parameters and custom behavior:
MockBehavior behavior = new MockBehavior { ThrowWhenNotSetup = true };
MyChocolateDispenser classMock = MyChocolateDispenser.CreateMock(["Dark", 42], behavior);
```

**`MockBehavior` options**

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
    MockBehavior behavior = MockBehavior.Default
      .WithDefaultValueFor<string>(() => "default")
      .WithDefaultValueFor<int>(() => 42);
    IChocolateDispenser sut = IChocolateDispenser.CreateMock(behavior);
    ```
    This is useful when you want mocks to return specific default values for certain types instead of the standard
    defaults.
- `Initialize<T>(params Action<IMockSetup<T>>[] setups)`:
  - Automatically initialize all mocks of type T with the given setups when they are created.
- `UseConstructorParametersFor<T>(object?[])`:
  - Configures constructor parameters to use when creating mocks of type `T`, unless explicit parameters are provided
    during mock creation via `CreateMock([…])`.

**Using a shared behavior**

You can reuse a `MockBehavior` instance across multiple mock creations to apply consistent, centrally configured
behavior:

```csharp
MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

IChocolateDispenser sut1 = IChocolateDispenser.CreateMock(behavior);
ILemonadeDispenser sut2 = ILemonadeDispenser.CreateMock(behavior);
```

This is especially useful when you need consistent mock setups across multiple tests or for different types.

## Setups

Specify setups during mock creation using the `CreateMock` overload with a setup callback. These setups also apply to virtual interactions in the constructor.

## Implementing additional interfaces

You can specify additional interfaces that the mock also implements using `.Implementing<T>()`:

```csharp
// return type is a MyChocolateDispenser that also implements ILemonadeDispenser
MyChocolateDispenser sut = MyChocolateDispenser.CreateMock().Implementing<ILemonadeDispenser>();

// Create a mock implementing multiple interfaces with inline setups
IChocolateDispenser sut2 = IChocolateDispenser.CreateMock()
    .Implementing<ILemonadeDispenser>(setup => setup.DispenseLemonade(It.IsAny<int>()).Returns(true));
```

**Notes:**

- Only the first type can be a class; additional types must be interfaces.

## Wrapping existing instances

You can wrap an existing instance with mock tracking using `.Wrapping(instance)`. This allows you to track interactions
with a real object:

```csharp
MyChocolateDispenser realDispenser = new MyChocolateDispenser();
IChocolateDispenser wrappedDispenser = IChocolateDispenser.CreateMock().Wrapping(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense("Dark", 5);

// But you can still verify interactions
wrappedDispenser.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5)).Once();
```

**Notes:**

- Both interface and class types can be wrapped.
- All public calls are forwarded to the wrapped instance.
- You can still set up custom behavior that overrides the wrapped instance's behavior.
- Protected members are not forwarded to the wrapped instance; the base class implementation is used instead.
- Verification works the same as with regular mocks.
