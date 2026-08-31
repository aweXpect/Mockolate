using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal sealed class MockClass : Class, IEquatable<MockClass>
{
	private readonly int _mockSurfaceHash;

	public MockClass(ITypeSymbol[] types, IAssemblySymbol sourceAssembly) : base(types[0], sourceAssembly)
	{
		AdditionalImplementations = new EquatableArray<Class>(
			types.Skip(1).Select(x => new Class(x, sourceAssembly)).ToArray());

		ImplementedInterfaces = new EquatableArray<string>(types[0].AllInterfaces
			.Select(x => x.ToDisplayString(Helpers.TypeDisplayFormat)).ToArray());

		HiddenBaseInterfaces = IsInterface
			? new EquatableArray<Class>(GetHiddenBaseInterfaces(types[0])
				.Select(x => new Class(x, sourceAssembly)).ToArray())
			: new EquatableArray<Class>([]);

		if (!IsInterface && types[0] is INamedTypeSymbol namedTypeSymbol)
		{
			Constructors =
				new EquatableArray<Method>(namedTypeSymbol.Constructors
					.Where(x => x.DeclaredAccessibility == Accessibility.Protected ||
					            x.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
					            x.DeclaredAccessibility == Accessibility.Public)
					// Parameter types are named verbatim in `MockExtensionsForXXX`, which does not derive
					// from the mocked type: a type reachable only through inheritance would cause CS0122
					// there and CS0051 on the generated `public` constructor.
					.Where(x => x.Parameters.All(p => Helpers.IsAccessibleFrom(p.Type, sourceAssembly)))
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
	///     The full names of all interfaces the mocked type implements. An entry in
	///     <see cref="AdditionalImplementations" /> that is listed here is not really additional: the
	///     mocked type already implements it, so its members are rebased onto the class members that
	///     implement them (see <see cref="Class.RebaseOnto" />).
	/// </summary>
	public EquatableArray<string> ImplementedInterfaces { get; }

	/// <summary>
	///     Base interfaces whose members are hidden (via <see langword="new" />) by the mocked
	///     interface. Their setup/verify surfaces are generated and implemented so the hidden slots are
	///     reachable through <c>.Mock.As&lt;TBase&gt;()</c>. Distinct from
	///     <see cref="AdditionalImplementations" /> (the user's explicit <c>Implementing&lt;T&gt;()</c>).
	/// </summary>
	public EquatableArray<Class> HiddenBaseInterfaces { get; }

	/// <summary>
	///     MockClass equality is keyed on <see cref="Class.ClassFullName" /> plus a content-derived
	///     hash that folds the base surface together with the mock-only fields
	///     (<see cref="AdditionalImplementations" />, <see cref="HiddenBaseInterfaces" />,
	///     <see cref="Constructors" />, <see cref="Delegate" />). Two mocks of the same root with
	///     different additional interfaces, different constructor surfaces, or different delegate
	///     signatures must hash apart so Roslyn's incremental cache invalidates when any of those change.
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

		foreach (Class hiddenBaseInterface in HiddenBaseInterfaces)
		{
			yield return hiddenBaseInterface;
		}
	}

	public override bool Equals(Class? other) => other is MockClass mc && Equals(mc);

	public override bool Equals(object? obj) => Equals(obj as MockClass);

	public override int GetHashCode() => _mockSurfaceHash;

	private int ComputeMockSurfaceHash()
	{
		int hash = base.GetHashCode();
		hash = unchecked((hash * 17) + AdditionalImplementations.GetHashCode());
		hash = unchecked((hash * 17) + ImplementedInterfaces.GetHashCode());
		hash = unchecked((hash * 17) + HiddenBaseInterfaces.GetHashCode());
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

	/// <summary>
	///     Base interfaces of <paramref name="type" /> that declare a member which a more-derived
	///     interface in the hierarchy hides (a <see langword="new" /> member with a matching signature).
	///     The hidden base member is a separate interface slot, so its setup/verify surface must be
	///     generated explicitly. Ordinary (non-hidden) inheritance returns nothing.
	/// </summary>
	private static IEnumerable<INamedTypeSymbol> GetHiddenBaseInterfaces(ITypeSymbol type)
	{
		ImmutableArray<INamedTypeSymbol> allInterfaces = type.AllInterfaces;
		foreach (INamedTypeSymbol baseInterface in allInterfaces)
		{
			if (baseInterface.GetMembers().Any(member => member.IsStatic))
			{
				continue;
			}

			bool hasHiddenMember = false;
			foreach (ISymbol baseMember in baseInterface.GetMembers())
			{
				if (!IsHidableMember(baseMember))
				{
					continue;
				}

				if (HidesMember(type, baseMember) ||
				    allInterfaces.Any(intermediate =>
					    !SymbolEqualityComparer.Default.Equals(intermediate, baseInterface) &&
					    intermediate.AllInterfaces.Contains(baseInterface, SymbolEqualityComparer.Default) &&
					    HidesMember(intermediate, baseMember)))
				{
					hasHiddenMember = true;
					break;
				}
			}

			if (hasHiddenMember)
			{
				yield return baseInterface;
			}
		}
	}

	private static bool HidesMember(ITypeSymbol hidingType, ISymbol baseMember)
		=> hidingType.GetMembers(baseMember.Name)
			.Any(candidate => !SymbolEqualityComparer.Default.Equals(candidate.ContainingType, baseMember.ContainingType) &&
			                  SignatureMatches(candidate, baseMember));

	private static bool IsHidableMember(ISymbol member)
		=> member switch
		{
			IMethodSymbol { MethodKind: MethodKind.Ordinary, } => true,
			IPropertySymbol => true,
			IEventSymbol => true,
			_ => false,
		};

	private static bool SignatureMatches(ISymbol a, ISymbol b)
		=> a.Kind == b.Kind && (a, b) switch
		{
			(IMethodSymbol ma, IMethodSymbol mb) => ma.TypeParameters.Length == mb.TypeParameters.Length &&
			                                        ParametersMatch(ma.Parameters, mb.Parameters),
			(IPropertySymbol pa, IPropertySymbol pb) => ParametersMatch(pa.Parameters, pb.Parameters),
			(IEventSymbol, IEventSymbol) => true,
			_ => false,
		};

	private static bool ParametersMatch(ImmutableArray<IParameterSymbol> a, ImmutableArray<IParameterSymbol> b)
	{
		if (a.Length != b.Length)
		{
			return false;
		}

		for (int i = 0; i < a.Length; i++)
		{
			if (a[i].RefKind != b[i].RefKind ||
			    !SymbolEqualityComparer.Default.Equals(a[i].Type, b[i].Type))
			{
				return false;
			}
		}

		return true;
	}
}
