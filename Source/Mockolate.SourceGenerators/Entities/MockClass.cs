using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal sealed class MockClass : Class, IEquatable<MockClass>
{
	private readonly int _mockSurfaceHash;

	public MockClass(ITypeSymbol[] types, IAssemblySymbol sourceAssembly) : base(types[0], sourceAssembly)
	{
		AdditionalImplementations = new EquatableArray<Class>(
			types.Skip(1).Select(x => new Class(x, sourceAssembly)).ToArray());

		if (!IsInterface && types[0] is INamedTypeSymbol namedTypeSymbol)
		{
			Constructors =
				new EquatableArray<Method>(namedTypeSymbol.Constructors
					.Where(x => x.DeclaredAccessibility == Accessibility.Protected ||
					            x.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
					            x.DeclaredAccessibility == Accessibility.Public)
					.Select(x => new Method(x, null, sourceAssembly)).ToArray());
			if (namedTypeSymbol.DelegateInvokeMethod is not null)
			{
				Delegate = new Method(namedTypeSymbol.DelegateInvokeMethod, null, sourceAssembly);
			}
		}

		_mockSurfaceHash = ComputeMockSurfaceHash();
	}

	public Method? Delegate { get; }

	public EquatableArray<Method>? Constructors { get; }

	public EquatableArray<Class> AdditionalImplementations { get; }

	/// <summary>
	///     MockClass equality is keyed on <see cref="Class.ClassFullName" /> plus a content-derived
	///     hash that folds the base surface together with the mock-only fields
	///     (<see cref="AdditionalImplementations" />, <see cref="Constructors" />,
	///     <see cref="Delegate" />). Two mocks of the same root with different additional
	///     interfaces, different constructor surfaces, or different delegate signatures must hash
	///     apart so Roslyn's incremental cache invalidates when any of those change.
	/// </summary>
	public bool Equals(MockClass? other)
		=> ReferenceEquals(this, other) ||
		   (other is not null &&
		    _mockSurfaceHash == other._mockSurfaceHash &&
		    ClassFullName == other.ClassFullName);

	public IEnumerable<Class> AllImplementations()
	{
		yield return this;
		foreach (Class additionalImplementation in AdditionalImplementations)
		{
			yield return additionalImplementation;
		}
	}

	public override bool Equals(Class? other) => other is MockClass mc && Equals(mc);

	public override bool Equals(object? obj) => Equals(obj as MockClass);

	public override int GetHashCode() => _mockSurfaceHash;

	private int ComputeMockSurfaceHash()
	{
		int hash = base.GetHashCode();
		hash = unchecked((hash * 17) + AdditionalImplementations.GetHashCode());
		if (Constructors is { } constructors)
		{
			hash = unchecked((hash * 17) + constructors.GetHashCode());
		}

		if (Delegate is { } @delegate)
		{
			hash = unchecked((hash * 17) + @delegate.GetHashCode());
		}

		return hash;
	}
}
