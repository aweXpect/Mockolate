using System.Threading;

namespace Mockolate.SourceGenerators.Tests;

public class MockGeneratorTests
{
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
			     			var m1 = Mock.Create<IMyInt>();
			     			var m2 = Mock.Create<I.MyInt>();
			     			var m3 = Mock.Create<IMy<int>>();
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
			     			var m1 = Mock.Create<I, IMyInt, I.MyInt, IMy<int>>();
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
			     			var m1 = Mock.Create<IMyInterface>();
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
				"MockRegistration.g.cs",
				]).InAnyOrder().IgnoringCase(),
			That(result.Diagnostics).IsEmpty()
		);
	}

}
