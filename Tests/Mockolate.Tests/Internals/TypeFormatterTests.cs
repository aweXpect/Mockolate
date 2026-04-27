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

	public static TheoryData<string, string> ProvideTestData()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		return new TheoryData<string, string>
		{
			{
				new PropertySetup<int>(registry, "Foo").ToString(), "int Foo"
			},
			{
				new PropertySetup<uint>(registry, "Foo").ToString(), "uint Foo"
			},
			{
				new PropertySetup<nint>(registry, "Foo").ToString(), "nint Foo"
			},
			{
				new PropertySetup<nuint>(registry, "Foo").ToString(), "nuint Foo"
			},
			{
				new PropertySetup<byte>(registry, "Foo").ToString(), "byte Foo"
			},
			{
				new PropertySetup<sbyte>(registry, "Foo").ToString(), "sbyte Foo"
			},
			{
				new PropertySetup<short>(registry, "Foo").ToString(), "short Foo"
			},
			{
				new PropertySetup<ushort>(registry, "Foo").ToString(), "ushort Foo"
			},
			{
				new PropertySetup<long>(registry, "Foo").ToString(), "long Foo"
			},
			{
				new PropertySetup<ulong>(registry, "Foo").ToString(), "ulong Foo"
			},
			{
				new PropertySetup<float>(registry, "Foo").ToString(), "float Foo"
			},
			{
				new PropertySetup<double>(registry, "Foo").ToString(), "double Foo"
			},
			{
				new PropertySetup<decimal>(registry, "Foo").ToString(), "decimal Foo"
			},
			{
				new PropertySetup<object>(registry, "Foo").ToString(), "object Foo"
			},
			{
				new PropertySetup<bool>(registry, "Foo").ToString(), "bool Foo"
			},
			{
				new PropertySetup<bool?>(registry, "Foo").ToString(), "bool? Foo"
			},
			{
				new PropertySetup<char>(registry, "Foo").ToString(), "char Foo"
			},
			{
				new PropertySetup<string>(registry, "Foo").ToString(), "string Foo"
			},
			{
				new PropertySetup<int[]>(registry, "Foo").ToString(), "int[] Foo"
			},
			{
				new PropertySetup<DateTime?>(registry, "Foo").ToString(), "DateTime? Foo"
			},
			{
				new PropertySetup<StreamReader>(registry, "Foo").ToString(), "StreamReader Foo"
			},
			{
				new PropertySetup<List<string>>(registry, "Foo").ToString(), "List<string> Foo"
			},
			{
				new PropertySetup<List<int?[]>>(registry, "Foo").ToString(), "List<int?[]> Foo"
			},
			{
				new PropertySetup<Dictionary<int, string>>(registry, "Foo").ToString(), "Dictionary<int, string> Foo"
			},
		};
	}
}
