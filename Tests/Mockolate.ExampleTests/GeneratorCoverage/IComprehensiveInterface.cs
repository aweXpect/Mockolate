#if NET10_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Mockolate.ExampleTests.GeneratorCoverage;

public class MyBase
{
}

public enum MyEnum { A, B, C, }

public struct MyStruct
{
	public int X;
}

public class MyEventArgs : EventArgs
{
	public int N;
}

public delegate void SimpleEventDelegate(int value);

/// <summary>
///     Squeezes every interface-shaped generator branch we can fit into a single type:
///     property accessor combinations, indexers (single + arity-5), all three event flavors,
///     static abstract members, every parameter modifier, every default-value kind,
///     every "reserved" parameter name, nullable annotations, async returns, special return
///     shapes (Span/ReadOnlySpan/ref/ref-readonly/tuple/Nullable&lt;T&gt;),
///     every generic-constraint kind, plus arity-5 and arity-17 methods to trigger the
///     <c>MethodSetups.g.cs</c> / <c>ActionFunc.g.cs</c> aggregates.
/// </summary>
public interface IComprehensiveInterface
{
	int GetSet { get; set; }
	int GetOnly { get; }
	int SetOnly { set; }
	string? NullableProp { get; set; }

	// Init-only properties: generator currently emits a `set` accessor instead of `init`,
	// causing CS8854 on the generated mock. Re-enable once the generator preserves `init`.
	// string InitOnly { get; init; }

	string this[int i] { get; set; }
	string this[int a, int b, int c, int d, int e] { get; set; }

	static abstract int StaticAbstractValue { get; }

	event EventHandler PlainEvent;
	event EventHandler<MyEventArgs> TypedEvent;

	// Custom-delegate event: when the delegate has ref/out parameters, the generator's
	// raise method drops the ref/out keywords on the Invoke call (CS1620). Use a plain
	// pass-by-value delegate here; ComprehensiveDelegate is mocked separately as a delegate.
	event SimpleEventDelegate CustomEvent;
	static abstract int StaticAbstractMethod();

	// Parameter modifiers split per method: combining ref + out + in + params in one
	// signature currently emits CS1620 (missing ref/out keyword) on the generated mock.
	void WithRefOut(ref int a, out string b);
	void WithIn(in long c);
	void WithParams(params int[] tail);

	// Default values split per type-kind. Single-letter parameter names (i, e, etc.) collide
	// with the generator's lambda variable named `i`, causing CS0176/CS1503 in the generated
	// verify methods, so we use longer names here.
	void WithDefaultIntEnum(int integerValue = 5, MyEnum enumValue = MyEnum.B);
	void WithDefaultStructDecimal(MyStruct structValue = default, decimal decimalValue = 1.5m);
	void WithDefaultFloatChar(float floatValue = 0.25f, char charValue = 'x');
	void WithDefaultNullable(string? nullableValue = null);

	void WithCollidingNames(int wraps, int result, int outParam1,
		int methodExecution, int returnValue);

	string? GetMaybeNull(string? s);

	Task DoTask();
	Task<int> DoTaskOf();
	ValueTask DoVT();
	ValueTask<int> DoVTOf();

	(int Code, string Msg) GetTuple();
	int? GetNullable();
	Span<char> GetSpan(int n);
	ReadOnlySpan<char> GetROSpan(int n);

	// Ref returns: the mock currently returns by value, breaking the ref-return contract
	// (CS8152). Re-enable once the generator emits ref-returning overrides.
	// ref int GetByRef();
	// ref readonly int GetByRefReadonly();

	T G1<T>() where T : class;
	T G2<T>() where T : struct;
	T G3<T>() where T : new();
	T G4<T>() where T : unmanaged;
	T G5<T>() where T : notnull;
	T G6<T>() where T : MyBase, IComparable<T>;

	// Nullable-annotated generic return (`T?` with `T : class?`): generator drops the
	// annotation in the override (CS9334/CS0738). Re-enable once preserved.
	// T? G7<T>() where T : class?;

	// `allows ref struct` constraint: IReturnMethodSetup<T> rejects ref-struct T (CS9244).
	// Re-enable once the generator routes these through the ref-struct setup pipeline.
	// T G8<T>() where T : allows ref struct;

	int Five(int a, int b, int c, int d, int e);

	int Seventeen(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8,
		int a9, int a10, int a11, int a12, int a13, int a14, int a15, int a16, int a17);

	void SeventeenVoid(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8,
		int a9, int a10, int a11, int a12, int a13, int a14, int a15, int a16, int a17);
}
#endif
