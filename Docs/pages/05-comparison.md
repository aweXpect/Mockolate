# Comparison

This page compares **Mockolate** with other .NET mocking libraries:
**[Moq](https://github.com/devlooped/moq)**,
**[NSubstitute](https://nsubstitute.github.io/)**, and
**[FakeItEasy](https://fakeiteasy.github.io/)** with the example
interface:

```csharp
public delegate void ChocolateDispensedDelegate(string type, int amount);
public interface IChocolateDispenser
{
    int this[string type] { get; set; }
    int TotalDispensed { get; set; }
    bool Dispense(string type, int amount);
    event ChocolateDispensedDelegate ChocolateDispensed;
}
```

## Setup

**Mockolate**

```csharp
IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
sut.SetupMock.Method.Dispense(With("Dark"), With(2)).Returns(true);
sut.SetupMock.Indexer(With("Dark")).Returns(10);
```

**Moq**

```csharp
Mock<IChocolateDispenser> mock = new Moq.Mock<IChocolateDispenser>();
mock.Setup(x => x.Dispense("Dark", 2)).Returns(true);
mock.SetupProperty(x => x["Dark"], 10);
```

**NSubstitute**

```csharp
IChocolateDispenser substitute = Substitute.For<IChocolateDispenser>();
substitute.Dispense("Dark", 2).Returns(true);
substitute["Dark"].Returns(10);
```

**FakeItEasy**

```csharp
IChocolateDispenser fake = A.Fake<IChocolateDispenser>();
A.CallTo(() => fake.Dispense("Dark", 2)).Returns(true);
A.CallTo(() => fake["Dark"]).Returns(10);
```

## Usage

**Mockolate**

```csharp
int available = sut["Dark"];
bool success = sut.Dispense("Dark", 2);
sut.RaiseOnMock.ChocolateDispensed("Dark", 2);
```

**Moq**

```csharp
int available = mock.Object["Dark"];
bool success = mock.Object.Dispense("Dark", 2);
mock.Raise(m => m.ChocolateDispensed += null, "Dark", 2);
```

**NSubstitute**

```csharp
int available = substitute["Dark"];
bool success = substitute.Dispense("Dark", 2);
substitute.ChocolateDispensed += Raise.Event<ChocolateDispensedDelegate>("Dark", 2);
```

**FakeItEasy**

```csharp
int available = fake["Dark"];
bool success = fake.Dispense("Dark", 2);
fake.ChocolateDispensed += Raise.FreeForm.With("Dark", 2);
```

## Verification

**Mockolate**

```csharp
sut.VerifyMock.Invoked.Dispense(With("Dark"), With(2)).Once();
sut.VerifyMock.GotIndexer(With("Dark")).Once();
```

**Moq**

```csharp
mock.Verify(x => x.Dispense("Dark", 2), Times.Once());
mock.VerifyGet(x => x["Dark"], Times.Once());
```

**NSubstitute**

```csharp
substitute.Received(1).Dispense("Dark", 2);
_ = substitute.Received(1)["Dark"];
```

**FakeItEasy**

```csharp
A.CallTo(() => fake.Dispense("Dark", 2)).MustHaveHappenedOnceExactly();
A.CallTo(() => fake["Dark"]).MustHaveHappenedOnceExactly();
```
