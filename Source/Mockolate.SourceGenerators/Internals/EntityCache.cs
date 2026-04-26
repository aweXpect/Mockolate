using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Entities;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators.Internals;

/// <summary>
///     Per-compilation cache for symbol-keyed entity records. Roslyn returns canonical
///     <see cref="ITypeSymbol" /> instances for the same type within one compilation, so
///     deduplicating on symbol identity is sound and lets a generator build that touches the
///     same shared types (<c>string</c>, <c>int</c>, <see cref="System.Threading.Tasks.Task" />,
///     etc.) hundreds of times allocate the entity record only once.
/// </summary>
/// <remarks>
///     Caches MUST NOT cross compilation boundaries — the symbols won't survive. The
///     <see cref="ConditionalWeakTable{TKey,TValue}" /> guarantees the cache is collected when
///     the <see cref="Compilation" /> is. Cache lookup is exposed through
///     <see cref="EnterScope" /> so the existing entity constructors can stay parameter-stable.
/// </remarks>
internal sealed class EntityCache
{
	private static readonly ConditionalWeakTable<Compilation, EntityCache> _caches = new();

	private readonly ConcurrentDictionary<IParameterSymbol, MethodParameter> _parameters = new(SymbolEqualityComparer.IncludeNullability);

	// IncludeNullability is essential — Default would treat `string` and `string?` as the same
	// key, but the resulting Type/MethodParameter records carry different `CanBeNullable` /
	// `IsNullableAnnotated` flags and produce different generated code.
	private readonly ConcurrentDictionary<ITypeSymbol, Type> _types = new(SymbolEqualityComparer.IncludeNullability);

	[field: ThreadStatic] public static EntityCache? Current { get; private set; }

	public static EntityCache GetOrCreate(Compilation compilation)
		=> _caches.GetValue(compilation, static _ => new EntityCache());

	public static Scope EnterScope(EntityCache cache)
	{
		EntityCache? previous = Current;
		Current = cache;
		return new Scope(previous);
	}

	public Type GetOrAddType(ITypeSymbol symbol, Func<ITypeSymbol, Type> factory)
		=> _types.GetOrAdd(symbol, factory);

	public MethodParameter GetOrAddParameter(IParameterSymbol symbol, Func<IParameterSymbol, MethodParameter> factory)
		=> _parameters.GetOrAdd(symbol, factory);

	public readonly struct Scope : IDisposable
	{
		private static void ExitScope(EntityCache? previous) => Current = previous;
		
		private readonly EntityCache? _previous;

		internal Scope(EntityCache? previous)
		{
			_previous = previous;
		}

		public void Dispose() => ExitScope(_previous);
	}
}
