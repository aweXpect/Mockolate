using System.Linq;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed class MockClassTests
{
	[Fact]
	public async Task ShouldAlwaysCreateMockClass()
	{
		int maxAdditionalInterfaces = 8;
		string expectedComment = "up to eight additional interfaces";

		GeneratorResult result = Generator.Run("");

		await That(result.Sources).ContainsKey("Mock.g.cs").WhoseValue
			.Contains(expectedComment).And
			.Contains("public static Mock<T> Create<T>(BaseClass.ConstructorParameters? constructorParameters = null)").And
			.Contains("public static Mock<T> Create<T>(MockBehavior mockBehavior)").And
			.Contains("public static Mock<T> Create<T>(BaseClass.ConstructorParameters constructorParameters, MockBehavior mockBehavior)");
		var source = result.Sources["Mock.g.cs"];
		for (int i = 1; i <= maxAdditionalInterfaces; i++)
		{
			// Creates "T2", "T2, T3", ..., "T2, T3, ..., T8"
			var types = string.Join(", ", Enumerable.Range(2, i).Select(x => $"T{x}"));
			await That(source)
				.Contains($"public static Mock<T, {types}> Create<T, {types}>(BaseClass.ConstructorParameters? constructorParameters = null)").And
				.Contains($"public static Mock<T, {types}> Create<T, {types}>(MockBehavior mockBehavior)").And
				.Contains($"public static Mock<T, {types}> Create<T, {types}>(BaseClass.ConstructorParameters constructorParameters, MockBehavior mockBehavior)");
		}
	}
}
