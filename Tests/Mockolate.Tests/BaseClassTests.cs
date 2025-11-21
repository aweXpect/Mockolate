namespace Mockolate.Tests;

public sealed class BaseClassTests
{
	[Test]
	public async Task ConstructorWithInitSetter_ShouldSetParameters()
	{
		BaseClass.ConstructorParameters result = new([2, "bar",])
		{
			Parameters = [1, "foo",],
		};

		await That(result.Parameters).IsEqualTo([1, "foo",]);
	}

	[Test]
	public async Task WithConstructorParameters_ShouldReturnParameters()
	{
		object?[] parameters =
		{
			42, "test", null, DateTime.Now,
		};

		BaseClass.ConstructorParameters result = BaseClass.WithConstructorParameters(parameters);

		await That(result.Parameters).IsEqualTo(parameters);
	}

	[Test]
	public async Task WithConstructorParameters_WithNull_ShouldReturnArrayWithNullElement()
	{
		BaseClass.ConstructorParameters result = BaseClass.WithConstructorParameters(null);

		await That(result.Parameters).HasSingle().Which.IsNull();
	}
}
