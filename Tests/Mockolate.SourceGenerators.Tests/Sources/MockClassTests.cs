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
			.Contains(expectedComment)
			.And.Contains(
				"public static T Create<T>(params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)")
			.And.Contains(
				"public static T Create<T>(global::Mockolate.BaseClass.ConstructorParameters constructorParameters, params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)")
			.And.Contains(
				"public static T Create<T>(global::Mockolate.MockBehavior mockBehavior, params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)")
			.And.Contains(
				"public static T Create<T>(global::Mockolate.BaseClass.ConstructorParameters constructorParameters, global::Mockolate.MockBehavior mockBehavior, params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)");
		string source = result.Sources["Mock.g.cs"];
		for (int i = 1; i <= maxAdditionalInterfaces; i++)
		{
			// Creates "T2", "T2, T3", ..., "T2, T3, ..., T8"
			string types = string.Join(", ", Enumerable.Range(2, i).Select(x => $"T{x}"));
			await That(source)
				.Contains(
					$"public static T Create<T, {types}>(params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)")
				.And.Contains(
					$"public static T Create<T, {types}>(global::Mockolate.MockBehavior mockBehavior, params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)")
				.And.Contains(
					$"public static T Create<T, {types}>(global::Mockolate.BaseClass.ConstructorParameters constructorParameters, params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)")
				.And.Contains(
					$"public static T Create<T, {types}>(global::Mockolate.BaseClass.ConstructorParameters constructorParameters, global::Mockolate.MockBehavior mockBehavior, params global::System.Action<global::Mockolate.Setup.IMockSetup<T>>[] setups)");
		}
	}

	[Fact]
	public async Task ShouldSupportSpecialTypes()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using Mockolate;

			     namespace MyCode;
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<IMyService>();
			         }
			     }

			     public interface IMyService
			     {
			         void MyMethod(object v1, bool v2, string v3, char v4, byte v5, sbyte v6, short v7, ushort v8, int v9, uint v10, long v11, ulong v12, float v13, double v14, decimal v15);
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
			.Contains("""
			          	public void MyMethod(object v1, bool v2, string v3, char v4, byte v5, sbyte v6, short v7, ushort v8, int v9, uint v10, long v11, ulong v12, float v13, double v14, decimal v15)
			          	{
			          		global::Mockolate.Setup.MethodSetupResult methodExecution = MockRegistrations.InvokeMethod("MyCode.IMyService.MyMethod", new global::Mockolate.Parameters.NamedParameterValue("v1", v1), new global::Mockolate.Parameters.NamedParameterValue("v2", v2), new global::Mockolate.Parameters.NamedParameterValue("v3", v3), new global::Mockolate.Parameters.NamedParameterValue("v4", v4), new global::Mockolate.Parameters.NamedParameterValue("v5", v5), new global::Mockolate.Parameters.NamedParameterValue("v6", v6), new global::Mockolate.Parameters.NamedParameterValue("v7", v7), new global::Mockolate.Parameters.NamedParameterValue("v8", v8), new global::Mockolate.Parameters.NamedParameterValue("v9", v9), new global::Mockolate.Parameters.NamedParameterValue("v10", v10), new global::Mockolate.Parameters.NamedParameterValue("v11", v11), new global::Mockolate.Parameters.NamedParameterValue("v12", v12), new global::Mockolate.Parameters.NamedParameterValue("v13", v13), new global::Mockolate.Parameters.NamedParameterValue("v14", v14), new global::Mockolate.Parameters.NamedParameterValue("v15", v15));
			          		if (this._wrapped is not null)
			          		{
			          			this._wrapped.MyMethod(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
			          		}
			          		methodExecution.TriggerCallbacks(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
			          	}
			          """).IgnoringNewlineStyle();
	}
}
