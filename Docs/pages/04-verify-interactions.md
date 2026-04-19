# Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how
many times, using the `Verify` API:

Supported call count verifications (in the `Mockolate.Verify` namespace):

- `.Never()`: The interaction never occurred
- `.Once()`: The interaction occurred exactly once
- `.Twice()`: The interaction occurred exactly twice
- `.Exactly(n)`: The interaction occurred exactly n times
- `.AtLeastOnce()`: The interaction occurred at least once
- `.AtLeastTwice()`: The interaction occurred at least twice
- `.AtLeast(n)`: The interaction occurred at least n times
- `.AtMostOnce()`: The interaction occurred at most once
- `.AtMostTwice()`: The interaction occurred at most twice
- `.AtMost(n)`: The interaction occurred at most n times
- `.Between(min, max)`: The interaction occurred between min and max times (inclusive)
- `.Times(predicate)`: The interaction count matches the predicate

If the invocations run in a background thread, you can use `Within(TimeSpan)` to specify a timeout in which to wait for
the expected interactions to occur:

```csharp
// Wait up to 1 second for Dispense("Dark", 5) to be invoked
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5))
    .Within(TimeSpan.FromSeconds(1))
    .AtLeastOnce();
```

You can also use `WithCancellation(CancellationToken)` to wait for the expected interactions until the cancellation
token is canceled. If you combine this with the `Within` method, both the timeout and the cancellation token are respected.

In both cases, it will block the test execution until the expected interaction occurs or the timeout is reached.
If the interaction does not occur within the specified time, a `MockVerificationException` will be thrown.

If you need truly asynchronous verification without blocking the test thread, you can use the
[aweXpect.Mockolate](https://awexpect.com/aweXpect.Mockolate) extension package which has an asynchronous `Within(TimeSpan)` variant.

## Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
sut.Mock.Verify.TotalDispensed.Got().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
sut.Mock.Verify.TotalDispensed.Set(It.Is(42)).Once();
```

**Note:**  
The setter value also supports argument matchers.

## Methods

You can verify that methods were invoked with specific arguments and how many times:

```csharp
// Verify that Dispense("Dark", 5) was invoked at least once
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5))
    .AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
sut.Mock.Verify.Dispense(It.Is("White"), It.IsAny<int>())
    .Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
sut.Mock.Verify.Dispense(Match.AnyParameters())
    .Exactly(2);

// Verify that Dispense was invoked between 3 and 5 times (inclusive)
sut.Mock.Verify.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Between(3, 5);

// Verify that Dispense was invoked an even number of times
sut.Mock.Verify.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Times(count => count % 2 == 0);
```

You can also verify that a specific setup was invoked a specific number of times:

```csharp
IMockSetup setup = sut.Mock.Setup.Dispense(It.Is("Dark"), It.Is(5)).Returns(true);
// Act
sut.Mock.VerifySetup(setup).AtLeastOnce();
```

## Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
sut.Mock.Verify[It.Is("Dark")].Got().Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
sut.Mock.Verify[It.Is("Milk")].Set(7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

## Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
sut.Mock.Verify.ChocolateDispensed.Subscribed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
sut.Mock.Verify.ChocolateDispensed.Unsubscribed().Once();
```

## Argument Matchers

You can use argument matchers from the `It` class to verify calls with flexible conditions:

- `It.IsAny<T>()`: Matches any value of type `T`.
- `It.Is<T>(value)`: Matches a specific value. With `.Using(IEqualityComparer<T>)`, you can provide a custom equality
  comparer.
- `It.IsOneOf<T>(params T[] values)`: Matches any of the given values. With `.Using(IEqualityComparer<T>)`, you can
  provide a custom equality comparer.
- `It.IsNull<T>()`: Matches null.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.IsOut<T>()`: Matches any out parameter of type `T`
- `It.IsRef<T>()`: Matches any ref parameter of type `T`
- `It.Matches<string>(pattern)`: Matches strings using wildcard patterns (`*` and `?`). With `.AsRegex()`, you can use
  regular expressions instead.
- `It.Contains<T>(item)`: Matches a collection parameter that contains `item`. With `.Using(IEqualityComparer<T>)`, you
  can provide a custom equality comparer.
- `It.SequenceEquals<T>(params IEnumerable<T> values)`: Matches a collection parameter whose elements equal `values` in
  order. With `.Using(IEqualityComparer<T>)`, you can provide a custom equality comparer.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

**Example:**

```csharp
sut.Mock.Verify.Dispense(It.Is<string>(t => t.StartsWith("D")), It.IsAny<int>()).Once();
sut.Mock.Verify.Dispense(It.Is("Milk"), It.IsAny<int>()).AtLeastOnce();
```

## Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(2)).Then(
    m => m.Dispense(It.Is("Dark"), It.Is(3))
);
```

You can chain multiple calls for strict order verification:

```csharp
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(1)).Then(
    m => m.Dispense(It.Is("Milk"), It.Is(2)),
    m => m.Dispense(It.Is("White"), It.Is(3)));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.
