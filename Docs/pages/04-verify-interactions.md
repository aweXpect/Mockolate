# Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how
many times, using the `Verify` API:

Supported call count verifications in the `Mockolate.VerifyMock` namespace:

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

## Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
sut.VerifyMock.Got.TotalDispensed().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
sut.VerifyMock.Set.TotalDispensed(It.Is(42)).Once();
```

**Note:**  
The setter value also supports argument matchers.

## Methods

You can verify that methods were invoked with specific arguments and how many times:

```csharp
// Verify that Dispense("Dark", 5) was invoked at least once
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(5))
    .AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
sut.VerifyMock.Invoked.Dispense(It.Is("White"), It.IsAny<int>())
    .Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
sut.VerifyMock.Invoked.Dispense(Match.AnyParameters())
    .Exactly(2);

// Verify that Dispense was invoked between 3 and 5 times (inclusive)
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Between(3, 5);

// Verify that Dispense was invoked an even number of times
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Times(count => count % 2 == 0);
```

## Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
sut.VerifyMock.GotIndexer(It.Is("Dark")).Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
sut.VerifyMock.SetIndexer(It.Is("Milk"), 7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

## Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
sut.VerifyMock.SubscribedTo.ChocolateDispensed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
sut.VerifyMock.UnsubscribedFrom.ChocolateDispensed().Once();
```

## Argument Matchers

You can use argument matchers from the `Match` class to verify calls with flexible conditions:

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
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

**Example:**

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is<string>(t => t.StartsWith("D")), It.IsAny<int>()).Once();
sut.VerifyMock.Invoked.Dispense(It.Is("Milk"), It.IsAny<int>()).AtLeastOnce();
```

## Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(2)).Then(
    m => m.Invoked.Dispense(It.Is("Dark"), It.Is(3))
);
```

You can chain multiple calls for strict order verification:

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(1)).Then(
    m => m.Invoked.Dispense(It.Is("Milk"), It.Is(2)),
    m => m.Invoked.Dispense(It.Is("White"), It.Is(3)));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.
