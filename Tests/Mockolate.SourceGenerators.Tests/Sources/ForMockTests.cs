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
			          					if (setups.Length > 0)
			          					{
			          						#pragma warning disable CS0618
			          						IMockSetup<MyCode.MyBaseClass> setupTarget = new MockSetup<MyCode.MyBaseClass>(mockRegistration);
			          						#pragma warning restore CS0618
			          						foreach (Action<IMockSetup<MyCode.MyBaseClass>> setup in setups)
			          						{
			          							setup.Invoke(setupTarget);
			          						}
			          					}
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
			          					if (setups.Length > 0)
			          					{
			          						#pragma warning disable CS0618
			          						IMockSetup<MyCode.MyBaseClass> setupTarget = new MockSetup<MyCode.MyBaseClass>(mockRegistration);
			          						#pragma warning restore CS0618
			          						foreach (Action<IMockSetup<MyCode.MyBaseClass>> setup in setups)
			          						{
			          							setup.Invoke(setupTarget);
			          						}
			          					}
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

	[Fact]
	public async Task ShouldIncludeInternalMethodsFromBaseClass()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyBaseClass
			     {
			     }

			     public class MyBaseClass
			     {
			     	internal virtual void InternalMethod() { }
			     	public virtual void PublicMethod() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.Contains("InternalMethod").And
			.Contains("PublicMethod");
	}

	[Fact]
	public async Task ShouldIncludeInternalPropertiesFromBaseClass()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyBaseClass
			     {
			     }

			     public class MyBaseClass
			     {
			     	internal virtual int InternalProperty { get; set; }
			     	public virtual int PublicProperty { get; set; }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.Contains("InternalProperty").And
			.Contains("PublicProperty");
	}

	[Fact]
	public async Task ShouldIncludeProtectedInternalMethodsFromBaseClass()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyBaseClass
			     {
			     }

			     public class MyBaseClass
			     {
			     	protected internal virtual void ProtectedInternalMethod() { }
			     	protected virtual void ProtectedMethod() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.Contains("ProtectedInternalMethod").And
			.Contains("ProtectedMethod");
	}

	[Fact]
	public async Task ShouldNotIncludeSealedOverrideMethodsFromBaseClass()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyMiddleClass
			     {
			     }

			     public class MyMiddleClass : MyBaseClass
			     {
			     	public sealed override void SomeMethod() { }
			     }

			     public class MyBaseClass
			     {
			     	public virtual void SomeMethod() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.DoesNotContain("override void SomeMethod");
	}

	[Fact]
	public async Task ShouldNotIncludeSealedOverridePropertiesFromBaseClass()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyMiddleClass
			     {
			     }

			     public class MyMiddleClass : MyBaseClass
			     {
			     	public sealed override int SomeProperty { get; set; }
			     }

			     public class MyBaseClass
			     {
			     	public virtual int SomeProperty { get; set; }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.DoesNotContain("override int SomeProperty");
	}

	[Fact]
	public async Task ShouldHandleComplexInheritanceWithSealedAndInternalMembers()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyMiddleClass
			     {
			     }

			     public class MyMiddleClass : MyBaseClass
			     {
			     	public sealed override void SealedMethod() { }
			     	protected internal override void ProtectedInternalMethod() { }
			     }

			     public class MyBaseClass
			     {
			     	public virtual void SealedMethod() { }
			     	public virtual void NormalMethod() { }
			     	protected internal virtual void ProtectedInternalMethod() { }
			     	internal virtual void InternalMethod() { }
			     	protected virtual void ProtectedMethod() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.DoesNotContain("override void SealedMethod").And
			.Contains("ProtectedInternalMethod").And
			.Contains("InternalMethod").And
			.Contains("override void NormalMethod").And
			.Contains("override void ProtectedMethod");
	}

	[Fact]
	public async Task ShouldNotIncludeSealedOverrideSpecialMethods()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyMiddleClass
			     {
			     }

			     public class MyMiddleClass : MyBaseClass
			     {
			     	public sealed override bool Equals(object? obj) => base.Equals(obj);
			     	public sealed override int GetHashCode() => base.GetHashCode();
			     	public sealed override string? ToString() => base.ToString();
			     }

			     public class MyBaseClass
			     {
			     	public virtual void SomeMethod() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.DoesNotContain("override bool Equals").And
			.DoesNotContain("override int GetHashCode").And
			.DoesNotContain("override string ToString");
	}

	[Fact]
	public async Task ShouldNotIncludeSealedOverrideSpecialMethodsWithNonNullableParameters()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = Mock.Create<MyDerivedClass>();
			         }
			     }

			     public class MyDerivedClass : MyMiddleClass
			     {
			     }

			     public class MyMiddleClass : MyBaseClass
			     {
			     	public sealed override bool Equals(object obj) => base.Equals(obj);
			     }

			     public class MyBaseClass
			     {
			     	public virtual void SomeMethod() { }
			     }
			     """);

		// Even though MyMiddleClass.Equals has non-nullable parameter (object),
		// it should still match and filter out object.Equals with nullable parameter (object?)
		await That(result.Sources).ContainsKey("MockForMyDerivedClass.g.cs").WhoseValue
			.DoesNotContain("override bool Equals");
	}
}
