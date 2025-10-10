using System.Collections.Generic;
using Mockolate.Interactions;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Internals;

public sealed class TypeFormatterTests
{
	[Theory]
	[MemberData(nameof(ProvideTestData))]
	public async Task FormattedType_ShouldBeExpectedValue(PropertySetup setup, string expectedValue)
	{
		var result = setup.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	public static TheoryData<PropertySetup, string> ProvideTestData()
	{
		return new()
		{
			{ new PropertySetup<int>(), "int" },
			{ new PropertySetup<byte>(), "byte" },
			{ new PropertySetup<short>(), "short" },
			{ new PropertySetup<long>(), "long" },
			{ new PropertySetup<float>(), "float" },
			{ new PropertySetup<double>(), "double" },
			{ new PropertySetup<decimal>(), "decimal" },
			{ new PropertySetup<object>(), "object" },
			{ new PropertySetup<bool>(), "bool" },
			{ new PropertySetup<char>(), "char" },
			{ new PropertySetup<string>(), "string" },
			{ new PropertySetup<int[]>(), "int[]" },
			{ new PropertySetup<List<string>>(), "List<string>" },
			{ new PropertySetup<Dictionary<int, string>>(), "Dictionary<int, string>" },
		};
	}
}
