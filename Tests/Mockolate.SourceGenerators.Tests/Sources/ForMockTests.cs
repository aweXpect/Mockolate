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
				"throw new global::Mockolate.Exceptions.MockException(\"No parameterless constructor found for 'MyBaseClass'. Please provide constructor parameters.\");");
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
			          				    && TryCast(constructorParameters.Parameters, 0, mockBehavior, out int c1p1))
			          				{
			          					global::Mockolate.MockRegistration mockRegistration = new global::Mockolate.MockRegistration(mockBehavior, "MyCode.MyBaseClass");
			          					global::Mockolate.Generated.MockForMyBaseClass.MockRegistrationsProvider.Value = mockRegistration;
			          					if (setups.Length > 0)
			          					{
			          						#pragma warning disable CS0618
			          						global::Mockolate.Setup.IMockSetup<MyCode.MyBaseClass> setupTarget = new global::Mockolate.MockSetup<MyCode.MyBaseClass>(mockRegistration);
			          						#pragma warning restore CS0618
			          						foreach (global::System.Action<global::Mockolate.Setup.IMockSetup<MyCode.MyBaseClass>> setup in setups)
			          						{
			          							setup.Invoke(setupTarget);
			          						}
			          					}
			          					_value = new global::Mockolate.Generated.MockForMyBaseClass(mockRegistration, c1p1);
			          				}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          				if (constructorParameters.Parameters.Length == 2
			          				    && TryCast(constructorParameters.Parameters, 0, mockBehavior, out int c2p1)
			          				    && TryCast(constructorParameters.Parameters, 1, mockBehavior, out bool c2p2))
			          				{
			          					global::Mockolate.MockRegistration mockRegistration = new global::Mockolate.MockRegistration(mockBehavior, "MyCode.MyBaseClass");
			          					global::Mockolate.Generated.MockForMyBaseClass.MockRegistrationsProvider.Value = mockRegistration;
			          					if (setups.Length > 0)
			          					{
			          						#pragma warning disable CS0618
			          						global::Mockolate.Setup.IMockSetup<MyCode.MyBaseClass> setupTarget = new global::Mockolate.MockSetup<MyCode.MyBaseClass>(mockRegistration);
			          						#pragma warning restore CS0618
			          						foreach (global::System.Action<global::Mockolate.Setup.IMockSetup<MyCode.MyBaseClass>> setup in setups)
			          						{
			          							setup.Invoke(setupTarget);
			          						}
			          					}
			          					_value = new global::Mockolate.Generated.MockForMyBaseClass(mockRegistration, c2p1, c2p2);
			          				}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          				if (constructorParameters is null || constructorParameters.Parameters.Length == 0)
			          				{
			          					throw new global::Mockolate.Exceptions.MockException("No parameterless constructor found for 'MyCode.MyBaseClass'. Please provide constructor parameters.");
			          				}
			          """).IgnoringNewlineStyle();

		await That(result.Sources).ContainsKey("MockForMyBaseClass.g.cs").WhoseValue
			.Contains("""
			          	public MockForMyBaseClass(global::Mockolate.MockRegistration mockRegistration, int value)
			          			: base(value)
			          	{
			          		_mock = new global::Mockolate.Mock<MyCode.MyBaseClass>(this, mockRegistration, new object?[] { value });
			          		_mockRegistrations = mockRegistration;
			          	}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          	public MockForMyBaseClass(global::Mockolate.MockRegistration mockRegistration, int value, bool flag)
			          			: base(value, flag)
			          	{
			          		_mock = new global::Mockolate.Mock<MyCode.MyBaseClass>(this, mockRegistration, new object?[] { value, flag });
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
				"throw new global::Mockolate.Exceptions.MockException(\"Could not find any constructor at all for the base type 'MyCode.MyBaseClass'. Therefore mocking is not supported!\");");
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
	public async Task MethodOrIndexerParametersWithReservedNames_ShouldPrefixAtSymbol()
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
			         string this[int @true] { get; set; }
			         void DoSomething(int @event);
			     }
			     """);

		await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
			.Contains("""
			          public string this[int @true]
			          """).And
			.Contains("""
			          public void DoSomething(int @event)
			          """);

		await That(result.Sources).ContainsKey("MockForIMyServiceExtensions.g.cs").WhoseValue
			.Contains("""
			          		/// <summary>
			          		///     Setup for the method <see cref="MyCode.IMyService.DoSomething(int)"/> with the given <paramref name="@event"/>.
			          		/// </summary>
			          		public global::Mockolate.Setup.IVoidMethodSetup<int> DoSomething(global::Mockolate.Parameters.IParameter<int>? @event)
			          		{
			          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int>("MyCode.IMyService.DoSomething", new global::Mockolate.Parameters.NamedParameter("@event", (global::Mockolate.Parameters.IParameter)(@event ?? global::Mockolate.It.IsNull<int>())));
			          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		/// <summary>
			          		///     Sets up the string indexer on the mock for <see cref="MyCode.IMyService" />.
			          		/// </summary>
			          		public global::Mockolate.Setup.IndexerSetup<string, int> Indexer(global::Mockolate.Parameters.IParameter<int>? parameter1)
			          		{
			          			var indexerSetup = new global::Mockolate.Setup.IndexerSetup<string, int>(new global::Mockolate.Parameters.NamedParameter("@true", (global::Mockolate.Parameters.IParameter)(parameter1 ?? global::Mockolate.It.IsNull<int>())));
			          			CastToMockRegistrationOrThrow(setup).SetupIndexer(indexerSetup);
			          			return indexerSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		/// <summary>
			          		///     Validates the invocations for the method <see cref="MyCode.IMyService.DoSomething(int)"/> with the given <paramref name="@event"/>.
			          		/// </summary>
			          		public global::Mockolate.Verify.VerificationResult<MyCode.IMyService> DoSomething(global::Mockolate.Parameters.IParameter<int>? @event)
			          			=> CastToMockOrThrow(verifyInvoked).Method("MyCode.IMyService.DoSomething", new global::Mockolate.Parameters.NamedParameter("@event", (global::Mockolate.Parameters.IParameter)(@event ?? global::Mockolate.It.IsNull<int>())));
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		/// <summary>
			          		///     Verifies the indexer read access for <see cref="MyCode.IMyService"/> on the mock.
			          		/// </summary>
			          		public global::Mockolate.Verify.VerificationResult<MyCode.IMyService> GotIndexer(global::Mockolate.Parameters.IParameter<int>? parameter1)
			          		{
			          			return CastToMockOrThrow(verify).GotIndexer(new global::Mockolate.Parameters.NamedParameter("@true", (global::Mockolate.Parameters.IParameter)(parameter1 ?? global::Mockolate.It.IsNull<int>())));
			          		}
			          """).IgnoringNewlineStyle();
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
