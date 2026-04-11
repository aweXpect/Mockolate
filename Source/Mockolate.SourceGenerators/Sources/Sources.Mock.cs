using System.Text;

namespace Mockolate.SourceGenerators.Sources;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
internal static partial class Sources
{
	/// <summary>
	///     Creates the static <c>Mock</c> class with the <c>CreateMock</c> extension methods to create new mocks.
	/// </summary>
	public static string MockClass()
	{
		StringBuilder sb = InitializeBuilder();

		sb.AppendLine("""
		              namespace Mockolate;

		              #nullable enable
		              /// <summary>
		              ///     Create new mocks by calling the static <c>T.CreateMock()</c> method on your type <c>T</c>.
		              /// </summary>
		              /// <remarks>
		              ///     You can also provide a <see cref="global::Mockolate.MockBehavior"/> parameter to customize how the mock should behave in certain scenarios.<br />
		              ///     If your type is a class without a default constructor, you can provide constructor parameters by passing an <c>object?[]?</c> to the corresponding <c>CreateMock(...)</c> overload.
		              /// </remarks>
		              """);
#if !DEBUG
		sb.Append("[global::System.Diagnostics.DebuggerNonUserCode]").AppendLine();
#endif
		sb.AppendLine("""
		              [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		              internal static partial class Mock
		              {
		              	/// <summary>
		              	///     This interface should never be used. If it is, this is an indication that the Mockolate source generator did not run correctly or that the used type is not mockable.
		              	/// </summary>
		              	/// <remarks>
		              	///     The source generator creates overloads with correct return values.
		              	/// </remarks>
		              	internal interface IMockGenerationDidNotRun {}

		              	/// <summary>
		              	///     Create a new mock of <typeparamref name="T" /> with the default <see cref="global::Mockolate.MockBehavior" />.
		              	/// </summary>
		              	/// <typeparam name="T">Type to mock, which can be an interface or a class.</typeparam>
		              	/// <remarks>
		              	///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		              	/// </remarks>
		              	extension<T>(T _)
		              	{
		              		/// <summary>
		              		///     Create a new mock of <typeparamref name="T" /> with the given <paramref name="mockBehavior" />.
		              		/// </summary>
		              		/// <remarks>
		              		///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		              		/// </remarks>
		              		public static global::Mockolate.Mock.IMockGenerationDidNotRun CreateMock(global::Mockolate.MockBehavior? mockBehavior = null)
		              		{
		              			throw new global::Mockolate.Exceptions.MockException($"This method should not be called directly. Either '{typeof(T)}' is not mockable or the source generator did not run correctly.");
		              		}

		              		/// <summary>
		              		///     Create a new mock of <typeparamref name="T" /> using the <paramref name="constructorParameters" /> with the given <paramref name="mockBehavior" />.
		              		/// </summary>
		              		/// <remarks>
		              		///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		              		/// </remarks>
		              		public static global::Mockolate.Mock.IMockGenerationDidNotRun CreateMock(object?[]? constructorParameters, global::Mockolate.MockBehavior? mockBehavior = null)
		              		{
		              			throw new global::Mockolate.Exceptions.MockException($"This method should not be called directly. Either '{typeof(T)}' is not mockable or the source generator did not run correctly.");
		              		}
		              	}

		              	extension(global::Mockolate.Mock.IMockGenerationDidNotRun _)
		              	{
		              		/// <summary>
		              		///     Add an interface <typeparamref name="TInterface" /> that the mock also implements.
		              		/// </summary>
		              		public global::Mockolate.Mock.IMockGenerationDidNotRun Implementing<TInterface>() where TInterface : class
		              		{
		              			throw new global::Mockolate.Exceptions.MockException($"This method should not be called directly. Either '{typeof(TInterface)}' is not mockable or the source generator did not run correctly.");
		              		}
		              	}
		              }
		              #nullable disable
		              """);
		return sb.ToString();
	}
}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
