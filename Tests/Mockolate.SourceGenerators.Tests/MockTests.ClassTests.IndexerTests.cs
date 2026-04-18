namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		public sealed class IndexerTests
		{
			[Fact]
			public async Task InterfaceIndexerWithParameterNamedLikeLocal_ShouldGenerateUniqueLocalVariableName()
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
					         int this[int access, int setup] { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.DoesNotContain("global::Mockolate.Interactions.IndexerGetterAccess<int, int> access = new(\"access\",")
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<int> access = new("index", index);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          				}
					          				int baseResult = wraps[index];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, int> access = new("index", index, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<int, bool?> access = new("index", index, "isReadOnly", isReadOnly);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, bool?>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, bool?>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          				}
					          				int baseResult = wraps[index, isReadOnly];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[int, string]" />
					          		public int this[int index, string isWriteOnly]
					          		{
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, string, int> access = new("index", index, "isWriteOnly", isWriteOnly, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, string>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<int> access = new("index", index);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				if (!(setup?.SkipBaseClass() ?? this.MockRegistry.Behavior.SkipBaseClass))
					          				{
					          					int baseResult = this.MockRegistry.Wraps is global::MyCode.MyService wraps ? wraps[index] : base[index];
					          					return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult);
					          				}
					          				return setup is null
					          					? this.MockRegistry.GetIndexerFallback<int>(access)
					          					: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, int> access = new("index", index, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				if (!this.MockRegistry.ApplyIndexerSetter(access, setup, value))
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<int, bool> access = new("index", index, "isReadOnly", isReadOnly);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, bool>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, bool>>(access);
					          				if (!(setup?.SkipBaseClass() ?? this.MockRegistry.Behavior.SkipBaseClass))
					          				{
					          					int baseResult = base[index, isReadOnly];
					          					return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult);
					          				}
					          				return setup is null
					          					? this.MockRegistry.GetIndexerFallback<int>(access)
					          					: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.this[int, string]" />
					          		protected override int this[int index, string isWriteOnly]
					          		{
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, string, int> access = new("index", index, "isWriteOnly", isWriteOnly, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, string>>(access);
					          				if (!this.MockRegistry.ApplyIndexerSetter(access, setup, value))
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<string> access = new("someAdditionalIndex", someAdditionalIndex);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, string>>(access);
					          				return setup is null
					          					? this.MockRegistry.GetIndexerFallback<int>(access)
					          					: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<string, int> access = new("someAdditionalIndex", someAdditionalIndex, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, string>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<global::Mockolate.Setup.SpanWrapper<char>> access = new("buffer", new global::Mockolate.Setup.SpanWrapper<char>(buffer));
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          				}
					          				int baseResult = wraps[buffer];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<global::Mockolate.Setup.SpanWrapper<char>, int> access = new("buffer", new global::Mockolate.Setup.SpanWrapper<char>(buffer), value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<global::Mockolate.Setup.ReadOnlySpanWrapper<int>> access = new("values", new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values));
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup);
					          				}
					          				int baseResult = wraps[values];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<global::Mockolate.Setup.ReadOnlySpanWrapper<int>, int> access = new("values", new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values), value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value);
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
