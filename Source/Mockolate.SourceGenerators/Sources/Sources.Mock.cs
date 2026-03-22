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
		              internal static partial class Mock
		              {
		              	/// <summary>
		              	///     This interface should never be used directly. With source generation the correct overloads are created.
		              	/// </summary>
		              	internal interface IMockGeneration {}

		              	/// <summary>
		              	///     Create a new mock for <typeparamref name="T" /> with the default <see cref="global::Mockolate.MockBehavior" />.
		              	/// </summary>
		              	/// <typeparam name="T">Type to mock, which can be an interface or a class.</typeparam>
		              	/// <remarks>
		              	///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		              	/// </remarks>
		              	extension<T>(T _)
		              	{
		              		/// <summary>
		              		///     Create a new mock for <typeparamref name="T" /> with the given <paramref name="mockBehavior" />.
		              		/// </summary>
		              		/// <remarks>
		              		///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		              		/// </remarks>
		              		[global::Mockolate.MockGenerator]
		              		public static global::Mockolate.Mock.IMockGeneration CreateMock(global::Mockolate.MockBehavior? mockBehavior = null)
		              		{
		              			throw new global::Mockolate.Exceptions.MockException($"This method should not be called directly. Either '{typeof(T)}' is not mockable or the source generator did not run correctly.");
		              		}

		              		/// <summary>
		              		///     Create a new mock for <typeparamref name="T" /> using the <paramref name="constructorParameters" /> with the given <paramref name="mockBehavior" />.
		              		/// </summary>
		              		/// <remarks>
		              		///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
		              		/// </remarks>
		              		[global::Mockolate.MockGenerator]
		              		public static global::Mockolate.Mock.IMockGeneration CreateMock(object?[]? constructorParameters, global::Mockolate.MockBehavior? mockBehavior = null)
		              		{
		              			throw new global::Mockolate.Exceptions.MockException($"This method should not be called directly. Either '{typeof(T)}' is not mockable or the source generator did not run correctly.");
		              		}
		              	}

		              	extension(global::Mockolate.Mock.IMockGeneration _)
		              	{
		              		/// <summary>
		              		///     Add an additional interface <typeparamref name="TInterface" /> that the mock also implements.
		              		/// </summary>
		              		[global::Mockolate.MockGeneratorImplementing]
		              		public global::Mockolate.Mock.IMockGeneration Implementing<TInterface>() where TInterface : class
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
