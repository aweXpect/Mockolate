using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record MockClass : Class
{
	public MockClass(ITypeSymbol[] types) : base(types[0])
	{
		AdditionalImplementations = new EquatableArray<Class>(
			types.Skip(1).Select(x => new Class(x)).ToArray());

		if (!IsInterface && types[0] is INamedTypeSymbol namedTypeSymbol)
		{
			Constructors =
				new EquatableArray<Method>(namedTypeSymbol.Constructors
				.Where(x => x.DeclaredAccessibility == Accessibility.Protected ||
				            x.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
				            x.DeclaredAccessibility == Accessibility.Public)
				.Select(x => new Method(x, null)).ToArray());
			if (namedTypeSymbol.DelegateInvokeMethod is not null)
			{
				Delegate = new Method(namedTypeSymbol.DelegateInvokeMethod, null);
			}
		}
	}

	public Method? Delegate { get; }

	public EquatableArray<Method>? Constructors { get; }

	public EquatableArray<Class> AdditionalImplementations { get; }

	public IEnumerable<Class> DistinctAdditionalImplementations()
		=> AdditionalImplementations.Distinct().Where(x => x.GetFullName() != GetFullName());

	internal IEnumerable<Class> GetAllClasses()
	{
		yield return this;
		foreach (Class implementation in DistinctAdditionalImplementations())
		{
			yield return implementation;
		}
	}
}
