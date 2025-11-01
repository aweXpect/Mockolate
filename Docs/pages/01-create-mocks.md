# Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, use `BaseClass.WithConstructorParameters(…)` to provide constructor arguments:

```csharp
// Create a mock for an interface
var mock = Mock.Create<IChocolateDispenser>();

// Create a mock for a class
var classMock = Mock.Create<MyChocolateDispenser>();

// For classes without a default constructor:
var classWithArgsMock = Mock.Create<MyChocolateDispenserWithCtor>(
    BaseClass.WithConstructorParameters("Dark", 42)
);

// Specify up to 8 additional interfaces for the mock:
var mock3 = factory.Create<MyChocolateDispenser, ILemonadeDispenser>();
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

### `MockBehavior` options

- `ThrowWhenNotSetup` (bool):
  - If `true`, the mock will throw an exception when a method or property is called without a setup.
  - If `false`, the mock will return a default value (see `DefaultValue`).
- `BaseClassBehavior` (enum):
  - Controls how the mock interacts with base class members. Options:
    - `DoNotCallBaseClass`: Do not call base class implementation (default).
    - `OnlyCallBaseClass`: Only call base class implementation.
    - `UseBaseClassAsDefaultValue`: Use base class as a fallback for default values.
- `DefaultValue` (IDefaultValueGenerator):
  - Customizes how default values are generated for methods/properties that are not set up.

## Using a factory for shared behavior

Use `Mock.Factory` to create multiple mocks with a shared behavior:

```csharp
var behavior = new MockBehavior { ThrowWhenNotSetup = true };
var factory = new Mock.Factory(behavior);

var mock1 = factory.Create<IChocolateDispenser>();
var mock2 = factory.Create<ILemonadeDispenser>();
```
