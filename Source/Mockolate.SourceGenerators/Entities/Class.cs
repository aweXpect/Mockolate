using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Class
{
	public Class(ITypeSymbol type,
		List<Method>? alreadyDefinedMethods = null,
		List<Property>? alreadyDefinedProperties = null,
		List<Event>? alreadyDefinedEvents = null)
	{
		Namespace = type.ContainingNamespace.ToString();
		ClassName = GetTypeName(type);
		ClassFullName = GetTypeFullName(type);

		var containingType = type.ContainingType;
		if (containingType is not null)
		{
			ContainingType = new Type(containingType);
			while (containingType is not null)
			{
				ClassName = containingType.Name + "." + ClassName;
				containingType = containingType.ContainingType;
			}
		}

		IsInterface = type.TypeKind == TypeKind.Interface;
		List<Method> methods = type.GetMembers().OfType<IMethodSymbol>()
			// Exclude getter/setter methods
			.Where(x => x.AssociatedSymbol is null && !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Select(x => new Method(x, alreadyDefinedMethods))
			.Distinct()
			.ToList();
		Methods = new EquatableArray<Method>(methods.ToArray());

		List<Property> properties =
			type.GetMembers().OfType<IPropertySymbol>()
				.Where(x => !x.IsSealed)
				.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
				.Select(x => new Property(x, alreadyDefinedProperties))
				.Distinct()
				.ToList();
		Properties = new EquatableArray<Property>(properties.ToArray());

		List<Event> events =
			type.GetMembers().OfType<IEventSymbol>()
				.Where(x => !x.IsSealed)
				.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
				.Select(x => (x, (x.Type as INamedTypeSymbol)?.DelegateInvokeMethod))
				.Where(x => x.DelegateInvokeMethod is not null)
				.Select(x => new Event(x.x, x.DelegateInvokeMethod!, alreadyDefinedEvents))
				.Distinct()
				.ToList();
		Events = new EquatableArray<Event>(events.ToArray());

		InheritedTypes = new EquatableArray<Class>(
			GetInheritedTypes(type).Select(t => new Class(t, methods, properties, events))
				.ToArray());
	}

	public Type? ContainingType { get; }

	public EquatableArray<Method> Methods { get; }
	public EquatableArray<Class> InheritedTypes { get; }
	public EquatableArray<Property> Properties { get; }

	public EquatableArray<Event> Events { get; }

	public bool IsInterface { get; }
	public string Namespace { get; }
	public string ClassName { get; }
	public string ClassFullName { get; }

	private string GetTypeName(ITypeSymbol type)
	{
		if (type is INamedTypeSymbol namedType)
		{
			if (namedType.IsGenericType)
			{
				return namedType.Name + "<" + string.Join(",",
					namedType.TypeArguments.Select(t => GetTypeName(t))) + ">";
			}

			return namedType.SpecialType switch
			{
				SpecialType.System_Int32 => "int",
				SpecialType.System_Int64 => "long",
				SpecialType.System_Int16 => "short",
				SpecialType.System_UInt32 => "uint",
				SpecialType.System_UInt64 => "ulong",
				SpecialType.System_UInt16 => "ushort",
				SpecialType.System_Boolean => "bool",
				_ => type.Name,
			};
		}

		return type.Name;
	}

	private string GetTypeFullName(ITypeSymbol type)
	{
		string GetPrefix(ITypeSymbol s)
		{
			string p = "";
			var containingType = s.ContainingType;
			while (containingType is not null)
			{
				p = $"{containingType.Name}.{p}";
				containingType = containingType.ContainingType;
			}
			return $"{s.ContainingNamespace}.{p}";
		}

		if (type is INamedTypeSymbol namedType)
		{
			if (namedType.IsGenericType)
			{
				return GetPrefix(namedType) + namedType.Name + "<" + string.Join(",",
					namedType.TypeArguments.Select(t => GetTypeFullName(t))) + ">";
			}

			return namedType.SpecialType switch
			{
				SpecialType.System_Int32 => "int",
				SpecialType.System_Int64 => "long",
				SpecialType.System_Int16 => "short",
				SpecialType.System_UInt32 => "uint",
				SpecialType.System_UInt64 => "ulong",
				SpecialType.System_UInt16 => "ushort",
				SpecialType.System_Boolean => "bool",
				_ => GetPrefix(namedType) + namedType.Name,
			};
		}

		return GetPrefix(type) + type.Name;
	}

	public static IEnumerable<ITypeSymbol> GetInheritedTypes(ITypeSymbol type)
	{
		ITypeSymbol? current = type;
		while (current != null)
		{
			if (!SymbolEqualityComparer.Default.Equals(current, type))
			{
				yield return current;
			}

			if (current.TypeKind == TypeKind.Interface)
			{
				foreach (INamedTypeSymbol? @interface in current.AllInterfaces)
				{
					yield return @interface;
				}
			}

			current = current.BaseType;
		}
	}

	public IEnumerable<Property> AllProperties()
		=> AllClasses().SelectMany(c => c.Properties).Distinct(Property.EqualityComparer);

	public IEnumerable<Method> AllMethods()
		=> AllClasses().SelectMany(c => c.Methods).Distinct(Method.EqualityComparer);

	public IEnumerable<Event> AllEvents()
		=> AllClasses().SelectMany(c => c.Events).Distinct(Event.EqualityComparer);

	public IEnumerable<Class> AllClasses()
	{
		yield return this;
		foreach (Class inherited in InheritedTypes)
		{
			yield return inherited;
		}
	}

	public string GetClassNameWithoutDots()
		=> ClassName
			.Replace(",", "")
			.Replace(".", "")
			.Replace("<", "")
			.Replace(">", "");
}
