using System.Text;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	/// <summary>
	///     Creates the <c>MockGenerator</c> attribute.
	/// </summary>
	public static string MockGeneratorAttribute()
	{
		StringBuilder sb = InitializeBuilder();

		sb.AppendLine("""
		              namespace Mockolate;

		              /// <summary>
		              ///     Marks a method as a mock generator for its generic parameters.
		              /// </summary>
		              [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
		              internal class MockGeneratorAttribute : global::System.Attribute
		              {
		              }

		              /// <summary>
		              ///     Marks a method as implementing additional interfaces for a mock.
		              /// </summary>
		              [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
		              internal class MockGeneratorImplementingAttribute : global::System.Attribute
		              {
		              }
		              """);
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
