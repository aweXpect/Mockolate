using System.Text;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	public static string MockBehaviorExtensions()
	{
		StringBuilder sb = InitializeBuilder([
			"System.Diagnostics",
			"Mockolate",
			"Mockolate.DefaultValues",
		]);

		sb.Append("""
		          namespace Mockolate;

		          #nullable enable annotations

		          /// <summary>
		          ///     Extensions for <see cref="MockBehavior" />.
		          /// </summary>
		          internal static class MockBehaviorExtensions
		          {
		          	private static MockBehavior _default = new MockBehavior(new DefaultValueGenerator());
		          	
		          	extension(MockBehavior)
		          	{
		          		/// <summary>
		          		///     The default mock behavior settings.
		          		/// </summary>
		          		public static MockBehavior Default => _default;
		          	}
		          }
		          #nullable disable
		          """);
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
