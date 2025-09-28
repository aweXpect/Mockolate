using Mockerade.SourceGenerators.Entities;

namespace Mockerade.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public static string RegisterMocks(ICollection<MockClass> mocks)
	{
		string result = """
		                using System;
		                """;
		foreach (string? @namespace in mocks.Select(x => x.Namespace).Distinct().OrderBy(x => x))
		{
			result += $$"""

			            using {{@namespace}};
			            """;
		}

		result += """

		          namespace Mockerade;

		          #nullable enable
		          public static partial class Mock
		          {
		          	private partial class MockGenerator
		          	{
		          		partial void Generate<T>(MockBehavior mockBehavior)
		          		{

		          """;
		int index = 0;
		foreach (MockClass mock in mocks)
		{
			if (index++ > 0)
			{
				result += "			else ";
			}
			else
			{
				result += "			";
			}

			result += $$"""
			            if (typeof(T) == typeof({{mock.ClassName}}))
			            			{
			            				_value = new For{{mock.ClassName}}.Mock(mockBehavior) as Mock<T>;
			            			}

			            """;
		}

		result += """
		              
		          		}
		          	}
		          }
		          #nullable disable
		          """;
		return result;
	}
}
