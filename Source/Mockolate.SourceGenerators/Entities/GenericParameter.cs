using System.Text;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct GenericParameter
{
	public GenericParameter(ITypeParameterSymbol typeSymbol)
	{
		Name = typeSymbol.Name;
		IsClass = typeSymbol.HasReferenceTypeConstraint;
		IsStruct = typeSymbol.HasValueTypeConstraint;
		IsNotNull = typeSymbol.HasNotNullConstraint;
		IsUnmanaged = typeSymbol.HasUnmanagedTypeConstraint;
		HasConstructor = typeSymbol.HasConstructorConstraint;
		AllowsRefStruct = typeSymbol.AllowsRefLikeType;
		NullableAnnotation = typeSymbol.ReferenceTypeConstraintNullableAnnotation;

		ConstraintTypes = new EquatableArray<Type>(typeSymbol.ConstraintTypes
			.Select(x => new Type(x)).ToArray());
	}
	
	public EquatableArray<Type> ConstraintTypes { get; }

	public string Name { get; }
	public bool IsStruct { get; }
	public bool IsNotNull { get; }
	public bool IsUnmanaged { get; }
	public bool HasConstructor { get; }
	public bool AllowsRefStruct { get; }
	public NullableAnnotation NullableAnnotation { get; }
	public bool IsClass { get; }

	public void AppendWhereConstraint(StringBuilder sb, string[] namespaces)
	{
		int count = 0;
		sb.Append("where ").Append(Name).Append(" : ");
		foreach (var constraintType in ConstraintTypes)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append(constraintType.GetMinimizedString(namespaces));
			if (NullableAnnotation == NullableAnnotation.Annotated)
			{
				sb.Append('?');
			}
		}
		if (IsStruct)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append("struct");
		}
		if (IsClass)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append("class");
			if (NullableAnnotation == NullableAnnotation.Annotated)
			{
				sb.Append('?');
			}
		}
		if (IsNotNull)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append("notnull");
		}
		if (IsUnmanaged)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append("unmanaged");
		}
		if (HasConstructor)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append("new()");
		}
		if (AllowsRefStruct)
		{
			if (count++ > 0)
			{
				sb.Append(", ");
			}
			sb.Append("allows ref struct");
		}
	}
}
