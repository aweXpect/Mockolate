using System.Collections.Generic;
using System.IO;
using Mockolate.Setup;

namespace Mockolate.Tests.Internals;

public sealed class TypeFormatterTests
{
	[Theory]
	[MemberData(nameof(ProvideTestData))]
	public async Task FormattedType_ShouldBeExpectedValue(string formattedType, string expectedValue)
		=> await That(formattedType).IsEqualTo(expectedValue);

	public static TheoryData<string, string> ProvideTestData() => new()
	{
		{
			new PropertySetup<int>().ToString(), "int"
		},
		{
			new PropertySetup<byte>().ToString(), "byte"
		},
		{
			new PropertySetup<short>().ToString(), "short"
		},
		{
			new PropertySetup<long>().ToString(), "long"
		},
		{
			new PropertySetup<float>().ToString(), "float"
		},
		{
			new PropertySetup<double>().ToString(), "double"
		},
		{
			new PropertySetup<decimal>().ToString(), "decimal"
		},
		{
			new PropertySetup<object>().ToString(), "object"
		},
		{
			new PropertySetup<bool>().ToString(), "bool"
		},
		{
			new PropertySetup<bool?>().ToString(), "bool?"
		},
		{
			new PropertySetup<char>().ToString(), "char"
		},
		{
			new PropertySetup<string>().ToString(), "string"
		},
		{
			new PropertySetup<int[]>().ToString(), "int[]"
		},
		{
			new PropertySetup<DateTime?>().ToString(), "DateTime?"
		},
		{
			new PropertySetup<StreamReader>().ToString(), "StreamReader"
		},
		{
			new PropertySetup<List<string>>().ToString(), "List<string>"
		},
		{
			new PropertySetup<List<int?[]>>().ToString(), "List<int?[]>"
		},
		{
			new PropertySetup<Dictionary<int, string>>().ToString(), "Dictionary<int, string>"
		},
	};
}
