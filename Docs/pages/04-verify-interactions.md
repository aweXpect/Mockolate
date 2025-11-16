# Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how
many times, using the `Verify` API:

Supported call count verifications in the `Mockolate.VerifyMock` namespace:

- `.Never()`
- `.Once()`
- `.Twice()`
- `.Exactly(n)`
- `.AtLeastOnce()`
- `.AtLeastTwice()`
- `.AtLeast(n)`
- `.AtMostOnce()`
- `.AtMostTwice()`
- `.AtMost(n)`

## Methods

You can verify that methods were invoked with specific arguments and how many times:

```csharp
// Verify that Dispense("Dark", 5) was invoked at least once
mock.VerifyMock.Invoked.Dispense(With("Dark"), With(5)).AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
mock.VerifyMock.Invoked.Dispense(With("White"), Any<int>()).Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
mock.VerifyMock.Invoked.Dispense(AnyParameters()).Exactly(2);
```

### Argument Matchers

You can use argument matchers from the `Match` class to verify calls with flexible conditions:

- `Match.Any<T>()`: matches any value of type `T`
- `Match.Null<T>()`: matches `null`
- `Match.With<T>(predicate)`: matches values satisfying a predicate
- `Match.With(value)`: matches a specific value
- `Match.Out<T>()`: matches any out parameter of type `T`
- `Match.Ref<T>()`: matches any ref parameter of type `T`

**Example:**

```csharp
mock.VerifyMock.Invoked.Dispense(With<string>(t => t.StartsWith("D")), Any<int>()).Once();
mock.VerifyMock.Invoked.Dispense(With("Milk"), Any<int>()).AtLeastOnce();
```

## Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
mock.VerifyMock.Got.TotalDispensed().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
mock.VerifyMock.Set.TotalDispensed(With(42)).Once();
```

**Note:**  
The setter value also supports argument matchers.

## Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
mock.VerifyMock.GotIndexer(With("Dark")).Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
mock.VerifyMock.SetIndexer(With("Milk"), 7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

## Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
mock.VerifyMock.SubscribedTo.ChocolateDispensed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
mock.VerifyMock.UnsubscribedFrom.ChocolateDispensed().Once();
```

## Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
mock.VerifyMock.Invoked.Dispense(With("Dark"), With(2)).Then(
    m => m.Invoked.Dispense(With("Dark"), With(3))
);
```

You can chain multiple calls for strict order verification:

```csharp
mock.VerifyMock.Invoked.Dispense(With("Dark"), With(1)).Then(
    m => m.Invoked.Dispense(With("Milk"), With(2)),
    m => m.Invoked.Dispense(With("White"), With(3)));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.

## Check for unexpected interactions

You can check if all interactions with the mock have been verified using `ThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = mock.VerifyMock.ThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made.
If any interaction was not verified, this method returns `false`.
