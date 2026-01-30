#!/bin/bash
# Script to create GitHub issues for undocumented Mockolate features
# Usage: ./create_documentation_issues.sh

REPO="aweXpect/Mockolate"

echo "Creating GitHub issues for Mockolate documentation gaps..."
echo "Repository: $REPO"
echo ""

# High Priority Issues

echo "Creating Issue 1: Document Mock.Wrap Feature..."
gh issue create --repo "$REPO" \
  --title "Document Mock.Wrap feature" \
  --label "documentation,enhancement" \
  --body "## Description

The \`Mock.Wrap<T>(instance)\` feature is completely missing from the README.md documentation.

## Missing Documentation

- \`Mock.Wrap<T>(instance)\` - Wrap existing instances with mock tracking
- Forward calls to real instance while tracking interactions
- Verify interactions on wrapped instances

## Suggested Documentation

Add a new subsection under \"Create mocks\" titled \"Wrapping Existing Instances\":

\`\`\`markdown
### Wrapping Existing Instances

You can wrap an existing instance with mock tracking using \`Mock.Wrap<T>()\`. This allows you to track interactions with a real object:

\`\`\`csharp
var realDispenser = new ChocolateDispenser();
var wrappedDispenser = Mock.Wrap<IChocolateDispenser>(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense(\"Dark\", 5);

// But you can still verify interactions
wrappedDispenser.VerifyMock.Invoked.Dispense(It.Is(\"Dark\"), It.Is(5)).Once();
\`\`\`
\`\`\`

## Priority

**High** - This is a core feature that is completely undocumented.

## Related Files

- README.md
- Docs/index.md (if applicable)"

echo "Created Issue 1 ✓"
echo ""

echo "Creating Issue 2: Document Delegate Mocking..."
gh issue create --repo "$REPO" \
  --title "Document delegate mocking feature" \
  --label "documentation,enhancement" \
  --body "## Description

While delegates are mentioned in the main example, delegate mocking is not properly documented as a standalone feature.

## Missing Documentation

- Mock \`Action<T>\`, \`Func<T>\`, and custom delegates
- \`SetupMock.Delegate()\` for setup
- \`VerifyMock.Invoked()\` for delegate verification
- Full parameter matching support for delegates

## Suggested Documentation

Add a new section titled \"Mocking Delegates\":

\`\`\`markdown
### Mocking Delegates

Mockolate supports mocking delegates including \`Action<T>\`, \`Func<T>\`, and custom delegates:

\`\`\`csharp
// Create a mock delegate
var myDelegate = Mock.Create<Action<string, int>>();

// Setup the delegate
myDelegate.SetupMock.Delegate(It.Is(\"Dark\"), It.IsAny<int>())
    .Do((type, amount) => Console.WriteLine(\$\"Dispensed {amount} {type}\"));

// Invoke the delegate
myDelegate(\"Dark\", 5);

// Verify invocation
myDelegate.VerifyMock.Invoked(It.Is(\"Dark\"), It.Is(5)).Once();
\`\`\`

Custom delegates are also supported:
\`\`\`csharp
var customDelegate = Mock.Create<ChocolateDispensedDelegate>();
customDelegate.SetupMock.Delegate(It.IsAny<string>(), It.IsAny<int>())
    .Do((type, amount) => { /* handler */ });
\`\`\`
\`\`\`

## Priority

**High** - Delegates are a common C# construct that many users will want to mock.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 2 ✓"
echo ""

echo "Creating Issue 3: Document Advanced Parameter Matching..."
gh issue create --repo "$REPO" \
  --title "Document advanced parameter matching (ref/out/span parameters)" \
  --label "documentation,enhancement" \
  --body "## Description

Several advanced parameter matching features are missing from the Parameter Matching section.

## Missing Documentation

- **Ref/Out parameters**: \`It.IsRef<T>()\`, \`It.IsOut<T>()\`, \`It.IsAnyRef<T>()\`, \`It.IsAnyOut<T>()\`
- **Span parameters**: \`It.IsSpan<T>()\`, \`It.IsReadOnlySpan<T>()\`, \`It.IsAnySpan<T>()\`, \`It.IsAnyReadOnlySpan<T>()\`
- **Regex matching**: \`.AsRegex()\` extension on \`It.Matches()\`
- **Custom equality comparers**: \`.Using(IEqualityComparer<T>)\` for \`It.Is()\` and \`It.IsOneOf()\`
- **Custom parameter predicates**: \`Match.Parameters(Func<NamedParameterValue[], bool>)\`

## Suggested Documentation

Expand the \"Parameter Matching\" section with subsections for:

1. **Ref and Out Parameters**
2. **Span Parameters** 
3. **Regular Expression Matching**
4. **Custom Equality Comparers**

See DOCUMENTATION_GAPS.md for detailed examples.

## Priority

**High** - These are critical for scenarios involving ref/out parameters and modern C# features like Span<T>.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 3 ✓"
echo ""

echo "Creating Issue 4: Document Protected Member Support..."
gh issue create --repo "$REPO" \
  --title "Document protected member support" \
  --label "documentation,enhancement" \
  --body "## Description

Mockolate supports setting up and verifying protected members, but this is completely undocumented.

## Missing Documentation

- \`SetupMock.Protected.Method()\` and \`.Property()\`
- \`VerifyMock.Protected.Invoked()\`, \`.Got()\`, \`.Set()\`
- Examples of working with protected methods and properties

## Suggested Documentation

Add a new section titled \"Working with Protected Members\":

\`\`\`markdown
### Working with Protected Members

Mockolate allows you to setup and verify protected methods and properties:

\`\`\`csharp
var sut = Mock.Create<MyChocolateDispenser>();

// Setup protected method
sut.SetupMock.Protected.Method(\"DispenseInternal\",
    It.Is(\"Dark\"), It.IsAny<int>())
    .Returns(true);

// Setup protected property
sut.SetupMock.Protected.Property(\"InternalStock\").InitializeWith(100);

// Verify protected method was called
sut.VerifyMock.Protected.Invoked(\"DispenseInternal\",
    It.Is(\"Dark\"), It.IsAny<int>()).Once();

// Verify protected property was accessed
sut.VerifyMock.Protected.Got(\"InternalStock\").AtLeastOnce();
\`\`\`
\`\`\`

## Priority

**High** - Important for testing classes with protected members.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 4 ✓"
echo ""

# Medium Priority Issues

echo "Creating Issue 5: Document Advanced Callback Features..."
gh issue create --repo "$REPO" \
  --title "Document advanced callback features" \
  --label "documentation,enhancement" \
  --body "## Description

Several advanced callback control features are missing from the callback documentation.

## Missing Documentation

- \`.When(Func<int, bool>)\` - Conditional callback execution
- \`.For(int times)\` - Execute callback N times
- \`.Only(int times)\` - Execute callback up to N times
- \`.InParallel()\` - Execute callback in parallel
- Invocation counter in callbacks (\`Action<int>\`)

## Suggested Documentation

Expand the callback sections with subsections for:

1. **Conditional Callbacks** (\`.When()\`)
2. **Frequency Control** (\`.For()\`, \`.Only()\`)
3. **Parallel Callbacks** (\`.InParallel()\`)
4. **Invocation Counter**

See DOCUMENTATION_GAPS.md for detailed examples.

## Priority

**Medium** - Enhances existing callback documentation.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 5 ✓"
echo ""

echo "Creating Issue 6: Document Advanced Property and Indexer Features..."
gh issue create --repo "$REPO" \
  --title "Document advanced property and indexer features" \
  --label "documentation,enhancement" \
  --body "## Description

Several advanced property and indexer features are missing from the documentation.

## Missing Documentation

- \`.Returns(Func<T, T>)\` - Return value based on previous value
- \`.OnGet.Do(Action<int, T>)\` - Getter callback with counter and value
- \`.OnSet.Do(Action<int, T>)\` - Setter callback with counter and value
- \`.Register()\` - Register setup without value

## Suggested Documentation

Expand the Property Setup and Indexer Setup sections with:

1. **Advanced Property Returns** (with previous value)
2. **Advanced Callbacks** (with counter and value)
3. **Register Without Value**

See DOCUMENTATION_GAPS.md for detailed examples.

## Priority

**Medium** - Completes existing property and indexer documentation.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 6 ✓"
echo ""

echo "Creating Issue 7: Document Additional Verification Features..."
gh issue create --repo "$REPO" \
  --title "Document additional verification count methods" \
  --label "documentation,enhancement" \
  --body "## Description

Some verification count methods are missing from the documentation.

## Missing Documentation

- \`.Between(min, max)\` - Between min and max times (inclusive)
- \`.Times(Func<int, bool>)\` - Custom predicate verification

## Suggested Documentation

Add these methods to the \"Verify interactions\" section under supported call count verifications:

\`\`\`markdown
- \`.Between(min, max)\` - Between min and max times (inclusive)
- \`.Times(predicate)\` - Custom predicate

**Examples:**

\`\`\`csharp
// Between min and max (inclusive)
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Between(3, 5);

// Custom predicate
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Times(count => count % 2 == 0);  // Even number of calls
\`\`\`
\`\`\`

## Priority

**Medium** - Rounds out verification options.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 7 ✓"
echo ""

echo "Creating Issue 8: Document Extended HttpClient Features..."
gh issue create --repo "$REPO" \
  --title "Document extended HttpClient features" \
  --label "documentation,enhancement" \
  --body "## Description

The HttpClient section is missing documentation for several matchers and HTTP methods.

## Missing Documentation

- \`It.IsBinaryContent()\` matcher
- \`.ForHttps()\`, \`.ForHttp()\` URI scheme filters
- \`.WithBodyMatching()\` content body matching (appears in example but not explained)
- PUT, DELETE, PATCH methods (only GET and POST shown)
- CancellationToken support

## Suggested Documentation

Expand the \"Mocking HttpClient\" section with subsections for:

1. **All HTTP Methods** (PUT, DELETE, PATCH)
2. **Binary Content Matching**
3. **URI Scheme Filtering**
4. **Content Body Matching** (explain the feature shown in example)
5. **CancellationToken Support**

See DOCUMENTATION_GAPS.md for detailed examples.

## Priority

**Medium** - Completes HttpClient section which is already documented.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 8 ✓"
echo ""

# Low Priority Issues

echo "Creating Issue 9: Document Interaction Tracking..."
gh issue create --repo "$REPO" \
  --title "Document interaction tracking features" \
  --label "documentation,enhancement" \
  --body "## Description

Interaction tracking features are completely undocumented.

## Missing Documentation

- \`.Interactions\` property - Access all recorded interactions
- \`.ClearAllInteractions()\` - Clear interaction history
- \`MockMonitor.Run()\` - Session-based monitoring

## Suggested Documentation

Add a new section titled \"Interaction Tracking\":

\`\`\`markdown
### Interaction Tracking

Mockolate tracks all interactions with mocks. You can access and manage these interactions:

\`\`\`csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense(\"Dark\", 5);
sut.Dispense(\"Milk\", 3);

// Access all interactions
foreach (var interaction in sut.Interactions)
{
    Console.WriteLine(\$\"Interaction: {interaction}\");
}

// Clear interaction history
sut.ClearAllInteractions();
\`\`\`

#### Mock Monitoring Sessions

Use \`MockMonitor.Run()\` to track interactions within a specific scope:

\`\`\`csharp
using (var monitor = MockMonitor.Run())
{
    sut.Dispense(\"Dark\", 5);
    
    // Access interactions within this session
    var interactions = monitor.GetInteractions();
}
\`\`\`
\`\`\`

## Priority

**Low** - Advanced debugging feature for specific use cases.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 9 ✓"
echo ""

echo "Creating Issue 10: Document Advanced Mock Behavior Options..."
gh issue create --repo "$REPO" \
  --title "Document advanced mock behavior options" \
  --label "documentation,enhancement" \
  --body "## Description

Several advanced mock behavior configuration options are missing from the documentation.

## Missing Documentation

- \`.WithDefaultValueFor<T>()\` - Custom default value factories
- \`.Initialize<T>(Action<int, IMockSetup<T>>)\` - Initialize with counter
- \`.UseConstructorParametersFor<T>()\` - Constructor parameter configuration

## Suggested Documentation

Expand the \"Customizing mock behavior\" section with subsection \"Advanced Mock Behavior\":

See DOCUMENTATION_GAPS.md for detailed examples.

## Priority

**Low** - Advanced edge cases.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 10 ✓"
echo ""

echo "Creating Issue 11: Document Special Method Overrides..."
gh issue create --repo "$REPO" \
  --title "Document special method overrides (ToString, Equals, GetHashCode)" \
  --label "documentation,enhancement" \
  --body "## Description

The ability to override \`ToString()\`, \`Equals()\`, and \`GetHashCode()\` is completely undocumented.

## Missing Documentation

- Override \`ToString()\`, \`Equals()\`, \`GetHashCode()\` on mocks
- Customize mock identity and string representation

## Suggested Documentation

Add a new section or subsection titled \"Overriding Special Methods\":

\`\`\`markdown
### Overriding Special Methods

You can override \`ToString()\`, \`Equals()\`, and \`GetHashCode()\` on your mocks:

\`\`\`csharp
var sut = Mock.Create<IChocolateDispenser>();

// Override ToString
sut.SetupMock.Method.ToString()
    .Returns(\"Custom Mock Dispenser\");

// Override Equals
sut.SetupMock.Method.Equals(It.IsAny<object>())
    .Returns((obj) => ReferenceEquals(sut, obj));

// Override GetHashCode
sut.SetupMock.Method.GetHashCode()
    .Returns(12345);

Console.WriteLine(sut.ToString());  // \"Custom Mock Dispenser\"
\`\`\`
\`\`\`

## Priority

**Low** - Rare use case.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 11 ✓"
echo ""

echo "Creating Issue 12: Document Async Exception Handling..."
gh issue create --repo "$REPO" \
  --title "Document async exception handling with ThrowsAsync" \
  --label "documentation,enhancement" \
  --body "## Description

The \`.ThrowsAsync<TException>()\` method is missing from the async documentation.

## Missing Documentation

- \`.ThrowsAsync<TException>()\` - Throw exceptions from async methods
- Variants with exception factories

## Suggested Documentation

Add to the \"Async Methods\" subsection under Method Setup:

\`\`\`markdown
**Async Exception Handling**

Throw exceptions from async methods:

\`\`\`csharp
sut.SetupMock.Method.DispenseAsync(It.Is(\"Invalid\"), It.IsAny<int>())
    .ThrowsAsync<InvalidOperationException>();

// Or with a specific exception
sut.SetupMock.Method.DispenseAsync(It.Is(\"Error\"), It.IsAny<int>())
    .ThrowsAsync(new InvalidChocolateException(\"Bad chocolate!\"));

// Or with a factory
sut.SetupMock.Method.DispenseAsync(It.IsAny<string>(), It.IsAny<int>())
    .ThrowsAsync((type, amount) => new Exception(\$\"Cannot dispense {amount} {type}\"));
\`\`\`
\`\`\`

## Priority

**Low** - Minor completion of async documentation.

## Related Files

- README.md
- Docs/index.md"

echo "Created Issue 12 ✓"
echo ""

echo "✅ All 12 GitHub issues created successfully!"
echo ""
echo "Issues created:"
echo "  1. Document Mock.Wrap feature (High Priority)"
echo "  2. Document delegate mocking feature (High Priority)"
echo "  3. Document advanced parameter matching (High Priority)"
echo "  4. Document protected member support (High Priority)"
echo "  5. Document advanced callback features (Medium Priority)"
echo "  6. Document advanced property and indexer features (Medium Priority)"
echo "  7. Document additional verification count methods (Medium Priority)"
echo "  8. Document extended HttpClient features (Medium Priority)"
echo "  9. Document interaction tracking features (Low Priority)"
echo " 10. Document advanced mock behavior options (Low Priority)"
echo " 11. Document special method overrides (Low Priority)"
echo " 12. Document async exception handling (Low Priority)"
echo ""
echo "View all issues at: https://github.com/$REPO/issues"
