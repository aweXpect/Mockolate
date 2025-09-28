using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public const string Mock =
		"""
		using System;

		namespace Mockerade;

		#nullable enable
		/// <summary>
		///     Create new mocks by calling <see cref="Mock.For{T}" />.
		/// </summary>
		public static partial class Mock
		{
			/// <summary>
			///     Create a new mock for <typeparamref name="T" /> with the default <see cref="MockBehavior" />.
			/// </summary>
			/// <typeparam name="T">Type to mock, which can be an interface or a class.</typeparam>
			/// <remarks>
			///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
			/// </remarks>
			public static Mock<T> For<T>()
			{
				var generator = new MockGenerator();
				return generator.Get<T>(MockBehavior.Default)
					?? throw new NotSupportedException("Could not generate Mock<T>. Did the source generator run correctly?");
			}
			
			/// <summary>
			///     Create a new mock for <typeparamref name="T" /> with the given <paramref name="mockBehavior" />.
			/// </summary>
			/// <typeparam name="T">Type to mock, which can be an interface or a class.</typeparam>
			/// <remarks>
			///     Any interface type can be used for mocking, but for classes, only abstract and virtual members can be mocked.
			///     <para />
			///     The behavior of the mock with regards to the setups and the actual calls is determined by the <see cref="MockBehavior" />.
			/// </remarks>
			public static Mock<T> For<T>(MockBehavior mockBehavior)
			{
				var generator = new MockGenerator();
				return generator.Get<T>(mockBehavior)
					?? throw new NotSupportedException("Could not generate Mock<T>. Did the source generator run correctly?");
			}
			
			private partial class MockGenerator
			{
				private object? _value;
				partial void Generate<T>(MockBehavior mockBehavior);
				public Mock<T>? Get<T>(MockBehavior mockBehavior)
				{
					Generate<T>(mockBehavior);
					return _value as Mock<T>;
				}
			}
		}
		#nullable disable
		""";

	internal static string ToVisibilityString(this Accessibility accessibility)
		=> accessibility switch
		{
			Accessibility.Private => "private",
			Accessibility.Protected => "protected",
			Accessibility.Internal => "internal",
			Accessibility.ProtectedOrInternal => "protected internal",
			Accessibility.Public => "public",
			Accessibility.ProtectedAndInternal => "protected internal",
			_ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null)
		};



	internal static string GetString(this RefKind refKind)
		=> refKind switch
		{
			RefKind.None => "",
			RefKind.In => "in ",
			RefKind.Out => "out ",
			RefKind.Ref => "ref ",
			RefKind.RefReadOnlyParameter => "ref readonly ",
			_ => ""
		};
}
