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

		await That(result.Sources).ContainsKey("MockForMyBaseClass.g.cs").And
			.ContainsKey("MockRegistration.g.cs").WhoseValue
			.DoesNotContain(
				"throw new MockException(\"No parameterless constructor found for 'MyBaseClass'. Please provide constructor parameters.\");");
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

		await That(result.Sources).ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains("""
			          				if (constructorParameters.Parameters.Length == 1
			          				    && TryCast(constructorParameters.Parameters[0], mockBehavior, out int c1p1))
			          				{
			          					MockRegistration mockRegistration = new MockRegistration(mockBehavior, "MyCode.MyBaseClass");
			          					MockForMyBaseClass.MockRegistrationsProvider.Value = mockRegistration;
			          					_value = new MockForMyBaseClass(c1p1, mockRegistration);
			          				}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          				if (constructorParameters.Parameters.Length == 2
			          				    && TryCast(constructorParameters.Parameters[0], mockBehavior, out int c2p1)
			          				    && TryCast(constructorParameters.Parameters[1], mockBehavior, out bool c2p2))
			          				{
			          					MockRegistration mockRegistration = new MockRegistration(mockBehavior, "MyCode.MyBaseClass");
			          					MockForMyBaseClass.MockRegistrationsProvider.Value = mockRegistration;
			          					_value = new MockForMyBaseClass(c2p1, c2p2, mockRegistration);
			          				}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          				if (constructorParameters is null || constructorParameters.Parameters.Length == 0)
			          				{
			          					throw new MockException("No parameterless constructor found for 'MyCode.MyBaseClass'. Please provide constructor parameters.");
			          				}
			          """).IgnoringNewlineStyle();

		await That(result.Sources).ContainsKey("MockForMyBaseClass.g.cs").WhoseValue
			.Contains("""
			          	public MockForMyBaseClass(int value, MockRegistration mockRegistration)
			          			: base(value)
			          	{
			          		_mock = new Mock<MyCode.MyBaseClass>(this, mockRegistration);
			          		_mockRegistrations = mockRegistration;
			          	}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          	public MockForMyBaseClass(int value, bool flag, MockRegistration mockRegistration)
			          			: base(value, flag)
			          	{
			          		_mock = new Mock<MyCode.MyBaseClass>(this, mockRegistration);
			          		_mockRegistrations = mockRegistration;
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

		await That(result.Sources).DoesNotContainKey("MockForMyBaseClass.g.cs").And
			.ContainsKey("MockForMyBaseClassExtensions.g.cs").And
			.ContainsKey("MockRegistration.g.cs").WhoseValue
			.Contains(
				"throw new MockException(\"Could not find any constructor at all for the base type 'MyCode.MyBaseClass'. Therefore mocking is not supported!\");");
	}

	[Fact]
	public async Task ForTypesWithSealedOverrideEvent_ShouldNotOverrideEvent()
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
			     		_ = Mock.Create<MyClassWithSealedEvents>();
			         }
			     }

			     public class MyClassWithSealedEvents : MySubClass
			     {
			     	public sealed override event EventHandler<long> SomeEvent;
			     }

			     public class MySubClass
			     {
			     	public virtual event EventHandler<long> SomeEvent;
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyClassWithSealedEvents.g.cs").WhoseValue
			.DoesNotContain("event System.EventHandler<long>? SomeEvent");
	}

	[Fact]
	public async Task ForTypesWithSealedOverrideIndexer_ShouldNotOverrideIndexer()
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
			     		_ = Mock.Create<MyClassWithSealedIndexers>();
			         }
			     }

			     public class MyClassWithSealedIndexers : MySubClass
			     {
			     	public sealed override int this[int index] { get => 3 * index; }
			     }

			     public class MySubClass
			     {
			     	public virtual int this[int index] { get => 2 * index; }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyClassWithSealedIndexers.g.cs").WhoseValue
			.DoesNotContain("override int this[int index]");
	}

	[Fact]
	public async Task ForTypesWithSealedOverrideMethod_ShouldNotOverrideMethod()
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
			     		_ = Mock.Create<MyClassWithSealedMethods>();
			         }
			     }

			     public class MyClassWithSealedMethods : MySubClass
			     {
			     	public sealed override void MyMethod(int value)
			     		=> base.MyMethod(value);
			     }

			     public class MySubClass
			     {
			     	public virtual void MyMethod(int value) { }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyClassWithSealedMethods.g.cs").WhoseValue
			.DoesNotContain("override void MyMethod(int value)");
	}

	[Fact]
	public async Task ForTypesWithSealedOverrideProperty_ShouldNotOverrideProperty()
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
			     		_ = Mock.Create<MyClassWithSealedProperties>();
			         }
			     }

			     public class MyClassWithSealedProperties : MySubClass
			     {
			     	public sealed override int MyProperty { get; set; }
			     }

			     public class MySubClass
			     {
			     	public virtual int MyProperty { get; set; }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyClassWithSealedProperties.g.cs").WhoseValue
			.DoesNotContain("override int MyProperty");
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

		await That(result.Sources).ContainsKey("MockForIMyServiceListMyData.g.cs").WhoseValue
			.Contains("using Mockolate.Setup;").And
			.DoesNotContain("using MyCode.Services;").And
			.DoesNotContain("using MyCode.Models;");
	}
}
