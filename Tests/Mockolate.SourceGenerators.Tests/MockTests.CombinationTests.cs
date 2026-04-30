namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed class CombinationTests
	{
		[Fact]
		public async Task AbstractBaseWithParameterlessAndMixedConstructor_ShouldEmitBothTryCastHelpers()
		{
			// A constructor with a required parameter sets useTryCast; a defaulted parameter sets
			// useTryCastWithDefaultValue. Mixing both flips both flags so the extension method ends
			// up with both helper functions and the constructor-arity dispatch 'else if' chain.
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = MyService.CreateMock([42]).Implementing<IMyInterface>();
				         }
				     }

				     public abstract class MyService
				     {
				         protected MyService() { }
				         protected MyService(int required, string text = "hi") { }
				     }

				     public interface IMyInterface
				     {
				         void DoWork();
				     }
				     """);

			await That(result.Diagnostics).IsEmpty();

			await That(result.Sources).ContainsKey("Mock.MyService__IMyInterface.g.cs");
			await That(result.Sources["Mock.MyService__IMyInterface.g.cs"])
				.Contains(
					"static bool TryCast<TValue>(object?[] values, int index, global::Mockolate.MockBehavior behavior, out TValue result)")
				.And
				.Contains(
					"static bool TryCastWithDefaultValue<TValue>(object?[] values, int index, TValue defaultValue, global::Mockolate.MockBehavior behavior, out TValue result)")
				.And
				.Contains(
					"if (mock.MockRegistry.ConstructorParameters is null || mock.MockRegistry.ConstructorParameters.Length == 0)")
				.Because("the parameterless dispatch branch must be emitted").And
				.Contains(
					"else if (mock.MockRegistry.ConstructorParameters.Length >= 1 && mock.MockRegistry.ConstructorParameters.Length <= 2")
				.Because(
					"the 'else if' arity dispatch chain must include the optional-range branch for a mixed-required + defaulted ctor");
		}

		[Fact]
		public async Task AbstractBaseWithRequiredOnlyConstructor_ShouldThrowNoParameterlessConstructorMessage()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = MyService.CreateMock([42]).Implementing<IMyInterface>();
				         }
				     }

				     public abstract class MyService
				     {
				         protected MyService(int value) { }
				     }

				     public interface IMyInterface
				     {
				         void DoWork();
				     }
				     """);

			await That(result.Diagnostics).IsEmpty();

			await That(result.Sources).ContainsKey("Mock.MyService__IMyInterface.g.cs");
			await That(result.Sources["Mock.MyService__IMyInterface.g.cs"])
				.Contains("No parameterless constructor found for 'MyCode.MyService'").And
				.Contains("static bool TryCast<TValue>(object?[] values, int index, global::Mockolate.MockBehavior behavior, out TValue result)").And
				.DoesNotContain("static bool TryCastWithDefaultValue<TValue>");
		}

		[Fact]
		public async Task
			CombinationOnConcreteClassWithStaticAbstractInterface_ShouldPrimeMockRegistryProviderInBaseClassConstructor()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = MyService.CreateMock().Implementing<IStaticAware>();
				         }
				     }

				     public class MyService
				     {
				         public MyService() { }
				     }

				     public interface IStaticAware
				     {
				         static abstract int Counter { get; }
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.MyService__IStaticAware.g.cs");
			await That(result.Sources["Mock.MyService__IStaticAware.g.cs"])
				.Contains("MockRegistryProvider.Value = mockRegistry;")
				.Because(
					"when a concrete base class is combined with an interface declaring static abstract members, the (MockRegistry) constructor must prime the AsyncLocal so virtual calls during base-class construction can resolve the registry");
		}

		[Fact]
		public async Task CombinationWithProtectedEventOnBaseClass_ShouldEmitProtectedRaiseRegion()
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
				     		_ = MyService.CreateMock().Implementing<IExtra>();
				         }
				     }

				     public class MyService
				     {
				         protected virtual event EventHandler? Pinged;
				     }

				     public interface IExtra
				     {
				         void DoWork();
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.MyService__IExtra.g.cs");
			await That(result.Sources["Mock.MyService__IExtra.g.cs"])
				.Contains("IMockProtectedRaiseOnMyService").And
				.Contains("#region IMockProtectedRaiseOnMyService");
		}

		[Fact]
		public async Task CombinationWithRequiredMemberOnBase_ShouldEmitSetsRequiredMembersAttribute()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = MyService.CreateMock().Implementing<IExtra>();
				         }
				     }

				     public class MyService
				     {
				         public required int Value { get; init; }
				         public MyService() { }
				     }

				     public interface IExtra
				     {
				         void DoWork();
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.MyService__IExtra.g.cs");
			await That(result.Sources["Mock.MyService__IExtra.g.cs"])
				.Contains("[global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]")
				.Because(
					"AppendMockSubject_BaseClassConstructor must stamp SetsRequiredMembers on the generated constructor when the base class declares any `required` member");
		}

		[Fact]
		public async Task CombinationWithStaticInterfaceEvents_ShouldEmitStaticRaiseRegion()
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
				     		_ = IBase.CreateMock().Implementing<IStaticEvents>();
				         }
				     }

				     public interface IBase
				     {
				         void DoWork();
				     }

				     public interface IStaticEvents
				     {
				         static abstract event EventHandler Pinged;
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IBase__IStaticEvents.g.cs");
			await That(result.Sources["Mock.IBase__IStaticEvents.g.cs"])
				.Contains("IMockStaticRaiseOnIStaticEvents").And
				.Contains("#region IMockStaticRaiseOnIStaticEvents").And
				.Contains(
					"internal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry> MockRegistryProvider");
		}

		[Fact]
		public async Task CombinationWithStaticInterfaceMembers_ShouldEmitStaticSetupAndVerifyRegions()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = IBase.CreateMock().Implementing<IStaticAware>();
				         }
				     }

				     public interface IBase
				     {
				         void DoWork();
				     }

				     public interface IStaticAware
				     {
				         static abstract int Counter { get; }
				         static abstract void Reset();
				     }
				     """);

			await That(result.Sources).ContainsKey("Mock.IBase__IStaticAware.g.cs");
			await That(result.Sources["Mock.IBase__IStaticAware.g.cs"])
				.Contains("IMockStaticSetupForIStaticAware").And
				.Contains("IMockStaticVerifyForIStaticAware").And
				.Contains("#region IMockStaticSetupForIStaticAware").And
				.Contains("#region IMockStaticVerifyForIStaticAware").And
				.Contains(
					"internal static readonly global::System.Threading.AsyncLocal<global::Mockolate.MockRegistry> MockRegistryProvider")
				.Because("the AsyncLocal field is required so static accessors can find the registry");
		}
	}
}
