# Check for unexpected interactions

## That all interactions are verified

You can check if all interactions with the mock have been verified using `ThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = sut.VerifyMock.ThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made.
If any interaction was not verified, this method returns `false`.

## That all setups are used

You can check if all registered setups on the mock have been used using `ThatAllSetupsAreUsed`:

```csharp
// Returns true if all setups have been used
bool allUsed = sut.VerifyMock.ThatAllSetupsAreUsed();
```

This is useful for ensuring that your test setup and test execution match.
If any setup was not used, this method returns `false`.
