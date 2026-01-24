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
			new PropertySetup<int>("Foo").ToString(), "int Foo"
		},
		{
			new PropertySetup<uint>("Foo").ToString(), "uint Foo"
		},
		{
			new PropertySetup<nint>("Foo").ToString(), "nint Foo"
		},
		{
			new PropertySetup<nuint>("Foo").ToString(), "nuint Foo"
		},
		{
			new PropertySetup<byte>("Foo").ToString(), "byte Foo"
		},
		{
			new PropertySetup<sbyte>("Foo").ToString(), "sbyte Foo"
		},
		{
			new PropertySetup<short>("Foo").ToString(), "short Foo"
		},
		{
			new PropertySetup<ushort>("Foo").ToString(), "ushort Foo"
		},
		{
			new PropertySetup<long>("Foo").ToString(), "long Foo"
		},
		{
			new PropertySetup<ulong>("Foo").ToString(), "ulong Foo"
		},
		{
			new PropertySetup<float>("Foo").ToString(), "float Foo"
		},
		{
			new PropertySetup<double>("Foo").ToString(), "double Foo"
		},
		{
			new PropertySetup<decimal>("Foo").ToString(), "decimal Foo"
		},
		{
			new PropertySetup<object>("Foo").ToString(), "object Foo"
		},
		{
			new PropertySetup<bool>("Foo").ToString(), "bool Foo"
		},
		{
			new PropertySetup<bool?>("Foo").ToString(), "bool? Foo"
		},
		{
			new PropertySetup<char>("Foo").ToString(), "char Foo"
		},
		{
			new PropertySetup<string>("Foo").ToString(), "string Foo"
		},
		{
			new PropertySetup<int[]>("Foo").ToString(), "int[] Foo"
		},
		{
			new PropertySetup<DateTime?>("Foo").ToString(), "DateTime? Foo"
		},
		{
			new PropertySetup<StreamReader>("Foo").ToString(), "StreamReader Foo"
		},
		{
			new PropertySetup<List<string>>("Foo").ToString(), "List<string> Foo"
		},
		{
			new PropertySetup<List<int?[]>>("Foo").ToString(), "List<int?[]> Foo"
		},
		{
			new PropertySetup<Dictionary<int, string>>("Foo").ToString(), "Dictionary<int, string> Foo"
		},
	};
}
