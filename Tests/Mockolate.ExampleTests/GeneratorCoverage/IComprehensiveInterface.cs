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
	string InitOnly { get; init; }

	string this[int i] { get; set; }
	string this[int a, int b, int c, int d, int e] { get; set; }

	static abstract int StaticAbstractValue { get; }

	event EventHandler PlainEvent;
	event EventHandler<MyEventArgs> TypedEvent;
	event ComprehensiveDelegate CustomEvent;
	static abstract int StaticAbstractMethod();

	void WithModifiers(ref int a, out string b, in long c, params int[] tail);

	void WithDefaults(int i = 5, MyEnum e = MyEnum.B, decimal d = 1.5m,
		float f = 0.25f, char c = 'x', string? s = null, MyStruct st = default);

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

	ref int GetByRef();
	ref readonly int GetByRefReadonly();

	T G1<T>() where T : class;
	T G2<T>() where T : struct;
	T G3<T>() where T : new();
	T G4<T>() where T : unmanaged;
	T G5<T>() where T : notnull;
	T G6<T>() where T : MyBase, IComparable<T>;

	T? G7<T>() where T : class?;

	// `allows ref struct` constraint: IReturnMethodSetup<T> rejects ref-struct T (CS9244).
	// Needs runtime-side `allows ref struct` on the setup interfaces or a generator carve-out.
	// Tracking: see follow-up prompt for Bug 4.
	// T G8<T>() where T : allows ref struct;

	int Five(int a, int b, int c, int d, int e);

	int Seventeen(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8,
		int a9, int a10, int a11, int a12, int a13, int a14, int a15, int a16, int a17);

	void SeventeenVoid(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8,
		int a9, int a10, int a11, int a12, int a13, int a14, int a15, int a16, int a17);
}
#endif
