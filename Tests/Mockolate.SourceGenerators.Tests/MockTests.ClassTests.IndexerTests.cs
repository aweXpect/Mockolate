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
						"return this.MockRegistry.GetIndexer<int, int>(\"indexerResult\", indexerResult).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));")
					.IgnoringNewlineStyle().And
					.Contains("this.MockRegistry.SetIndexer<int, int>(value, \"indexerResult\", indexerResult);")
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
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return this.MockRegistry.GetIndexer<int, int>("index", index).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          				}
					          				var indexerResult = this.MockRegistry.GetIndexer<int, int>("index", index);
					          				var baseResult = wraps[index];
					          				return indexerResult.GetResult(baseResult);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int, int>(value, "index", index);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps[index] = value;
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
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return this.MockRegistry.GetIndexer<int, int, bool?>("index", index, "isReadOnly", isReadOnly).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          				}
					          				var indexerResult = this.MockRegistry.GetIndexer<int, int, bool?>("index", index, "isReadOnly", isReadOnly);
					          				var baseResult = wraps[index, isReadOnly];
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
					          				this.MockRegistry.SetIndexer<int, int, string>(value, "index", index, "isWriteOnly", isWriteOnly);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps[index, isWriteOnly] = value;
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
					          				var indexerResult = this.MockRegistry.GetIndexer<int, int>("index", index);
					          				if (!indexerResult.SkipBaseClass)
					          				{
					          					var baseResult = this.MockRegistry.Wraps is global::MyCode.MyService wraps ? wraps[index] : base[index];
					          					return indexerResult.GetResult(baseResult);
					          				}
					          				return indexerResult.GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          			}
					          			set
					          			{
					          				if (!this.MockRegistry.SetIndexer<int, int>(value, "index", index))
					          				{
					          					if (this.MockRegistry.Wraps is global::MyCode.MyService wraps)
					          					{
					          						wraps[index] = value;
					          					}
					          					else
					          					{
					          						base[index] = value;
					          					}
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
					          				var indexerResult = this.MockRegistry.GetIndexer<int, int, bool>("index", index, "isReadOnly", isReadOnly);
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
					          				if (!this.MockRegistry.SetIndexer<int, int, string>(value, "index", index, "isWriteOnly", isWriteOnly))
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
					          				return this.MockRegistry.GetIndexer<int, string>("someAdditionalIndex", someAdditionalIndex).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int, string>(value, "someAdditionalIndex", someAdditionalIndex);
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldSupportSpanAndReadOnlySpanIndexerParameters()
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
					         int this[Span<char> buffer] { get; set; }
					         int this[ReadOnlySpan<int> values] { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[global::System.Span{char}]" />
					          		public int this[global::System.Span<char> buffer]
					          		{
					          			get
					          			{
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return this.MockRegistry.GetIndexer<int, global::Mockolate.Setup.SpanWrapper<char>>("buffer", new global::Mockolate.Setup.SpanWrapper<char>(buffer)).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          				}
					          				var indexerResult = this.MockRegistry.GetIndexer<int, global::Mockolate.Setup.SpanWrapper<char>>("buffer", new global::Mockolate.Setup.SpanWrapper<char>(buffer));
					          				var baseResult = wraps[buffer];
					          				return indexerResult.GetResult(baseResult);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int, global::Mockolate.Setup.SpanWrapper<char>>(value, "buffer", new global::Mockolate.Setup.SpanWrapper<char>(buffer));
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps[buffer] = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[global::System.ReadOnlySpan{int}]" />
					          		public int this[global::System.ReadOnlySpan<int> values]
					          		{
					          			get
					          			{
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return this.MockRegistry.GetIndexer<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>("values", new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values)).GetResult(() => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!));
					          				}
					          				var indexerResult = this.MockRegistry.GetIndexer<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>("values", new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values));
					          				var baseResult = wraps[values];
					          				return indexerResult.GetResult(baseResult);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetIndexer<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>(value, "values", new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values));
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps[values] = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}
		}
	}
}
