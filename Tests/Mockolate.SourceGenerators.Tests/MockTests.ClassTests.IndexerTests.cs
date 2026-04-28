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
			public async Task InterfaceIndexerWithParameterNamedWraps_ShouldRenameWrapsCastVariable()
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
					         int this[int wraps] { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					// The wrap-base pattern-match cast must not collide with the user's `wraps`
					// indexer parameter.
					.DoesNotContain("global::MyCode.IMyService wraps)")
					.IgnoringNewlineStyle().And
					.DoesNotContain("global::MyCode.IMyService wraps ?")
					.IgnoringNewlineStyle().And
					.Contains("global::MyCode.IMyService wraps1")
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
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_int_Get.Append(index);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_int_Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, int> s_setup && s_setup.Matches(index))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				global::Mockolate.Interactions.IndexerGetterAccess<int> access = new(index);
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access, 0)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 0);
					          				}
					          				int baseResult = wraps[index];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult, 0);
					          			}
					          			set
					          			{
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_int_Set.Append(index, value);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_int_Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, int> s_setup && s_setup.Matches(index, value))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, int> access = new(index, value);
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value, 0);
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
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_int_bool__Get.Append(index, isReadOnly);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, bool?>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_int_bool__Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, int, bool?> s_setup && s_setup.Matches(index, isReadOnly))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				global::Mockolate.Interactions.IndexerGetterAccess<int, bool?> access = new(index, isReadOnly);
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, bool?>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access, 1)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 1);
					          				}
					          				int baseResult = wraps[index, isReadOnly];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult, 1);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.this[int, string]" />
					          		public int this[int index, string isWriteOnly]
					          		{
					          			set
					          			{
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_int_string_Set.Append(index, isWriteOnly, value);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, string>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_int_string_Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, int, string> s_setup && s_setup.Matches(index, isWriteOnly, value))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, string, int> access = new(index, isWriteOnly, value);
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, string>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value, 2);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<int> access = new(index);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				if (!(setup?.SkipBaseClass() ?? this.MockRegistry.Behavior.SkipBaseClass))
					          				{
					          					int baseResult = this.MockRegistry.Wraps is global::MyCode.MyService wraps ? wraps[index] : base[index];
					          					return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult, 0);
					          				}
					          				return setup is null
					          					? this.MockRegistry.GetIndexerFallback<int>(access, 0)
					          					: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 0);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, int> access = new(index, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int>>(access);
					          				if (!this.MockRegistry.ApplyIndexerSetter(access, setup, value, 0))
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<int, bool> access = new(index, isReadOnly);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, bool>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, bool>>(access);
					          				if (!(setup?.SkipBaseClass() ?? this.MockRegistry.Behavior.SkipBaseClass))
					          				{
					          					int baseResult = base[index, isReadOnly];
					          					return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult, 1);
					          				}
					          				return setup is null
					          					? this.MockRegistry.GetIndexerFallback<int>(access, 1)
					          					: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 1);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.this[int, string]" />
					          		protected override int this[int index, string isWriteOnly]
					          		{
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<int, string, int> access = new(index, isWriteOnly, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, int, string>>(access);
					          				if (!this.MockRegistry.ApplyIndexerSetter(access, setup, value, 2))
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<string> access = new(someAdditionalIndex);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, string>>(access);
					          				return setup is null
					          					? this.MockRegistry.GetIndexerFallback<int>(access, 3)
					          					: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 3);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<string, int> access = new(someAdditionalIndex, value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockRegistry.RegisterInteraction(access);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, string>? setup = this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, string>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value, 3);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<global::Mockolate.Setup.SpanWrapper<char>> access = new(new global::Mockolate.Setup.SpanWrapper<char>(buffer));
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_global__System_Span_char__Get.Append(new global::Mockolate.Setup.SpanWrapper<char>(buffer));
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_global__System_Span_char__Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>> s_setup && s_setup.Matches(access.Parameter1))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access, 0)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 0);
					          				}
					          				int baseResult = wraps[buffer];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult, 0);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<global::Mockolate.Setup.SpanWrapper<char>, int> access = new(new global::Mockolate.Setup.SpanWrapper<char>(buffer), value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_global__System_Span_char__Set.Append(new global::Mockolate.Setup.SpanWrapper<char>(buffer), value);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_global__System_Span_char__Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>> s_setup && s_setup.Matches(access.Parameter1, access.TypedValue))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.SpanWrapper<char>>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value, 0);
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
					          				global::Mockolate.Interactions.IndexerGetterAccess<global::Mockolate.Setup.ReadOnlySpanWrapper<int>> access = new(new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values));
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_global__System_ReadOnlySpan_int__Get.Append(new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values));
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_global__System_ReadOnlySpan_int__Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>> s_setup && s_setup.Matches(access.Parameter1))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>>(access);
					          				if (this.MockRegistry.Wraps is not global::MyCode.IMyService wraps)
					          				{
					          					return setup is null
					          						? this.MockRegistry.GetIndexerFallback<int>(access, 1)
					          						: this.MockRegistry.ApplyIndexerSetup<int>(access, setup, 1);
					          				}
					          				int baseResult = wraps[values];
					          				return this.MockRegistry.ApplyIndexerGetter(access, setup, baseResult, 1);
					          			}
					          			set
					          			{
					          				global::Mockolate.Interactions.IndexerSetterAccess<global::Mockolate.Setup.ReadOnlySpanWrapper<int>, int> access = new(new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values), value);
					          				if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          				{
					          					this.MockolateBuffer_Indexer_global__System_ReadOnlySpan_int__Set.Append(new global::Mockolate.Setup.ReadOnlySpanWrapper<int>(values), value);
					          				}
					          				global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>? setup = null;
					          				if (string.IsNullOrEmpty(this.MockRegistry.Scenario))
					          				{
					          					global::Mockolate.Setup.IndexerSetup[]? snapshot_setup = this.MockRegistry.GetIndexerSetupSnapshot(global::Mockolate.Mock.IMyService.MemberId_Indexer_global__System_ReadOnlySpan_int__Get);
					          					if (snapshot_setup is not null)
					          					{
					          						for (int i_setup = snapshot_setup.Length - 1; i_setup >= 0; i_setup--)
					          						{
					          							if (snapshot_setup[i_setup] is global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>> s_setup && s_setup.Matches(access.Parameter1, access.TypedValue))
					          							{
					          								setup = s_setup;
					          								break;
					          							}
					          						}
					          					}
					          				}
					          				setup ??= this.MockRegistry.GetIndexerSetup<global::Mockolate.Setup.IndexerSetup<int, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>>(access);
					          				this.MockRegistry.ApplyIndexerSetter(access, setup, value, 1);
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
