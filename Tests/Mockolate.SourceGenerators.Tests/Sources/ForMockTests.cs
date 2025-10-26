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
			          					if (_constructorParameters.Parameters.Length == 1
			          					    && TryCast(_constructorParameters.Parameters[0], out int c1p1))
			          					{
			          						MockSubject._mockProvider.Value = this;
			          						_subject = new MockSubject(this, c1p1);
			          					}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          					if (_constructorParameters.Parameters.Length == 2
			          					    && TryCast(_constructorParameters.Parameters[0], out int c2p1)
			          					    && TryCast(_constructorParameters.Parameters[1], out bool c2p2))
			          					{
			          						MockSubject._mockProvider.Value = this;
			          						_subject = new MockSubject(this, c2p1, c2p2);
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
			          					if (_constructorParameters is null || _constructorParameters.Parameters.Length == 0)
			          					{
			          						throw new MockException("No parameterless constructor found for 'MyCode.MyBaseClass'. Please provide constructor parameters.");
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
			.Contains("public class Mock : Mock<MyCode.MyBaseClass>").And
			.Contains(
				"throw new MockException(\"Could not find any constructor at all for the base type 'MyCode.MyBaseClass'. Therefore mocking is not supported!\");")
			.And
			.DoesNotContain("public partial class MockSubject");
	}

	[Fact]
	public async Task ShouldNotIncludeNamespacesFromMockTypes()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System.Collections.Generic;
			     using Mockolate;
			     using MyCode.Models;
			     using MyCode.Services;

			     namespace MyCode
			     {
			     	public class Program
			     	{
			     	    public static void Main(string[] args)
			     	    {
			     			_ = Mock.Create<IMyService<List<MyData>>>();
			     	    }
			     	}
			     }
			     
			     namespace MyCode.Services
			     {
			         public interface IMyService<T> { }
			     }
			     
			     namespace MyCode.Models
			     {
			         public record MyData(int Value);
			     }
			     
			     """, typeof(List<>));

		 await That(result.Sources).ContainsKey("ForIMyServiceListMyData.g.cs").WhoseValue
			.Contains("using Mockolate.Setup;").And
			.DoesNotContain("using MyCode.Services;").And
			.DoesNotContain("using MyCode.Models;");
	}
}
