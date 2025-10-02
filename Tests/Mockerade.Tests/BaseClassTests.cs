namespace Mockerade.Tests;

public sealed class BaseClassTests
{
	[Fact]
	public async Task WithConstructorParameters_ShouldReturnParameters()
	{
		var parameters = new object?[] { 42, "test", null, DateTime.Now, };

		var result = BaseClass.WithConstructorParameters(parameters);

		await That(result.Parameters).IsEqualTo(parameters);
	}

	[Fact]
	public async Task WithConstructorParameters_WithNull_ShouldReturnArrayWithNullElement()
	{
		var result = BaseClass.WithConstructorParameters(null);

		await That(result.Parameters).HasSingle().Which.IsNull();
	}

	[Fact]
	public async Task ConstructorWithInitSetter_ShouldSetParameters()
	{
		var result = new BaseClass.ConstructorParameters([2, "bar",])
		{
			Parameters = [1, "foo",]
		};

		await That(result.Parameters).IsEqualTo([1, "foo"]);
	}
}
