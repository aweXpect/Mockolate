namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		public sealed class IndexerTests
		{
			[Fact]
			public async Task InterfaceIndexerWithParameterNamedIndexerResult_ShouldGenerateUniqueLocalVariableName()
			{
				GeneratorResult result = Generator
					.Run("""
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
					         int this[int indexerResult] { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains(
						"return this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue(\"indexerResult\", indexerResult)).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));")
					.IgnoringNewlineStyle().And
					.Contains("this.MockRegistry.SetIndexer<int>(value, new global::Mockolate.Parameters.NamedParameterValue(\"indexerResult\", indexerResult));")
					.IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementAllIndexersFromInterfaces()
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
					         int this[int index] { get; set; }
					         int this[int index, bool? isReadOnly] { get; }
					         int this[int index, string isWriteOnly] { set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[int]" />
					          		public int this[int index]
					          		{
					          			get
					          			{
					          				if (this.Wraps is null)
					          				{
					          					return this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("index", index)).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          				}
					          				var indexerResult = this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("index", index));
					          				var baseResult = this.Wraps[index];
					          				return indexerResult.GetResult(baseResult);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int>(value, new global::Mockolate.Parameters.NamedParameterValue("index", index));
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps[index] = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[int, bool?]" />
					          		public int this[int index, bool? isReadOnly]
					          		{
					          			get
					          			{
					          				if (this.Wraps is null)
					          				{
					          					return this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isReadOnly", isReadOnly)).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          				}
					          				var indexerResult = this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isReadOnly", isReadOnly));
					          				var baseResult = this.Wraps[index, isReadOnly];
					          				return indexerResult.GetResult(baseResult);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[int, string]" />
					          		public int this[int index, string isWriteOnly]
					          		{
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int>(value, new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isWriteOnly", isWriteOnly));
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps[index, isWriteOnly] = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementVirtualIndexersOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
					     		_ = MyService.CreateMock().Implementing<IMyOtherService>();
					     		_ = MyProtectedService.CreateMock();
					         }
					     }

					     public class MyService
					     {
					         public virtual int this[int index] { get; set; }
					         protected virtual int this[int index, bool isReadOnly] { get; }
					         protected virtual int this[int index, string isWriteOnly] { set; }
					         public int this[int index, long isNotVirtual] { get; set; }
					     }

					     public class MyProtectedService
					     {
					         protected virtual int this[int index, bool? isReadOnly] { get; set; }
					     }

					     public interface IMyOtherService
					     {
					         int this[string someAdditionalIndex] { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.MyService__IMyOtherService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.this[int]" />
					          		public override int this[int index]
					          		{
					          			get
					          			{
					          				var indexerResult = this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("index", index));
					          				if (!indexerResult.SkipBaseClass)
					          				{
					          					var baseResult = base[index];
					          					return indexerResult.GetResult(baseResult);
					          				}
					          				return indexerResult.GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          			}
					          			set
					          			{
					          				if (!this.MockRegistry.SetIndexer<int>(value, new global::Mockolate.Parameters.NamedParameterValue("index", index)))
					          				{
					          					base[index] = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.this[int, bool]" />
					          		protected override int this[int index, bool isReadOnly]
					          		{
					          			get
					          			{
					          				var indexerResult = this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isReadOnly", isReadOnly));
					          				if (!indexerResult.SkipBaseClass)
					          				{
					          					var baseResult = base[index, isReadOnly];
					          					return indexerResult.GetResult(baseResult);
					          				}
					          				return indexerResult.GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.this[int, string]" />
					          		protected override int this[int index, string isWriteOnly]
					          		{
					          			set
					          			{
					          				if (!this.MockRegistry.SetIndexer<int>(value, new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isWriteOnly", isWriteOnly)))
					          				{
					          					base[index, isWriteOnly] = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.DoesNotContain("int this[int index, long isNotVirtual]").Because("The indexer is not virtual!").And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyOtherService.this[string]" />
					          		int global::MyCode.IMyOtherService.this[string someAdditionalIndex]
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetIndexer<int>(new global::Mockolate.Parameters.NamedParameterValue("someAdditionalIndex", someAdditionalIndex)).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int>(value, new global::Mockolate.Parameters.NamedParameterValue("someAdditionalIndex", someAdditionalIndex));
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}
		}
	}
}
