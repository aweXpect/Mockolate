namespace Mockolate.Tests;

public sealed class BaseClassTests
{
	[Fact]
	public async Task ConstructorWithInitSetter_ShouldSetParameters()
	{
		BaseClass.ConstructorParameters result = new([2, "bar",])
		{
			Parameters = [1, "foo",],
		};

		await That(result.Parameters).IsEqualTo([1, "foo",]);
	}

	[Fact]
	public async Task WithConstructorParameters_ShouldReturnParameters()
	{
		object?[] parameters =
		{
			42, "test", null, DateTime.Now,
		};

		BaseClass.ConstructorParameters result = BaseClass.WithConstructorParameters(parameters);

		await That(result.Parameters).IsEqualTo(parameters);
	}

	[Fact]
	public async Task WithConstructorParameters_WithNull_ShouldReturnArrayWithNullElement()
	{
		BaseClass.ConstructorParameters result = BaseClass.WithConstructorParameters(null);

		await That(result.Parameters).HasSingle().Which.IsNull();
	}

	[Fact]
	public async Task WithOptionalParameters_ShouldUseDefaultValues()
	{
		MyBaseClass mock = Mock.Create<MyBaseClass>(BaseClass.WithConstructorParameters(1));

		await That(mock.A).IsEqualTo(1);
		await That(mock.B).IsEqualTo(2);
		await That(mock.C).IsEqualTo("foo");
	}

	[Fact]
	public async Task WithOptionalParameters_WithAllExplicitValues_ShouldUseExplicitValues()
	{
		MyBaseClass mock = Mock.Create<MyBaseClass>(BaseClass.WithConstructorParameters(1, 4, "bar"));

		await That(mock.A).IsEqualTo(1);
		await That(mock.B).IsEqualTo(4);
		await That(mock.C).IsEqualTo("bar");
	}

	[Fact]
	public async Task WithOptionalParameters_WithOneExplicitValue_ShouldUseExplicitAndDefaultValues()
	{
		MyBaseClass mock = Mock.Create<MyBaseClass>(BaseClass.WithConstructorParameters(1, 4));

		await That(mock.A).IsEqualTo(1);
		await That(mock.B).IsEqualTo(4);
		await That(mock.C).IsEqualTo("foo");
	}

	public class MyBaseClass
	{
		public int A { get; }
		public int B { get; }
		public string C { get; }

		public MyBaseClass(int a, int b = 2, string c = "foo")
		{
			A = a;
			B = b;
			C = c;
		}
	}
}
