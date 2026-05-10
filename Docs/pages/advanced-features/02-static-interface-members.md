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

// Raise static events
sut.Mock.RaiseStatic.BatchCompleted(42);

// Verify static interactions
sut.Mock.VerifyStatic.ProduceBatch(It.Is("Dark"), It.IsAny<int>()).Once();
sut.Mock.VerifyStatic.DefaultRecipe.Got().Once();
sut.Mock.VerifyStatic.BatchCompleted.Subscribed().Once();
```

**Notes:**

- Static member scoping is implemented via `AsyncLocal<MockRegistry>`. When you call
  `sut.Mock.SetupStatic.Method()`, it creates an async-flow scope that routes static member invocations to that
  specific mock instance.
- Each mock instance has an independent static member context, so parallel tests will not interfere with each other.
