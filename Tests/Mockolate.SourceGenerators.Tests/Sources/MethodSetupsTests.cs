using System.Threading;

namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed class MethodSetupsTests
{
	[Fact]
	public async Task GenerateMethodSetupsForMethodsWithMoreParameters()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyOutermostInterface>();
			             }
			         }

			         public interface IMyOutermostInterface : IMyOuterInterface;
			         public interface IMyOuterInterface : IMyInterface;
			         public interface IMyInterface
			         {
			             bool MyMethod1(int v1, bool v2, double v3, long v4, CancellationToken v5);
			             void MyMethod2(int v1, bool v2, double v3, long v4, CancellationToken v5);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("MethodSetups.g.cs").WhoseValue
			.Contains(
				"internal class ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5> : MethodSetup")
			.And
			.Contains(
				"internal class VoidMethodSetup<T1, T2, T3, T4, T5> : MethodSetup");
	}

	[Fact]
	public async Task WhenAllMethodsHaveUpTo4Parameters_ShouldNotGenerateMethodSetups()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             Task<int> MyMethod(int v1, bool v2, double v3, long v4);
			             void MyVoidMethod(int v1, bool v2, double v3, long v4);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).DoesNotContainKey("MethodSetups.g.cs");
	}

	[Theory]
	[InlineData("""
	            string ToString();
	            bool Equals(object obj);
	            int GetHashCode();
	            """, "IMockMethodSetupWithToStringWithEqualsWithGetHashCode")]
	[InlineData("""
	            bool Equals(object obj);
	            int GetHashCode();
	            """, "IMockMethodSetupWithEqualsWithGetHashCode")]
	[InlineData("""
	            string ToString();
	            int GetHashCode();
	            """, "IMockMethodSetupWithToStringWithGetHashCode")]
	[InlineData("""
	            string ToString();
	            bool Equals(object obj);
	            """, "IMockMethodSetupWithToStringWithEquals")]
	[InlineData("""
	            string ToString();
	            """, "IMockMethodSetupWithToString")]
	[InlineData("""
	            bool Equals(object obj);
	            """, "IMockMethodSetupWithEquals")]
	[InlineData("""
	            bool Equals(object? obj);
	            """, "IMockMethodSetupWithEquals")]
	[InlineData("""
	            int GetHashCode();
	            """, "IMockMethodSetupWithGetHashCode")]
	public async Task WhenImplementingObjectMethods_ShouldUseSpecialInterfaces(string methods, string expectedType)
	{
		GeneratorResult result = Generator
			.Run($$"""
			       using System;
			       using Mockolate;

			       namespace MyCode
			       {
			           public class Program
			           {
			               public static void Main(string[] args)
			               {
			       			_ = Mock.Create<IMyInterface>();
			               }
			           }

			           public interface IMyInterface
			           {
			               {{methods}}
			           }
			       }
			       """);

		await That(result.Sources).ContainsKey("MockForIMyInterfaceExtensions.g.cs").WhoseValue
			.Contains($"{expectedType}<MyCode.IMyInterface> Method");
	}

	[Fact]
	public async Task WithComplexMethodReturningAValue_ShouldOnlyGenerateNecessaryExtensions()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             int MyMethod(int v1, bool v2, double v3, long v4, string v5, CancellationToken v6);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("MethodSetups.g.cs").WhoseValue
			.Contains("class ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6>").And
			.DoesNotContain("class VoidMethodSetup<T1, T2, T3, T4, T5, T6>").And
			.DoesNotContain("class ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5>").And
			.DoesNotContain("class ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6, T7>");
	}

	[Fact]
	public async Task WithComplexMethodReturningVoid_ShouldOnlyGenerateNecessaryExtensions()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, bool v2, double v3, long v4, string v5, CancellationToken v6);
			         }
			     }
			     """, typeof(DateTime), typeof(Task), typeof(CancellationToken));

		await That(result.Sources).ContainsKey("MethodSetups.g.cs").WhoseValue
			.Contains("class VoidMethodSetup<T1, T2, T3, T4, T5, T6>").And
			.DoesNotContain("class ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5, T6>").And
			.DoesNotContain("class VoidMethodSetup<T1, T2, T3, T4, T5>").And
			.DoesNotContain("class VoidMethodSetup<T1, T2, T3, T4, T5, T6, T7>");
	}
}
