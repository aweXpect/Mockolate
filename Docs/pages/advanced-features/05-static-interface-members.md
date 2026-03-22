# Static interface members (.NET 8+)

Mockolate supports mocking static abstract and static virtual members on interfaces (.NET 8+). Static member
invocations use async-flow scoping, meaning each mock instance has its own isolated static member context, this makes parallel test execution safe.

Static members can be set up, raised, and verified just like instance members, but through the `Mock.SetupStatic`, `Mock.RaiseStatic`, and `Mock.VerifyStatic` properties:

```csharp
// Setup static members
sut.Mock.SetupStatic.AbstractStaticMethod().Returns("some-value");
sut.Mock.SetupStatic.AbstractStaticProperty.Returns("some-value");

// Raise static events
sut.Mock.RaiseStatic.AbstractStaticEvent(value);

// Verify static interactions
sut.Mock.VerifyStatic.AbstractStaticMethod().Once();
sut.Mock.VerifyStatic.AbstractStaticProperty.Got().Once();
sut.Mock.VerifyStatic.AbstractStaticEvent.Subscribed().Once();
```

**Notes:**

- Static member scoping is implemented via `AsyncLocal<MockRegistration>`. When you call
  `sut.Mock.SetupStatic.Method()`, it creates an async-flow scope that routes static member invocations to that
  specific mock instance.
- Each mock instance has an independent static member context, so parallel tests will not interfere with each other.
