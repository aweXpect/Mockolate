using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal sealed class MockClass : Class, IEquatable<MockClass>
{
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
	}

	public Method? Delegate { get; }

	public EquatableArray<Method>? Constructors { get; }

	public EquatableArray<Class> AdditionalImplementations { get; }

	public IEnumerable<Class> AllImplementations()
	{
		yield return this;
		foreach (Class additionalImplementation in AdditionalImplementations)
		{
			yield return additionalImplementation;
		}
	}

	// MockClass identity is the root ClassFullName plus the ClassFullNames of any additional
	// implementations: two mocks of the same root with different additional interfaces produce
	// different generated files, so the per-mock incremental cache must distinguish them.
	public bool Equals(MockClass? other)
		=> ReferenceEquals(this, other) ||
		   (other is not null &&
		    ClassFullName == other.ClassFullName &&
		    AdditionalImplementationsEqual(AdditionalImplementations, other.AdditionalImplementations));

	public override bool Equals(Class? other) => other is MockClass mc && Equals(mc);

	public override bool Equals(object? obj) => Equals(obj as MockClass);

	public override int GetHashCode()
	{
		int hash = ClassFullName.GetHashCode();
		Class[]? additional = AdditionalImplementations.AsArray();
		if (additional is null)
		{
			return hash;
		}

		int multiplier = 17;
		foreach (Class c in additional)
		{
			hash = unchecked(hash + c.ClassFullName.GetHashCode() * multiplier);
			multiplier *= 17;
		}

		return hash;
	}

	private static bool AdditionalImplementationsEqual(EquatableArray<Class> left, EquatableArray<Class> right)
	{
		if (left.Count != right.Count)
		{
			return false;
		}

		Class[]? leftArray = left.AsArray();
		Class[]? rightArray = right.AsArray();
		if (leftArray is null || rightArray is null)
		{
			return leftArray is null && rightArray is null;
		}

		for (int i = 0; i < leftArray.Length; i++)
		{
			if (leftArray[i].ClassFullName != rightArray[i].ClassFullName)
			{
				return false;
			}
		}

		return true;
	}
}
