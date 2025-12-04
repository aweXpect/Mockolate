using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Class
{
	public Class(ITypeSymbol type,
		List<Method>? alreadyDefinedMethods = null,
		List<Property>? alreadyDefinedProperties = null,
		List<Event>? alreadyDefinedEvents = null,
		List<Method>? exceptMethods = null,
		List<Property>? exceptProperties = null,
		List<Event>? exceptEvents = null)
	{
		Namespace = type.ContainingNamespace.ToString();
		DisplayString = type.ToDisplayString();
		ClassName = GetTypeName(type);
		ClassFullName = GetTypeFullName(type);

		INamedTypeSymbol? containingType = type.ContainingType;
		if (containingType is not null)
		{
			while (containingType is not null)
			{
				ClassName = containingType.Name + "." + ClassName;
				containingType = containingType.ContainingType;
			}
		}

		IsInterface = type.TypeKind == TypeKind.Interface;
		List<Method> methods = ToListExcept(type.GetMembers().OfType<IMethodSymbol>()
			// Exclude getter/setter methods
			.Where(x => x.AssociatedSymbol is null && !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Select(x => new Method(x, alreadyDefinedMethods))
			.Distinct(), exceptMethods, Method.ContainingTypeIndependentEqualityComparer);
		Methods = new EquatableArray<Method>(methods.ToArray());

		List<Property> properties = ToListExcept(type.GetMembers().OfType<IPropertySymbol>()
			.Where(x => !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Select(x => new Property(x, alreadyDefinedProperties))
			.Distinct(), exceptProperties, Property.ContainingTypeIndependentEqualityComparer);
		Properties = new EquatableArray<Property>(properties.ToArray());

		List<Event> events = ToListExcept(type.GetMembers().OfType<IEventSymbol>()
			.Where(x => !x.IsSealed)
			.Where(x => IsInterface || x.IsVirtual || x.IsAbstract)
			.Select(x => (x, (x.Type as INamedTypeSymbol)?.DelegateInvokeMethod))
			.Where(x => x.DelegateInvokeMethod is not null)
			.Select(x => new Event(x.x, x.DelegateInvokeMethod!, alreadyDefinedEvents))
			.Distinct(), exceptEvents, Event.ContainingTypeIndependentEqualityComparer);
		Events = new EquatableArray<Event>(events.ToArray());

		exceptProperties ??= type.GetMembers().OfType<IPropertySymbol>()
			.Where(x => x.IsSealed)
			.Select(x => new Property(x, null))
			.Distinct()
			.ToList();
		exceptMethods ??= type.GetMembers().OfType<IMethodSymbol>()
			.Where(x => x.IsSealed)
			.Select(x => new Method(x, null))
			.Distinct()
			.ToList();
		exceptEvents ??= type.GetMembers().OfType<IEventSymbol>()
			.Where(x => x.IsSealed)
			.Select(x => (x, (x.Type as INamedTypeSymbol)?.DelegateInvokeMethod))
			.Where(x => x.DelegateInvokeMethod is not null)
			.Select(x => new Event(x.x, x.DelegateInvokeMethod!, null))
			.Distinct()
			.ToList();

		InheritedTypes = new EquatableArray<Class>(
			GetInheritedTypes(type).Select(t
					=> new Class(t, methods, properties, events, exceptMethods, exceptProperties, exceptEvents))
				.ToArray());
	}

	public EquatableArray<Method> Methods { get; }
	public EquatableArray<Class> InheritedTypes { get; }
	public EquatableArray<Property> Properties { get; }

	public EquatableArray<Event> Events { get; }

	public bool IsInterface { get; }
	public string? Namespace { get; }
	public string DisplayString { get; }
	public string ClassName { get; }
	public string ClassFullName { get; }

	private string GetTypeName(ITypeSymbol type)
	{
		if (type is INamedTypeSymbol namedType)
		{
			if (namedType.IsGenericType)
			{
				return namedType.Name + "<" + string.Join(",",
					namedType.TypeArguments.Select(GetTypeName)) + ">";
			}

			if (TryExtractSpecialName(namedType, out string? name))
			{
				return name;
			}
		}

		return type.Name;
	}

	private string GetTypeFullName(ITypeSymbol type)
	{
		string GetPrefix(ITypeSymbol s)
		{
			string p = "";
			INamedTypeSymbol? containingType = s.ContainingType;
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
					namedType.TypeArguments.Select(t
						=> t.TypeKind == TypeKind.TypeParameter ? t.Name : GetTypeFullName(t))) + ">";
			}

			if (TryExtractSpecialName(namedType, out string? name))
			{
				return name;
			}
		}

		return GetPrefix(type) + type.Name;
	}

	private static bool TryExtractSpecialName(INamedTypeSymbol namedType, [NotNullWhen(true)] out string? specialName)
	{
		(specialName, bool hasSpecialType) = namedType.SpecialType switch
		{
			SpecialType.System_Void => ("void", true),
			SpecialType.System_Object => ("object", true),
			SpecialType.System_Boolean => ("bool", true),
			SpecialType.System_String => ("string", true),
			SpecialType.System_Char => ("char", true),
			SpecialType.System_Byte => ("byte", true),
			SpecialType.System_SByte => ("sbyte", true),
			SpecialType.System_Int16 => ("short", true),
			SpecialType.System_UInt16 => ("ushort", true),
			SpecialType.System_Int32 => ("int", true),
			SpecialType.System_UInt32 => ("uint", true),
			SpecialType.System_Int64 => ("long", true),
			SpecialType.System_UInt64 => ("ulong", true),
			SpecialType.System_Single => ("float", true),
			SpecialType.System_Double => ("double", true),
			SpecialType.System_Decimal => ("decimal", true),
			_ => (null, false),
		};
		return hasSpecialType;
	}

	private static List<T> ToListExcept<T>(IEnumerable<T> source, IEnumerable<T>? except, IEqualityComparer<T> comparer)
	{
		if (except is null)
		{
			return source.ToList();
		}

		return source.Except(except, comparer).ToList();
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
