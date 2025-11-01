namespace Mockolate.SourceGenerators.Tests;

public class MockGeneratorTests
{
	[Fact]
	public async Task SealedClass_ShouldNotBeIncluded()
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
			     			_ = Mock.Create<MyService>();
			             }
			         }

			         public sealed class MyService { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).IsEqualTo([
				"Mock.g.cs",
				"MockGeneratorAttribute.g.cs",
				"MockRegistration.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenNamesConflict_ShouldAppendAnIndex()
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
			     			_ = Mock.Create<IMyInt>();
			     			_ = Mock.Create<I.MyInt>();
			     			_ = Mock.Create<IMy<int>>();
			             }
			         }

			         public interface IMyInt { }

			         public class I
			         {
			     		public interface MyInt { }
			         }

			         public interface IMy<T> { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).Contains([
				"ForIMyInt.g.cs",
				"ForIMyInt_1.g.cs",
				"ForIMyInt_2.g.cs",
				"ForIMyInt.Extensions.g.cs",
				"ForIMyInt_1.Extensions.g.cs",
				"ForIMyInt_2.Extensions.g.cs",
				"ForIMyInt.SetupExtensions.g.cs",
				"ForIMyInt_1.SetupExtensions.g.cs",
				"ForIMyInt_2.SetupExtensions.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenNamesConflictForAdditionalClasses_ShouldAppendAnIndex()
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
			     			_ = Mock.Create<I, IMyInt, I.MyInt, IMy<int>>();
			             }
			         }

			         public interface IMyInt { }

			         public class I
			         {
			     		public interface MyInt { }
			         }

			         public interface IMy<T> { }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).Contains([
				"ForI_IMyInt_IMyInt_IMyint.g.cs",
				"ForI_IMyInt_IMyInt_IMyint.Extensions.g.cs",
				"ForIMyInt.SetupExtensions.g.cs",
				"ForIMyInt_1.SetupExtensions.g.cs",
				"ForIMyInt_2.SetupExtensions.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenUsingMockCreateFromOtherNamespace_ShouldNotBeIncluded()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading;
			     using System.Threading.Tasks;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			     			_ = Mock.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface { }

			         public class Mock
			         {
			     		public static Mock<T> Create<T>() => new Mock<T>();
			         }

			         public class Mock<T>{ }
			     }
			     """);

		await ThatAll(
			That(result.Sources.Keys).IsEqualTo([
				"Mock.g.cs",
				"MockGeneratorAttribute.g.cs",
				"MockRegistration.g.cs",
			]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

	[Fact]
	public async Task WhenUsingMockFactory_ShouldGenerateMocksAndExtensions()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			                var factory = new Mock.Factory(MockBehavior.Default);
			     			_ = factory.Create<IMyInterface>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).HasCount().AtLeast(5);
		await That(result.Sources).ContainsKey("ForIMyInterface.g.cs");
		await That(result.Sources).ContainsKey("ForIMyInterface.Extensions.g.cs");
		await That(result.Sources).ContainsKey("ForIMyInterface.SetupExtensions.g.cs");
	}

	[Fact]
	public async Task WithCustomGenerator_ShouldWork()
	{
		GeneratorResult result = Generator
			.Run("""
			     using System;
			     using System.Threading.Tasks;
			     using Mockolate;

			     namespace MyCode
			     {
			         public class Program
			         {
			             public static void Main(string[] args)
			             {
			                 MyGenerator.Create<IMyInterface>();
			             }
			         }

			         public static class MyGenerator
			         {
			             [MockGenerator]
			             public static void Create<T>()
			                 where T : class
			             {
			                 _ = Mock.Create<T>();
			             }
			         }

			         public interface IMyInterface
			         {
			             void MyMethod(int v1, bool v2, double v3, long v4, uint v5, string v6, DateTime v7);
			         }
			     }
			     """, typeof(DateTime), typeof(Task));

		await That(result.Diagnostics).IsEmpty();
		await That(result.Sources).HasCount().AtLeast(5);
		await That(result.Sources).ContainsKey("ForIMyInterface.g.cs");
		await That(result.Sources).ContainsKey("ForIMyInterface.Extensions.g.cs");
		await That(result.Sources).ContainsKey("ForIMyInterface.SetupExtensions.g.cs");
	}
}
