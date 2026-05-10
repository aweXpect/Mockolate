# Static interface members

Mockolate supports mocking static abstract and static virtual members on interfaces (.NET 8+). Static member
invocations use async-flow scoping, meaning each mock instance has its own isolated static member context, this makes parallel test execution safe.

Static members can be set up, raised, and verified just like instance members, but through the `Mock.SetupStatic`, `Mock.RaiseStatic`, and `Mock.VerifyStatic` properties:

**Example**

```csharp
public interface IChocolateFactory
{
    static abstract string DefaultRecipe { get; set; }
    static abstract int ProduceBatch(string type, int amount);
    static abstract event Action<int> BatchCompleted;
}

IChocolateFactory sut = IChocolateFactory.CreateMock();

// Setup static members
sut.Mock.SetupStatic.ProduceBatch(It.Is("Dark"), It.IsAny<int>()).Returns(42);
sut.Mock.SetupStatic.DefaultRecipe.Returns("Dark");

// Static abstract members can only be invoked through a generic type parameter,
// so route the call through a helper constrained to the interface and pass the
// generated mock type (Mock.IChocolateFactory) as the type argument.
string recipe = ReadRecipe<Mock.IChocolateFactory>();
int produced = Produce<Mock.IChocolateFactory>("Dark", 10);
Subscribe<Mock.IChocolateFactory>(amount => Console.WriteLine($"Batch of {amount} ready"));

// Raise static events to the registered handlers
sut.Mock.RaiseStatic.BatchCompleted(produced);

// Verify static interactions
sut.Mock.VerifyStatic.ProduceBatch(It.Is("Dark"), It.IsAny<int>()).Once();
sut.Mock.VerifyStatic.DefaultRecipe.Got().Once();
sut.Mock.VerifyStatic.BatchCompleted.Subscribed().Once();

static string ReadRecipe<T>() where T : IChocolateFactory
    => T.DefaultRecipe;
static int Produce<T>(string type, int amount) where T : IChocolateFactory
    => T.ProduceBatch(type, amount);
static void Subscribe<T>(Action<int> handler) where T : IChocolateFactory
    => T.BatchCompleted += handler;
```

**Notes:**

- Static member scoping is implemented via `AsyncLocal<MockRegistry>`. When you call
  `sut.Mock.SetupStatic.Method()`, it creates an async-flow scope that routes static member invocations to that
  specific mock instance.
- Each mock instance has an independent static member context, so parallel tests will not interfere with each other.
