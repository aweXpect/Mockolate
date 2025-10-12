using System.Collections.Generic;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed partial class ForMockTests
{
	[Fact]
	public async Task ForTypesWithAdditionalConstructorsWithParameters_ShouldWorkForAllNonPrivateConstructors()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyBaseClass>();
			         }
			     }

			     public class MyBaseClass
			     {
			         public MyBaseClass() { }
			         public MyBaseClass(int value) { }
			         protected MyBaseClass(int value, bool flag) { }
			     }
			     """);

		await That(result.Sources).ContainsKey("ForMyBaseClass.g.cs").WhoseValue
			.Contains("""
			          		public MockSubject(IMock mock)
			          			: base()
			          		{
			          			_mock = mock;
			          		}
			          """).IgnoringNewlineStyle().And
			.DoesNotContain("""
			                			if (constructorParameters is null || constructorParameters.Parameters.Length == 0)
			                			{
			                				throw new MockException("No parameterless constructor found for 'MyBaseClass'. Please provide constructor parameters.");
			                			}
			                """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task ForTypesWithConstructorWithParameters_ShouldWorkForAllNonPrivateConstructors()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyBaseClass>();
			         }
			     }

			     public class MyBaseClass
			     {
			         public MyBaseClass(int value) { }
			         protected MyBaseClass(int value, bool flag) { }
			     }
			     """);

		await That(result.Sources).ContainsKey("ForMyBaseClass.g.cs").WhoseValue
			.Contains("""
			          			if (constructorParameters.Parameters.Length == 1
			          			    && TryCast(constructorParameters.Parameters[0], out int p1))
			          			{
			          				Subject = new MockSubject(this, p1);
			          			}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          			if (constructorParameters.Parameters.Length == 2
			          			    && TryCast(constructorParameters.Parameters[0], out int p1)
			          			    && TryCast(constructorParameters.Parameters[1], out bool p2))
			          			{
			          				Subject = new MockSubject(this, p1, p2);
			          			}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          		public MockSubject(IMock mock, int value)
			          			: base(value)
			          		{
			          			_mock = mock;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public MockSubject(IMock mock, int value, bool flag)
			          			: base(value, flag)
			          		{
			          			_mock = mock;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          			if (constructorParameters is null || constructorParameters.Parameters.Length == 0)
			          			{
			          				throw new MockException("No parameterless constructor found for 'MyBaseClass'. Please provide constructor parameters.");
			          			}
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task ForTypesWithoutPublicOrProtectedConstructor_ShouldOnlyGenerateMockThatThrowsException()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyBaseClass>();
			         }
			     }

			     public class MyBaseClass
			     {
			         private MyBaseClass() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("ForMyBaseClass.g.cs").WhoseValue
			.Contains("public class Mock : Mock<MyBaseClass>").And
			.Contains(
				"throw new MockException(\"Could not find any constructor at all for the base type 'MyBaseClass'. Therefore mocking is not supported!\");")
			.And
			.DoesNotContain("public partial class MockSubject");
	}

	[Fact]
	public async Task ShouldIncludeNamespacesFromMockTypes()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System.Collections.Generic;
			     using Mockolate;

			     namespace MyCode;
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<IMyService<List<int>>>();
			         }
			     }

			     public class IMyService<T> { }
			     """, typeof(List<>));

		await That(result.Sources).ContainsKey("ForIMyServiceListint.g.cs").WhoseValue
			.Contains("using System.Collections.Generic;").And
			.Contains("using MyCode;");
	}
}
