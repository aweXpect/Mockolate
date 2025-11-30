using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Method
{
	public Method(IMethodSymbol methodSymbol, List<Method>? alreadyDefinedMethods)
	{
		Accessibility = methodSymbol.DeclaredAccessibility;
		UseOverride = methodSymbol.IsVirtual || methodSymbol.IsAbstract;
		IsAbstract = methodSymbol.IsAbstract;
		ReturnType = methodSymbol.ReturnsVoid ? Type.Void : new Type(methodSymbol.ReturnType);
		Name = methodSymbol.Name;
		ContainingType = methodSymbol.ContainingType.ToDisplayString();
		Parameters = new EquatableArray<MethodParameter>(
			methodSymbol.Parameters.Select(x => new MethodParameter(x)).ToArray());

		if (methodSymbol.IsGenericMethod)
		{
			GenericParameters = new EquatableArray<GenericParameter>(methodSymbol.TypeArguments
				.Select(x => new GenericParameter((ITypeParameterSymbol)x)).ToArray());
			Name += $"<{string.Join(", ", GenericParameters.Value.Select(x => x.Name))}>";
		}

		Attributes = methodSymbol.GetAttributes().ToAttributeArray();

		if (alreadyDefinedMethods is not null)
		{
			if (alreadyDefinedMethods.Any(m =>
				    m.Name == Name &&
				    m.Parameters.Count == Parameters.Count &&
				    m.Parameters.SequenceEqual(Parameters)))
			{
				ExplicitImplementation = ContainingType;
			}

			alreadyDefinedMethods.Add(this);
		}
	}

	public static IEqualityComparer<Method> EqualityComparer { get; } = new MethodEqualityComparer();

	public static IEqualityComparer<Method> ContainingTypeIndependentEqualityComparer { get; } =
		new ContainingTypeIndependentMethodEqualityComparer();

	public EquatableArray<GenericParameter>? GenericParameters { get; }

	public bool UseOverride { get; }
	public bool IsAbstract { get; }
	public Accessibility Accessibility { get; }
	public Type ReturnType { get; }
	public string Name { get; }
	public string ContainingType { get; }
	public EquatableArray<MethodParameter> Parameters { get; }
	public string? ExplicitImplementation { get; }
	public EquatableArray<Attribute>? Attributes { get; }

	internal string GetUniqueNameString()
	{
		if (GenericParameters != null)
		{
			string name = Name.Substring(0, Name.IndexOf('<'));
			string parameters = string.Join(", ",
				GenericParameters.Value.Select(genericParameter => $"{{typeof({genericParameter.Name})}}"));
			return $"$\"{ContainingType}.{name}<{parameters}>\"";
		}

		return $"\"{ContainingType}.{Name}\"";
	}

	public bool IsToString()
		=> Name == "ToString" && Parameters.Count == 0;

	public bool IsGetHashCode()
		=> Name == "GetHashCode" && Parameters.Count == 0;

	public bool IsEquals()
	{
		return Name == "Equals" && Parameters.Count == 1 && IsObjectOrNullableObject(Parameters.Single());

		static bool IsObjectOrNullableObject(MethodParameter parameter)
		{
			return parameter.Type.Namespace == "System" &&
			       (parameter.Type.Fullname == "object" || parameter.Type.Fullname == "object?");
		}
	}

	private sealed class MethodEqualityComparer : IEqualityComparer<Method>
	{
		public bool Equals(Method? x, Method? y)
			=> (x is null && y is null) ||
			   (x is not null && y is not null &&
			    x.Name.Equals(y.Name) && x.ContainingType.Equals(y.ContainingType) &&
			    x.Parameters.Count == y.Parameters.Count &&
			    x.Parameters.SequenceEqual(y.Parameters));

		public int GetHashCode(Method obj) => obj.Name.GetHashCode();
	}

	private sealed class ContainingTypeIndependentMethodEqualityComparer : IEqualityComparer<Method>
	{
		public bool Equals(Method? x, Method? y)
			=> (x is null && y is null) ||
			   (x is not null && y is not null &&
			    x.Name.Equals(y.Name) &&
			    x.Parameters.Count == y.Parameters.Count &&
			    x.Parameters.SequenceEqual(y.Parameters));

		public int GetHashCode(Method obj) => obj.Name.GetHashCode();
	}
}
