# Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how many times, using the `Verify` API:

Supported call count verifications in the `Mockolate.Verify` namespace:
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
mock.Verify.Invoked.Dispense("Dark", 5).AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
mock.Verify.Invoked.Dispense("White", With.Any<int>()).Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
mock.Verify.Invoked.Dispense(With.Any<string>(), With.Any<int>()).Exactly(2);
```

### Argument Matchers

You can use argument matchers from the `With` class to verify calls with flexible conditions:

- `With.Any<T>()` — matches any value of type `T`
- `Parameter.Null<T>()` — matches `null`
- `Parameter.With<T>(predicate)` — matches values satisfying a predicate
- `Parameter.With(value)` — matches a specific value
- `Parameter.Out<T>()` — matches any out parameter of type `T`
- `Parameter.Ref<T>()` — matches any ref parameter of type `T`
- `Parameter.Out<T>(setter)` — matches and sets an out parameter
- `Parameter.Ref<T>(setter)` — matches and sets a ref parameter
- `With.ValueBetween<T>(min).And(max)` — matches a value between min and max (for numeric types, .NET 8+)

**Example:**

```csharp
mock.Verify.Invoked.Dispense(Parameter.With<string>(t => t.StartsWith("D")), With.ValueBetween(1).And(10)).Once();
mock.Verify.Invoked.Dispense("Milk", With.ValueBetween(1).And(5)).AtLeastOnce();
```

## Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
mock.Verify.Got.TotalDispensed().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
mock.Verify.Set.TotalDispensed(42).Once();
```

**Note:**  
The setter value also supports argument matchers.

## Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
mock.Verify.GotIndexer("Dark").Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
mock.Verify.SetIndexer("Milk", 7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

## Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
mock.Verify.SubscribedTo.ChocolateDispensed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
mock.Verify.UnsubscribedFrom.ChocolateDispensed().Once();
```

## Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
mock.Verify.Invoked.Dispense("Dark", 2).Then(
    m => m.Invoked.Dispense("Dark", 3)
);
```

You can chain multiple calls for strict order verification:

```csharp
mock.Verify.Invoked.Dispense("Dark", 1).Then(
    m => m.Invoked.Dispense("Milk", 2),
    m => m.Invoked.Dispense("White", 3));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.

## Check for unexpected interactions

You can check if all interactions with the mock have been verified using `ThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = mock.Verify.ThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made.
If any interaction was not verified, this method returns `false`.
