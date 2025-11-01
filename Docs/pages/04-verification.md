# Verify mock interactions

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

You can verify that methods were called with specific arguments and how many times:

```csharp
// Verify that AddUser("Bob") was called at least once
mock.Verify.Invoked.AddUser("Bob").AtLeastOnce();

// Verify that TryDelete was never called with the given id and any out parameter
mock.Verify.Invoked.TryDelete(id, With.Out<User?>()).Never();

// Verify that DoSomething was called exactly twice with any int argument
mock.Verify.Invoked.DoSomething(With.Any<int>()).Exactly(2);
```

### Argument Matchers

You can use argument matchers from the `With` class to verify calls with flexible conditions:

- `With.Any<T>()` — matches any value of type `T`
- `With.Null<T>()` — matches `null`
- `With.Matching<T>(predicate)` — matches values satisfying a predicate
- `With.Value(value)` — matches a specific value
- `With.Out<T>()` — matches any out parameter of type `T`
- `With.Ref<T>()` — matches any ref parameter of type `T`
- `With.Out<T>(setter)` — matches and sets an out parameter
- `With.Ref<T>(setter)` — matches and sets a ref parameter
- `With.ValueBetween<T>(min).And(max)` — matches a value between min and max (for numeric types, .NET 8+)

Example:
```csharp
mock.Verify.Invoked.DoSomething(With.Matching<int>(x => x > 10)).Once();
mock.Verify.Invoked.DoSomething(With.ValueBetween(1).And(5)).AtLeastOnce();
```

## Properties

You can verify property gets and sets:

```csharp
// Verify that the property 'Name' was read at least once
mock.Verify.Got.Name().AtLeastOnce();

// Verify that the property 'Age' was set to 42 exactly once
mock.Verify.Set.Age(42).Once();
```

Note: The setter value also supports argument matchers.

## Indexers

You can verify indexer gets and sets:

```csharp
// Verify that the indexer was read with key "foo" exactly once
mock.Verify.GotIndexer("foo").Once();

// Verify that the indexer was set with key "bar" to value 123 at least once
mock.Verify.SetIndexer("bar", 123).AtLeastOnce();
```

Note: The keys and value also supports argument matchers.

## Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'Changed' was subscribed to at least once
mock.Verify.SubscribedTo.Changed().AtLeastOnce();

// Verify that the event 'Changed' was unsubscribed from exactly once
mock.Verify.UnsubscribedFrom.Changed().Once();
```

## Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
mock.Verify.Invoked.AddUser("Alice").Then(
    m => m.Invoked.DeleteUser("Alice")
);
```

You can chain multiple calls for strict order verification:

```csharp
mock.Verify.Invoked.DoSomething(1)
    .Then(m => m.Invoked.DoSomething(2), m => m.Invoked.DoSomething(3));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.

## Verifying All Interactions

You can check if all interactions with the mock have been verified using `ThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = mock.Verify.ThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made. If any interaction was not verified, this method returns `false`.
