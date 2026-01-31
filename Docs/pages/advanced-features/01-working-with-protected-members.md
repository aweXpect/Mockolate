# Working with protected members

Mockolate allows you to set up and verify protected virtual members on class mocks. Access protected members using the
`.Protected` property:

## Example:

```csharp
public abstract class ChocolateDispenser
{
    protected virtual bool DispenseInternal(string type, int amount) => true;
    protected virtual int InternalStock { get; set; }
}

var sut = Mock.Create<ChocolateDispenser>();
```

### Setup

```csharp
// Setup protected method
sut.SetupMock.Protected.Method.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>())
    .Returns(true);

// Setup protected property
sut.SetupMock.Protected.Property.InternalStock.InitializeWith(100);
```

**Notes:**

- Protected members can be set up and verified just like public members, using the `.Protected` accessor.
- All setup options (`.Returns()`, `.Throws()`, `.Do()`, `.InitializeWith()`, etc.) work with protected members.

### Verification

```csharp
// Verify protected method was invoked
sut.VerifyMock.Invoked.Protected.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>()).Once();

// Verify protected property was read
sut.VerifyMock.Got.Protected.InternalStock().AtLeastOnce();

// Verify protected property was set
sut.VerifyMock.Set.Protected.InternalStock(It.Is(100)).Once();

// Verify protected indexer was read
sut.VerifyMock.GotProtectedIndexer(It.Is(0)).Once();

// Verify protected indexer was set
sut.VerifyMock.SetProtectedIndexer(It.Is(0), It.Is(42)).Once();
```

**Note:**

- All verification options (argument matchers, count assertions) work the same for protected members as for public
  members.
- Protected indexers are supported using `.GotProtectedIndexer()`/`.SetProtectedIndexer()` for verification.
