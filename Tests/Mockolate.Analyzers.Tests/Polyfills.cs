// Local polyfills that win type resolution over IVT-imported polyfills from
// Mockolate.Analyzers (netstandard2.0). Without these, the imported internal
// nullable-attribute types collide with System.Runtime's public types as
// CS0433. With local definitions present the conflict downgrades to CS0436,
// which is suppressed via NoWarn in the project file.
namespace System.Diagnostics.CodeAnalysis
{
	internal sealed class AllowNullAttribute : Attribute;
	internal sealed class DisallowNullAttribute : Attribute;
	internal sealed class DoesNotReturnAttribute : Attribute;
	internal sealed class DoesNotReturnIfAttribute(bool parameterValue) : Attribute
	{
		public bool ParameterValue { get; } = parameterValue;
	}
	internal sealed class MaybeNullAttribute : Attribute;
	internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
	{
		public bool ReturnValue { get; } = returnValue;
	}
	internal sealed class MemberNotNullAttribute(params string[] members) : Attribute
	{
		public string[] Members { get; } = members;
	}
	internal sealed class MemberNotNullWhenAttribute(bool returnValue, params string[] members) : Attribute
	{
		public bool ReturnValue { get; } = returnValue;
		public string[] Members { get; } = members;
	}
	internal sealed class NotNullAttribute : Attribute;
	internal sealed class NotNullIfNotNullAttribute(string parameterName) : Attribute
	{
		public string ParameterName { get; } = parameterName;
	}
	internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
	{
		public bool ReturnValue { get; } = returnValue;
	}
}
