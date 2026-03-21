# Working with protected members

Mockolate allows you to set up and verify protected virtual members on class mocks. Protected members are accessed
the same way as public members — no separate accessor is needed.

**Example**

```csharp
public abstract class ChocolateDispenser
{
    protected virtual bool DispenseInternal(string type, int amount) => true;
    protected virtual int InternalStock { get; set; }
}

ChocolateDispenser sut = ChocolateDispenser.CreateMock();
```

## Setup

```csharp
// Setup protected method
sut.Mock.Setup.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>())
    .Returns(true);

// Setup protected property
sut.Mock.Setup.InternalStock.InitializeWith(100);
```

**Notes:**

- Protected members can be set up and verified just like public members.
- All setup options (`.Returns()`, `.Throws()`, `.Do()`, `.InitializeWith()`, etc.) work with protected members.

## Verification

```csharp
// Verify protected method was invoked
sut.Mock.Verify.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>()).Once();

// Verify protected property was read
sut.Mock.Verify.InternalStock.Got().AtLeastOnce();

// Verify protected property was set
sut.Mock.Verify.InternalStock.Set(It.Is(100)).Once();

// Verify protected indexer was read
sut.Mock.Verify[It.Is(0)].Got().Once();

// Verify protected indexer was set
sut.Mock.Verify[It.Is(0)].Set(It.Is(42)).Once();
```

**Note:**

- All verification options (argument matchers, count assertions) work the same for protected members as for public
  members.
- Protected indexers are supported using the same C# indexer syntax on `sut.Mock.Verify[...]`.
