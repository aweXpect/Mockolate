using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record AdditionalImplementationClass : Class
{
	public AdditionalImplementationClass(ITypeSymbol type,
		List<Property> exceptProperties,
		List<Event> exceptEvents,
		List<Method> exceptMethods)
		: base(type, exceptEvents: exceptEvents, exceptProperties: exceptProperties, exceptMethods: exceptMethods)
	{
	}
}
