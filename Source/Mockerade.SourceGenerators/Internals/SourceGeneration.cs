using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public const string Mock =
		"""
		using System;

		namespace Mockerade;

		#nullable enable
		public static partial class Mock
		{
			public static Mock<T> For<T>()
			{
				var generator = new MockGenerator();
				return generator.Get<T>(MockBehavior.Default)
					?? throw new NotSupportedException("Could not generate Mock<T>");
			}
			
			public static Mock<T> For<T>(MockBehavior mockBehavior)
			{
				var generator = new MockGenerator();
				return generator.Get<T>(mockBehavior)
					?? throw new NotSupportedException("Could not generate Mock<T>");
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
