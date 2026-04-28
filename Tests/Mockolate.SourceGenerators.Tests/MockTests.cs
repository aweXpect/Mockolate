using System.Collections;
using System.Collections.Generic;

namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task DeeplyNestedClass_ShouldSetupAndVerifyForAllInheritedTypesExceptExplicitInterfaceMembers()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Collections.Generic;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode;

			     public interface INestedInterface
			     {
			     	int NestedValue { get; }
			     	int NestedMethod();
			     	event EventHandler NestedEvent;
			     }
			     public interface IParentInterface : INestedInterface
			     {
			     	int ParentValue { get; }
			     	int ParentMethod();
			     	event EventHandler ParentEvent;
			     }
			     public interface IDirectInterface
			     {
			     	int DirectValue { get; }
			     	int DirectMethod();
			     	event EventHandler DirectEvent;
			     }
			     public abstract class BaseClass : IParentInterface
			     {
			     	public virtual int BaseClassValue { get; } = 1;
			     	public virtual int BaseClassMethod() => 1;
			     	public virtual event EventHandler? BaseClassEvent;
			     	int IParentInterface.ParentValue { get; } = 2;
			     	int IParentInterface.ParentMethod() => 2;
			     	event EventHandler IParentInterface.ParentEvent;
			     	int INestedInterface.NestedValue { get; } = 3;
			     	int INestedInterface.NestedMethod() => 3;
			     	event EventHandler INestedInterface.NestedEvent;
			     }
			     public class OuterClass : BaseClass, IDirectInterface
			     {
			     	public virtual int OuterValue { get; } = 4;
			     	public virtual int OuterMethod() => 1;
			     	public virtual event EventHandler? OuterEvent;
			     	int IDirectInterface.DirectValue { get; } = 5;
			     	int IDirectInterface.DirectMethod() => 5;
			     	event EventHandler IDirectInterface.DirectEvent;
			     }
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = OuterClass.CreateMock();
			         }
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.OuterClass.g.cs").WhoseValue
			.Contains("global::Mockolate.Setup.PropertySetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.OuterValue").And
			.Contains("global::Mockolate.Setup.PropertySetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.BaseClassValue").And
			.DoesNotContain("global::Mockolate.Setup.PropertySetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.DirectValue").And
			.DoesNotContain("global::Mockolate.Setup.PropertySetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.ParentValue").And
			.DoesNotContain("global::Mockolate.Setup.PropertySetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.NestedValue").And
			.Contains("global::Mockolate.Setup.IReturnMethodSetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.OuterMethod()").And
			.Contains("global::Mockolate.Setup.IReturnMethodSetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.BaseClassMethod()").And
			.DoesNotContain("global::Mockolate.Setup.IReturnMethodSetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.DirectMethod()").And
			.DoesNotContain("global::Mockolate.Setup.IReturnMethodSetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.ParentMethod()").And
			.DoesNotContain("global::Mockolate.Setup.IReturnMethodSetup<int> global::Mockolate.Mock.IMockSetupForOuterClass.NestedMethod()").And
			.Contains("void IMockRaiseOnOuterClass.OuterEvent(object? sender, global::System.EventArgs e)").And
			.Contains("void IMockRaiseOnOuterClass.BaseClassEvent(object? sender, global::System.EventArgs e)").And
			.DoesNotContain("void IMockRaiseOnOuterClass.DirectEvent(object? sender, global::System.EventArgs e)").And
			.DoesNotContain("void IMockRaiseOnOuterClass.ParentEvent(object? sender, global::System.EventArgs e)").And
			.DoesNotContain("void IMockRaiseOnOuterClass.NestedEvent(object? sender, global::System.EventArgs e)").And
			.Contains("global::Mockolate.Verify.VerificationPropertyResult<IMockVerifyForOuterClass, int> IMockVerifyForOuterClass.OuterValue").And
			.Contains("global::Mockolate.Verify.VerificationPropertyResult<IMockVerifyForOuterClass, int> IMockVerifyForOuterClass.BaseClassValue").And
			.DoesNotContain("global::Mockolate.Verify.VerificationPropertyResult<IMockVerifyForOuterClass, int> IMockVerifyForOuterClass.DirectValue").And
			.DoesNotContain("global::Mockolate.Verify.VerificationPropertyResult<IMockVerifyForOuterClass, int> IMockVerifyForOuterClass.ParentValue").And
			.DoesNotContain("global::Mockolate.Verify.VerificationPropertyResult<IMockVerifyForOuterClass, int> IMockVerifyForOuterClass.NestedValue").And
			.Contains("global::Mockolate.Verify.VerificationResult<IMockVerifyForOuterClass>.IgnoreParameters IMockVerifyForOuterClass.OuterMethod()").And
			.Contains("global::Mockolate.Verify.VerificationResult<IMockVerifyForOuterClass>.IgnoreParameters IMockVerifyForOuterClass.BaseClassMethod()").And
			.DoesNotContain("IMockVerifyForOuterClass.DirectMethod()").And
			.DoesNotContain("IMockVerifyForOuterClass.ParentMethod()").And
			.DoesNotContain("IMockVerifyForOuterClass.NestedMethod()").And
			.Contains("global::Mockolate.Verify.VerificationEventResult<IMockVerifyForOuterClass> IMockVerifyForOuterClass.OuterEvent").And
			.Contains("global::Mockolate.Verify.VerificationEventResult<IMockVerifyForOuterClass> IMockVerifyForOuterClass.BaseClassEvent").And
			.DoesNotContain("global::Mockolate.Verify.VerificationEventResult<IMockVerifyForOuterClass> IMockVerifyForOuterClass.DirectEvent").And
			.DoesNotContain("global::Mockolate.Verify.VerificationEventResult<IMockVerifyForOuterClass> IMockVerifyForOuterClass.ParentEvent").And
			.DoesNotContain("global::Mockolate.Verify.VerificationEventResult<IMockVerifyForOuterClass> IMockVerifyForOuterClass.NestedEvent");
	}

	[Fact]
	public async Task ExplicitInterfaceImplementation_ShouldNotAddAccessibility()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Collections;
			     using System.Collections.Generic;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode;

			     public abstract class MyService : IEnumerable<int>
			     {
			     	public IEnumerator<int> GetEnumerator()
			     	{
			     		return new List<int>().GetEnumerator();
			     	}

			     	IEnumerator IEnumerable.GetEnumerator()
			     	{
			     		return GetEnumerator();
			     	}
			     }
			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyService.CreateMock();
			         }
			     }
			     """, typeof(IEnumerator), typeof(IEnumerable<int>));

		await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
			.DoesNotContain("private global::System.Collections.IEnumerator GetEnumerator()");
	}

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
			     		_ = MyBaseClass.CreateMock();
			         }
			     }

			     public class MyBaseClass
			     {
			         public MyBaseClass() { }
			         public MyBaseClass(int value) { }
			         protected MyBaseClass(int value, bool flag) { }
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs").WhoseValue
			.DoesNotContain("No parameterless constructor found");
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
			     		_ = MyBaseClass.CreateMock();
			         }
			     }

			     public class MyBaseClass
			     {
			         public MyBaseClass(int value) { }
			         protected MyBaseClass(int value, bool flag) { }
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs").WhoseValue
			.Contains("""
			          			if (constructorParameters.Length == 1
			          			    && TryCast(constructorParameters, 0, mockRegistry.Behavior, out int c1p1))
			          			{
			          				global::Mockolate.Mock.MyBaseClass.MockRegistryProvider.Value = mockRegistry;
			          				global::Mockolate.MockExtensionsForMyBaseClass.MockSetup? setupTarget = null;
			          				if (setup is not null)
			          				{
			          					setupTarget ??= new(mockRegistry);
			          					setup.Invoke(setupTarget);
			          				}
			          				return new global::Mockolate.Mock.MyBaseClass(mockRegistry, c1p1);
			          			}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          			if (constructorParameters.Length == 2
			          			    && TryCast(constructorParameters, 0, mockRegistry.Behavior, out int c2p1)
			          			    && TryCast(constructorParameters, 1, mockRegistry.Behavior, out bool c2p2))
			          			{
			          				global::Mockolate.Mock.MyBaseClass.MockRegistryProvider.Value = mockRegistry;
			          				global::Mockolate.MockExtensionsForMyBaseClass.MockSetup? setupTarget = null;
			          				if (setup is not null)
			          				{
			          					setupTarget ??= new(mockRegistry);
			          					setup.Invoke(setupTarget);
			          				}
			          				return new global::Mockolate.Mock.MyBaseClass(mockRegistry, c2p1, c2p2);
			          			}
			          """.TrimStart()).IgnoringNewlineStyle().And
			.Contains("""
			          			if (constructorParameters is null || constructorParameters.Length == 0)
			          			{
			          				throw new global::Mockolate.Exceptions.MockException("No parameterless constructor found for 'MyCode.MyBaseClass'. Please provide constructor parameters.");
			          			}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public MyBaseClass(global::Mockolate.MockRegistry mockRegistry, int value)
			          			: base(value)
			          		{
			          			this.MockRegistry = mockRegistry;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		public MyBaseClass(global::Mockolate.MockRegistry mockRegistry, int value, bool flag)
			          			: base(value, flag)
			          		{
			          			this.MockRegistry = mockRegistry;
			          		}
			          """).IgnoringNewlineStyle();
	}

	[Fact]
	public async Task ForTypesWithOnlyParameterlessConstructor_ShouldOmitConstructorParametersOverloads()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyBaseClass.CreateMock();
			         }
			     }

			     public class MyBaseClass
			     {
			         public MyBaseClass() { }
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs").WhoseValue
			.DoesNotContain("CreateMock(object?[] constructorParameters)").And
			.DoesNotContain("CreateMock(global::Mockolate.MockBehavior mockBehavior, object?[] constructorParameters)").And
			.DoesNotContain("object?[] constructorParameters)").And
			.Contains(
				"private static global::MyCode.MyBaseClass CreateMock(global::Mockolate.MockBehavior? mockBehavior, global::System.Action<global::Mockolate.Mock.IMockSetupForMyBaseClass>? setup, object?[]? constructorParameters)");
	}

	[Fact]
	public async Task ForTypesWithoutExplicitConstructor_ShouldOmitConstructorParametersOverloads()
	{
		GeneratorResult result = Generator
			.Run("""
			     using Mockolate;

			     namespace MyCode;

			     public class Program
			     {
			         public static void Main(string[] args)
			         {
			     		_ = MyBaseClass.CreateMock();
			         }
			     }

			     public class MyBaseClass
			     {
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.MyBaseClass.g.cs").WhoseValue
			.DoesNotContain("CreateMock(object?[] constructorParameters)").And
			.DoesNotContain("object?[] constructorParameters)").And
			.Contains(
				"private static global::MyCode.MyBaseClass CreateMock(global::Mockolate.MockBehavior? mockBehavior, global::System.Action<global::Mockolate.Mock.IMockSetupForMyBaseClass>? setup, object?[]? constructorParameters)");
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
			     		_ = MyBaseClass.CreateMock();
			         }
			     }

			     public class MyBaseClass
			     {
			         private MyBaseClass() { }
			     }
			     """);

		await That(result.Sources).DoesNotContainKey("Mock.MyBaseClass.g.cs");
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
			     		_ = MyClassWithSealedEvents.CreateMock();
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

		await That(result.Sources).ContainsKey("Mock.MyClassWithSealedEvents.g.cs").WhoseValue
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
			     		_ = MyClassWithSealedIndexers.CreateMock();
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

		await That(result.Sources).ContainsKey("Mock.MyClassWithSealedIndexers.g.cs").WhoseValue
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
			     		_ = MyClassWithSealedMethods.CreateMock();
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

		await That(result.Sources).ContainsKey("Mock.MyClassWithSealedMethods.g.cs").WhoseValue
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
			     		_ = MyClassWithSealedProperties.CreateMock();
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

		await That(result.Sources).ContainsKey("Mock.MyClassWithSealedProperties.g.cs").WhoseValue
			.DoesNotContain("override int MyProperty");
	}

	[Fact]
	public async Task MembersWithReservedNames_ShouldPrefixAtSymbol()
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
			     		_ = IMyService.CreateMock();
			         }
			     }

			     public interface IMyService
			     {
			         int @class { get; }
			         string @return();
			         void @event(int @params);
			         int @void<@class>(int @ref);
			         string this[int @params, string @void] { get; set; }
			         event EventHandler @event;
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
			.Contains("public int @class").And
			.Contains("public string @return()").And
			.Contains("public void @event(int @params)").And
			.Contains("public int @void<@class>(int @ref)").And
			.Contains("public string this[int @params, string @void]").And
			.Contains("public event global::System.EventHandler @event").And
			.Contains("private global::System.EventHandler? _mockolateEvent_global__MyCode_IMyService_event;").And
			.DoesNotContain("_mockolateEvent_global__MyCode_IMyService_@event");
		;
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
			     		_ = IMyService.CreateMock();
			         }
			     }

			     public interface IMyService
			     {
			         string this[int @true] { get; set; }
			         void DoSomething(int @event);
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
			.Contains("""
			          public string this[int @true]
			          """).And
			.Contains("""
			          public void DoSomething(int @event)
			          """);

		await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
			.Contains("""
			          		global::Mockolate.Setup.IVoidMethodSetupWithCallback<int> global::Mockolate.Mock.IMockSetupForIMyService.DoSomething(global::Mockolate.Parameters.IParameter<int>? @event)
			          		{
			          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int>.WithParameterCollection(MockRegistry, "global::MyCode.IMyService.DoSomething", CovariantParameterAdapter<int>.Wrap(@event ?? global::Mockolate.It.IsNull<int>("null")));
			          			this.MockRegistry.SetupMethod(global::Mockolate.Mock.IMyService.MemberId_DoSomething, methodSetup);
			          			return methodSetup;
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		global::Mockolate.Setup.IndexerSetup<string, int> global::Mockolate.Mock.IMockSetupForIMyService.this[global::Mockolate.Parameters.IParameter<int>? parameter1]
			          		{
			          			get
			          			{
			          				var indexerSetup = new global::Mockolate.Setup.IndexerSetup<string, int>(MockRegistry, CovariantParameterAdapter<int>.Wrap(parameter1 ?? global::Mockolate.It.IsNull<int>("null")));
			          				this.MockRegistry.SetupIndexer(global::Mockolate.Mock.IMyService.MemberId_Indexer_int_Get, indexerSetup);
			          				return indexerSetup;
			          			}
			          		}
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.DoSomething(global::Mockolate.Parameters.IParameter<int>? @event)
			          """).IgnoringNewlineStyle().And
			.Contains("""
			          		global::Mockolate.Verify.VerificationIndexerResult<IMockVerifyForIMyService, string> IMockVerifyForIMyService.this[global::Mockolate.Parameters.IParameter<int>? @true]
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
			     		_ = MyDerivedClass.CreateMock();
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

		await That(result.Sources).ContainsKey("Mock.MyDerivedClass.g.cs").WhoseValue
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
			     		_ = MyDerivedClass.CreateMock();
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

		await That(result.Sources).ContainsKey("Mock.MyDerivedClass.g.cs").WhoseValue
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
			     		_ = MyDerivedClass.CreateMock();
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
		await That(result.Sources).ContainsKey("Mock.MyDerivedClass.g.cs").WhoseValue
			.DoesNotContain("override bool Equals");
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
			     		_ = IMyService.CreateMock();
			         }
			     }

			     public interface IMyService
			     {
			         void MyMethod(object v1, bool v2, string v3, char v4, byte v5, sbyte v6, short v7, ushort v8, int v9, uint v10, long v11, ulong v12, float v13, double v14, decimal v15);
			     }
			     """);

		await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
			.Contains("""
			          		public void MyMethod(object v1, bool v2, string v3, char v4, byte v5, sbyte v6, short v7, ushort v8, int v9, uint v10, long v11, ulong v12, float v13, double v14, decimal v15)
			          		{
			          """)
			.IgnoringNewlineStyle().And
			.Contains(
				"foreach (global::Mockolate.Setup.VoidMethodSetup<object, bool, string, char, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal> s_methodSetup in this.MockRegistry.GetMethodSetups<global::Mockolate.Setup.VoidMethodSetup<object, bool, string, char, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal>>(\"global::MyCode.IMyService.MyMethod\"))")
			.IgnoringNewlineStyle().And
			.Contains("""
			          			bool hasWrappedResult = false;
			          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
			          			{
			          				this.MockolateBuffer_MyMethod.Append("global::MyCode.IMyService.MyMethod", v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
			          			}
			          			try
			          			{
			          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
			          				{
			          					wraps.MyMethod(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
			          					hasWrappedResult = true;
			          				}
			          			}
			          			finally
			          			{
			          				methodSetup?.TriggerCallbacks(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
			          			}
			          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
			          			{
			          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod(object, bool, string, char, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal)' was invoked without prior setup.");
			          			}
			          		}
			          """).IgnoringNewlineStyle();
	}
}
