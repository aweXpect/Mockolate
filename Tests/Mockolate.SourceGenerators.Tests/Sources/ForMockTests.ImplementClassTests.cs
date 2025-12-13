namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed partial class ForMockTests
{
	public sealed class ImplementClassTests
	{
		[Fact]
		public async Task BaseClassMethodWithParameterNamedBaseResult_ShouldGenerateUniqueLocalVariableName()
		{
			GeneratorResult result = Generator
				.Run("""
				     using Mockolate;

				     namespace MyCode;

				     public class Program
				     {
				         public static void Main(string[] args)
				         {
				     		_ = Mock.Create<MyService>();
				         }
				     }

				     public class MyService
				     {
				         public virtual int ProcessData(int baseResult) => baseResult;
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForMyService.g.cs").WhoseValue
				.Contains("MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>(")
				.IgnoringNewlineStyle().And
				.Contains("if (methodExecution.CallBaseClass)")
				.IgnoringNewlineStyle().And
				.Contains("var baseResult1 = base.ProcessData(baseResult);")
				.IgnoringNewlineStyle().And
				.Contains("return methodExecution.GetResult(baseResult1);")
				.Or
				.Contains("if (!methodExecution.HasSetupResult)")
				.IgnoringNewlineStyle().And
				.Contains("return baseResult1;")
				.IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Events_MultipleImplementations_ShouldOnlyHaveOneExplicitImplementation()
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
				     		_ = Mock.Create<IMyService, IMyServiceBase1, IMyServiceBase2>();
				         }
				     }

				     public interface IMyService : IMyServiceBase1
				     {
				         new event EventHandler<string> SomeEvent;
				     }

				     public interface IMyServiceBase1 : IMyServiceBase2
				     {
				         new event EventHandler<int> SomeEvent;
				     }

				     public interface IMyServiceBase2
				     {
				         event EventHandler<long> SomeEvent;
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService_IMyServiceBase1_IMyServiceBase2.g.cs").WhoseValue
				.Contains("public event System.EventHandler<string>? SomeEvent").Once().And
				.Contains("event System.EventHandler<int>? MyCode.IMyServiceBase1.SomeEvent").Once().And
				.Contains("event System.EventHandler<long>? MyCode.IMyServiceBase2.SomeEvent").Once();
		}

		[Fact]
		public async Task Events_ShouldImplementAllEventsFromInterfaces()
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
				         event EventHandler SomeEvent;
				         event EventHandler? SomeOtherEvent;
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeEvent" />
				          	public event System.EventHandler? SomeEvent
				          	{
				          		add
				          		{
				          			MockRegistrations.AddEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.SomeEvent += value;
				          			}
				          		}
				          		remove
				          		{
				          			MockRegistrations.RemoveEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.SomeEvent -= value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeOtherEvent" />
				          	public event System.EventHandler? SomeOtherEvent
				          	{
				          		add
				          		{
				          			MockRegistrations.AddEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.SomeOtherEvent += value;
				          			}
				          		}
				          		remove
				          		{
				          			MockRegistrations.RemoveEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.SomeOtherEvent -= value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Events_ShouldImplementImplicitlyInheritedEvents()
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

				     public interface IMyService : IMyServiceBase1
				     {
				         event EventHandler MyDirectEvent;
				     }

				     public interface IMyServiceBase1 : IMyServiceBase2
				     {
				         event EventHandler MyBaseEvent1;
				     }

				     public interface IMyServiceBase2 : IMyServiceBase3
				     {
				         event EventHandler MyBaseEvent2;
				     }

				     public interface IMyServiceBase3
				     {
				         event EventHandler MyBaseEvent3;
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyDirectEvent" />
				          	public event System.EventHandler? MyDirectEvent
				          	{
				          		add
				          		{
				          			MockRegistrations.AddEvent("MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyDirectEvent += value;
				          			}
				          		}
				          		remove
				          		{
				          			MockRegistrations.RemoveEvent("MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyDirectEvent -= value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseEvent1" />
				          	public event System.EventHandler? MyBaseEvent1
				          	{
				          		add
				          		{
				          			MockRegistrations.AddEvent("MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseEvent1 += value;
				          			}
				          		}
				          		remove
				          		{
				          			MockRegistrations.RemoveEvent("MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseEvent1 -= value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseEvent2" />
				          	public event System.EventHandler? MyBaseEvent2
				          	{
				          		add
				          		{
				          			MockRegistrations.AddEvent("MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseEvent2 += value;
				          			}
				          		}
				          		remove
				          		{
				          			MockRegistrations.RemoveEvent("MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseEvent2 -= value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseEvent3" />
				          	public event System.EventHandler? MyBaseEvent3
				          	{
				          		add
				          		{
				          			MockRegistrations.AddEvent("MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseEvent3 += value;
				          			}
				          		}
				          		remove
				          		{
				          			MockRegistrations.RemoveEvent("MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseEvent3 -= value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Events_ShouldImplementVirtualEventsOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
				     		_ = Mock.Create<MyService, IMyOtherService>();
				         }
				     }

				     public class MyService
				     {
				         public virtual event EventHandler SomeEvent;
				         public event EventHandler? SomeOtherEvent;
				         protected virtual event EventHandler SomeProtectedEvent;
				     }

				     public interface IMyOtherService
				     {
				         event EventHandler SomeThirdEvent;
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeEvent" />
				          	public override event System.EventHandler? SomeEvent
				          	{
				          		add => MockRegistrations.AddEvent("MyCode.MyService.SomeEvent", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.MyService.SomeEvent", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("SomeOtherEvent").Because("The event is not virtual!").And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyOtherService.SomeThirdEvent" />
				          	event System.EventHandler? MyCode.IMyOtherService.SomeThirdEvent
				          	{
				          		add => MockRegistrations.AddEvent("MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Indexers_ShouldImplementAllIndexersFromInterfaces()
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
				         int this[int index] { get; set; }
				         int this[int index, bool isReadOnly] { get; }
				         int this[int index, string isWriteOnly] { set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.this[int]" />
				          	public int this[int index]
				          	{
				          		get
				          		{
				          			if (this._wrapped is null)
				          			{
				          				return MockRegistrations.GetIndexer<int>(index).GetResult(() => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!));
				          			}
				          			var indexerResult = MockRegistrations.GetIndexer<int>(index);
				          			var baseResult = this._wrapped[index];
				          			return indexerResult.GetResult(baseResult);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetIndexer<int>(value, index);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped[index] = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.this[int, bool]" />
				          	public int this[int index, bool isReadOnly]
				          	{
				          		get
				          		{
				          			if (this._wrapped is null)
				          			{
				          				return MockRegistrations.GetIndexer<int>(index, isReadOnly).GetResult(() => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!));
				          			}
				          			var indexerResult = MockRegistrations.GetIndexer<int>(index, isReadOnly);
				          			var baseResult = this._wrapped[index, isReadOnly];
				          			return indexerResult.GetResult(baseResult);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.this[int, string]" />
				          	public int this[int index, string isWriteOnly]
				          	{
				          		set
				          		{
				          			MockRegistrations.SetIndexer<int>(value, index, isWriteOnly);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped[index, isWriteOnly] = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Indexers_ShouldImplementVirtualIndexersOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
				     		_ = Mock.Create<MyService, IMyOtherService>();
				         }
				     }

				     public class MyService
				     {
				         public virtual int this[int index] { get; set; }
				         protected virtual int this[int index, bool isReadOnly] { get; }
				         protected virtual int this[int index, string isWriteOnly] { set; }
				         public int this[int index, long isNotVirtual] { get; set; }
				     }

				     public interface IMyOtherService
				     {
				         int this[string someAdditionalIndex] { get; set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.this[int]" />
				          	public override int this[int index]
				          	{
				          		get
				          		{
				          			var indexerResult = MockRegistrations.GetIndexer<int>(index);
				          			if (indexerResult.CallBaseClass)
				          			{
				          				var baseResult = base[index];
				          				return indexerResult.GetResult(baseResult);
				          			}
				          			return indexerResult.GetResult(() => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!));
				          		}
				          		set
				          		{
				          			if (MockRegistrations.SetIndexer<int>(value, index))
				          			{
				          				base[index] = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.this[int, bool]" />
				          	protected override int this[int index, bool isReadOnly]
				          	{
				          		get
				          		{
				          			var indexerResult = MockRegistrations.GetIndexer<int>(index, isReadOnly);
				          			if (indexerResult.CallBaseClass)
				          			{
				          				var baseResult = base[index, isReadOnly];
				          				return indexerResult.GetResult(baseResult);
				          			}
				          			return indexerResult.GetResult(() => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!));
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.this[int, string]" />
				          	protected override int this[int index, string isWriteOnly]
				          	{
				          		set
				          		{
				          			if (MockRegistrations.SetIndexer<int>(value, index, isWriteOnly))
				          			{
				          				base[index, isWriteOnly] = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("int this[int index, long isNotVirtual]").Because("The indexer is not virtual!").And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyOtherService.this[string]" />
				          	int MyCode.IMyOtherService.this[string someAdditionalIndex]
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetIndexer<int>(someAdditionalIndex).GetResult(() => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!));
				          		}
				          		set
				          		{
				          			MockRegistrations.SetIndexer<int>(value, someAdditionalIndex);
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

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
				     		_ = Mock.Create<IMyService>();
				         }
				     }

				     public interface IMyService
				     {
				         int this[int indexerResult] { get; set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains(
					"return MockRegistrations.GetIndexer<int>(indexerResult).GetResult(() => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!));")
				.IgnoringNewlineStyle().And
				.Contains("MockRegistrations.SetIndexer<int>(value, indexerResult);")
				.IgnoringNewlineStyle();
		}

		[Fact]
		public async Task InterfaceMethodWithParameterNamedMethodExecution_ShouldGenerateUniqueLocalVariableName()
		{
			GeneratorResult result = Generator
				.Run("""
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
				         int ProcessData(int methodExecution);
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("MethodSetupResult<int> methodExecution1 = MockRegistrations.InvokeMethod<int>(")
				.IgnoringNewlineStyle().And
				.Contains("methodExecution1.TriggerCallbacks(methodExecution)")
				.IgnoringNewlineStyle().And
				.Contains("return methodExecution1.Result;")
				.IgnoringNewlineStyle();
		}

		[Fact]
		public async Task InterfaceMethodWithParameterNamedResult_ShouldGenerateUniqueLocalVariableName()
		{
			GeneratorResult result = Generator
				.Run("""
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
				         int ProcessResult(int result);
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>(")
				.IgnoringNewlineStyle().And
				.Contains("methodExecution.TriggerCallbacks(result)")
				.IgnoringNewlineStyle().And
				.Contains("return methodExecution.Result;")
				.IgnoringNewlineStyle();
		}

		[Theory]
		[InlineData("class, T")]
		[InlineData("struct")]
		[InlineData("class")]
		[InlineData("notnull")]
		[InlineData("class?")]
		[InlineData("MyCode.IMyInterface")]
		[InlineData("new()")]
		[InlineData("MyCode.IMyInterface?")]
		[InlineData("allows ref struct")]
		[InlineData("MyCode.IMyInterface, new()")]
		public async Task Methods_Generic_ShouldApplyAllConstraints(string constraint)
		{
			GeneratorResult result = Generator
				.Run($$"""
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
				           bool MyMethod1<T, U>(int index)
				               where T : notnull, new()
				               where U : {{constraint}};
				           void MyMethod2(int index, bool isReadOnly);
				       }

				       public interface IMyInterface
				       {
				       }

				       public class MyClass<out T>
				       {
				       	T Value { get; set; }
				       }
				       """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains($$"""
				            	/// <inheritdoc cref="MyCode.IMyService.MyMethod1{T, U}(int)" />
				            	public bool MyMethod1<T, U>(int index)
				            		where T : notnull, new()
				            		where U : {{constraint}}
				            """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_Generic_WithoutConstraints_ShouldNotHaveWhereClause()
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
				         bool MyMethod1<T, U>(int index);
				         void MyMethod2(int index, bool isReadOnly);
				     }

				     public interface IMyInterface
				     {
				     }

				     public class MyClass<out T>
				     {
				     	T Value { get; set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod1{T, U}(int)" />
				          	public bool MyMethod1<T, U>(int index)
				          	{
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_MultipleImplementations_ShouldOnlyHaveOneExplicitImplementation()
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
				     		_ = Mock.Create<IMyService, IMyServiceBase1, IMyServiceBase2>();
				         }
				     }

				     public interface IMyService : IMyServiceBase1
				     {
				         new string Value();
				         string Value(string p1);
				     }

				     public interface IMyServiceBase1 : IMyServiceBase2
				     {
				         new int Value();
				         int Value(int p1);
				     }

				     public interface IMyServiceBase2
				     {
				         long Value();
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService_IMyServiceBase1_IMyServiceBase2.g.cs").WhoseValue
				.Contains("public string Value()").Once().And
				.Contains("public string Value(string p1)").Once().And
				.Contains("int MyCode.IMyServiceBase1.Value()").Once().And
				.Contains("public int Value(int p1)").Once().And
				.Contains("long MyCode.IMyServiceBase2.Value()").Once();
		}

		[Fact]
		public async Task Methods_ShouldImplementAllMethodsFromInterfaces()
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
				         bool MyMethod1(int index);
				         void MyMethod2(int index, bool isReadOnly);
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod1(int)" />
				          	public bool MyMethod1(int index)
				          	{
				          		MethodSetupResult<bool> methodExecution = MockRegistrations.InvokeMethod<bool>("MyCode.IMyService.MyMethod1", p => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!, p), index);
				          		if (this._wrapped is not null)
				          		{
				          			var baseResult = this._wrapped.MyMethod1(index);
				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(index);
				          				return baseResult;
				          			}
				          		}
				          		methodExecution.TriggerCallbacks(index);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, bool)" />
				          	public void MyMethod2(int index, bool isReadOnly)
				          	{
				          		MethodSetupResult methodExecution = MockRegistrations.InvokeMethod("MyCode.IMyService.MyMethod2", index, isReadOnly);
				          		if (this._wrapped is not null)
				          		{
				          			this._wrapped.MyMethod2(index, isReadOnly);
				          		}
				          		methodExecution.TriggerCallbacks(index, isReadOnly);
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_ShouldImplementImplicitlyInheritedMethods()
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

				     public interface IMyService : IMyServiceBase1
				     {
				         int MyDirectMethod(int value);
				     }

				     public interface IMyServiceBase1 : IMyServiceBase2
				     {
				         int MyBaseMethod1(int value);
				     }

				     public interface IMyServiceBase2 : IMyServiceBase3
				     {
				         int MyBaseMethod2(int value);
				     }

				     public interface IMyServiceBase3
				     {
				         int MyBaseMethod3(int value);
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyDirectMethod(int)" />
				          	public int MyDirectMethod(int value)
				          	{
				          		MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyService.MyDirectMethod", p => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!, p), value);
				          		if (this._wrapped is not null)
				          		{
				          			var baseResult = this._wrapped.MyDirectMethod(value);
				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(value);
				          				return baseResult;
				          			}
				          		}
				          		methodExecution.TriggerCallbacks(value);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseMethod1(int)" />
				          	public int MyBaseMethod1(int value)
				          	{
				          		MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyServiceBase1.MyBaseMethod1", p => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!, p), value);
				          		if (this._wrapped is not null)
				          		{
				          			var baseResult = this._wrapped.MyBaseMethod1(value);
				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(value);
				          				return baseResult;
				          			}
				          		}
				          		methodExecution.TriggerCallbacks(value);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseMethod2(int)" />
				          	public int MyBaseMethod2(int value)
				          	{
				          		MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyServiceBase2.MyBaseMethod2", p => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!, p), value);
				          		if (this._wrapped is not null)
				          		{
				          			var baseResult = this._wrapped.MyBaseMethod2(value);
				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(value);
				          				return baseResult;
				          			}
				          		}
				          		methodExecution.TriggerCallbacks(value);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseMethod3(int)" />
				          	public int MyBaseMethod3(int value)
				          	{
				          		MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyServiceBase3.MyBaseMethod3", p => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!, p), value);
				          		if (this._wrapped is not null)
				          		{
				          			var baseResult = this._wrapped.MyBaseMethod3(value);
				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(value);
				          				return baseResult;
				          			}
				          		}
				          		methodExecution.TriggerCallbacks(value);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_ShouldImplementVirtualMethodsOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
				     		_ = Mock.Create<MyService, IMyOtherService>();
				         }
				     }

				     public class MyService
				     {
				         public virtual void MyMethod1(int index, ref int value1, out bool flag)
				         {
				             flag = true;
				         }
				         protected virtual bool MyMethod2(int index, bool isReadOnly, ref int value1, out bool flag)
				         {
				             flag = true;
				         }
				         public void MyNonVirtualMethod();
				     }

				     public interface IMyOtherService
				     {
				         int SomeOtherMethod();
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.MyMethod1(int, ref int, out bool)" />
				          	public override void MyMethod1(int index, ref int value1, out bool flag)
				          	{
				          		MethodSetupResult methodExecution = MockRegistrations.InvokeMethod("MyCode.MyService.MyMethod1", index, value1, null);
				          		if (methodExecution.CallBaseClass)
				          		{
				          			base.MyMethod1(index, ref value1, out flag);
				          		}
				          		methodExecution.TriggerCallbacks(index, value1, flag);

				          		value1 = methodExecution.SetRefParameter<int>("value1", value1);
				          		methodExecution.TriggerCallbacks(index, value1, flag);

				          		flag = methodExecution.SetOutParameter<bool>("flag", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!));
				          		methodExecution.TriggerCallbacks(index, value1, flag);
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.MyMethod2(int, bool, ref int, out bool)" />
				          	protected override bool MyMethod2(int index, bool isReadOnly, ref int value1, out bool flag)
				          	{
				          		MethodSetupResult<bool> methodExecution = MockRegistrations.InvokeMethod<bool>("MyCode.MyService.MyMethod2", p => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!, p), index, isReadOnly, value1, null);
				          		if (methodExecution.CallBaseClass)
				          		{
				          			var baseResult = base.MyMethod2(index, isReadOnly, ref value1, out flag);
				          			if (methodExecution.HasSetupResult == true)
				          			{
				          				value1 = methodExecution.SetRefParameter<int>("value1", value1);
				          			}

				          			if (methodExecution.HasSetupResult == true)
				          			{
				          				flag = methodExecution.SetOutParameter<bool>("flag", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!));
				          			}

				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(index, isReadOnly, value1, flag);
				          				return baseResult;
				          			}
				          		}
				          		else
				          		{
				          			value1 = methodExecution.SetRefParameter<int>("value1", value1);
				          			flag = methodExecution.SetOutParameter<bool>("flag", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!));
				          		}

				          		methodExecution.TriggerCallbacks(index, isReadOnly, value1, flag);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("MyNonVirtualMethod").Because("The method is not virtual!").And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyOtherService.SomeOtherMethod()" />
				          	int MyCode.IMyOtherService.SomeOtherMethod()
				          	{
				          		MethodSetupResult<int> methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyOtherService.SomeOtherMethod", p => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!, p));
				          		methodExecution.TriggerCallbacks();
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_ShouldSupportRefAndOutParameters()
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
				         void MyMethod1(ref int index);
				         bool MyMethod2(int index, out bool isReadOnly);
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod1(ref int)" />
				          	public void MyMethod1(ref int index)
				          	{
				          		MethodSetupResult methodExecution = MockRegistrations.InvokeMethod("MyCode.IMyService.MyMethod1", index);
				          		if (this._wrapped is not null)
				          		{
				          			this._wrapped.MyMethod1(ref index);
				          			if (methodExecution.HasSetupResult == true)
				          			{
				          				index = methodExecution.SetRefParameter<int>("index", index);
				          			}

				          		}
				          		index = methodExecution.SetRefParameter<int>("index", index);
				          		methodExecution.TriggerCallbacks(index);
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, out bool)" />
				          	public bool MyMethod2(int index, out bool isReadOnly)
				          	{
				          		MethodSetupResult<bool> methodExecution = MockRegistrations.InvokeMethod<bool>("MyCode.IMyService.MyMethod2", p => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!, p), index, null);
				          		if (this._wrapped is not null)
				          		{
				          			var baseResult = this._wrapped.MyMethod2(index, out isReadOnly);
				          			if (methodExecution.HasSetupResult == true)
				          			{
				          				isReadOnly = methodExecution.SetOutParameter<bool>("isReadOnly", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!));
				          			}

				          			if (!methodExecution.HasSetupResult)
				          			{
				          				methodExecution.TriggerCallbacks(index, isReadOnly);
				          				return baseResult;
				          			}
				          		}
				          		isReadOnly = methodExecution.SetOutParameter<bool>("isReadOnly", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool)!));
				          		methodExecution.TriggerCallbacks(index, isReadOnly);
				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_ShouldSupportSpanParameters()
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
				         void MyMethod1(Span<char> buffer);
				         bool MyMethod2(ReadOnlySpan<int> values);
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyServiceExtensions.g.cs").WhoseValue
				.Contains("""
				          		public IVoidMethodSetup<SpanWrapper<char>> MyMethod1(ISpanParameter<char> buffer)
				          		{
				          			var methodSetup = new VoidMethodSetup<SpanWrapper<char>>("MyCode.IMyService.MyMethod1", new NamedParameter("buffer", (IParameter)(buffer)));
				          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		public IReturnMethodSetup<bool, ReadOnlySpanWrapper<int>> MyMethod2(IReadOnlySpanParameter<int> values)
				          		{
				          			var methodSetup = new ReturnMethodSetup<bool, ReadOnlySpanWrapper<int>>("MyCode.IMyService.MyMethod2", new NamedParameter("values", (IParameter)(values)));
				          			CastToMockRegistrationOrThrow(setup).SetupMethod(methodSetup);
				          			return methodSetup;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		public VerificationResult<MyCode.IMyService> MyMethod1(IVerifySpanParameter<char> buffer)
				          			=> CastToMockOrThrow(verifyInvoked).Method("MyCode.IMyService.MyMethod1", new NamedParameter("buffer", (IParameter)(buffer)));
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		public VerificationResult<MyCode.IMyService> MyMethod2(IVerifyReadOnlySpanParameter<int> values)
				          			=> CastToMockOrThrow(verifyInvoked).Method("MyCode.IMyService.MyMethod2", new NamedParameter("values", (IParameter)(values)));
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Properties_MultipleImplementations_ShouldOnlyHaveOneExplicitImplementation()
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
				     		_ = Mock.Create<IMyService, IMyServiceBase1, IMyServiceBase2>();
				         }
				     }

				     public interface IMyService : IMyServiceBase1
				     {
				         new string Value { get; set; }
				     }

				     public interface IMyServiceBase1 : IMyServiceBase2
				     {
				         new int Value { get; set; }
				     }

				     public interface IMyServiceBase2
				     {
				         long Value { get; set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService_IMyServiceBase1_IMyServiceBase2.g.cs").WhoseValue
				.Contains("public string Value").Once().And
				.Contains("int MyCode.IMyServiceBase1.Value").Once().And
				.Contains("long MyCode.IMyServiceBase2.Value").Once();
		}

		[Fact]
		public async Task Properties_ShouldImplementAllPropertiesFromInterfaces()
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
				         int SomeProperty { get; set; }
				         bool? SomeReadOnlyProperty { get; }
				         bool? SomeWriteOnlyProperty { set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeProperty" />
				          	public int SomeProperty
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyService.SomeProperty", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), this._wrapped is null ? null : () => this._wrapped.SomeProperty);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyService.SomeProperty", value);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.SomeProperty = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeReadOnlyProperty" />
				          	public bool? SomeReadOnlyProperty
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<bool?>("MyCode.IMyService.SomeReadOnlyProperty", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool?)!), this._wrapped is null ? null : () => this._wrapped.SomeReadOnlyProperty);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeWriteOnlyProperty" />
				          	public bool? SomeWriteOnlyProperty
				          	{
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyService.SomeWriteOnlyProperty", value);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.SomeWriteOnlyProperty = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Properties_ShouldImplementImplicitlyInheritedProperties()
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

				     public interface IMyService : IMyServiceBase1
				     {
				         int MyDirectProperty { get; set; }
				     }

				     public interface IMyServiceBase1 : IMyServiceBase2
				     {
				         int MyBaseProperty1 { get; set; }
				     }

				     public interface IMyServiceBase2 : IMyServiceBase3
				     {
				         int MyBaseProperty2 { get; set; }
				     }

				     public interface IMyServiceBase3
				     {
				         int MyBaseProperty3 { get; set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForIMyService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyDirectProperty" />
				          	public int MyDirectProperty
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyService.MyDirectProperty", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), this._wrapped is null ? null : () => this._wrapped.MyDirectProperty);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyService.MyDirectProperty", value);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyDirectProperty = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseProperty1" />
				          	public int MyBaseProperty1
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyServiceBase1.MyBaseProperty1", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), this._wrapped is null ? null : () => this._wrapped.MyBaseProperty1);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyServiceBase1.MyBaseProperty1", value);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseProperty1 = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseProperty2" />
				          	public int MyBaseProperty2
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyServiceBase2.MyBaseProperty2", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), this._wrapped is null ? null : () => this._wrapped.MyBaseProperty2);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyServiceBase2.MyBaseProperty2", value);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseProperty2 = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseProperty3" />
				          	public int MyBaseProperty3
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyServiceBase3.MyBaseProperty3", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), this._wrapped is null ? null : () => this._wrapped.MyBaseProperty3);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyServiceBase3.MyBaseProperty3", value);
				          			if (this._wrapped is not null)
				          			{
				          				this._wrapped.MyBaseProperty3 = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task
			Properties_ShouldImplementVirtualPropertiesOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
				     		_ = Mock.Create<MyService, IMyOtherService>();
				         }
				     }

				     public class MyService
				     {
				         public virtual int SomeProperty1 { protected get; set; }
				         public virtual int SomeProperty2 { get; protected set; }
				         protected virtual bool? SomeReadOnlyProperty { get; }
				         protected virtual bool? SomeWriteOnlyProperty { set; }
				         public bool? SomeNonVirtualProperty { get; set; }
				     }

				     public interface IMyOtherService
				     {
				         int SomeAdditionalProperty { get; set; }
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeProperty1" />
				          	public override int SomeProperty1
				          	{
				          		protected get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.MyService.SomeProperty1", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), () => base.SomeProperty1);
				          		}
				          		set
				          		{
				          			if (MockRegistrations.SetProperty("MyCode.MyService.SomeProperty1", value))
				          			{
				          				base.SomeProperty1 = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeProperty2" />
				          	public override int SomeProperty2
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.MyService.SomeProperty2", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), () => base.SomeProperty2);
				          		}
				          		protected set
				          		{
				          			if (MockRegistrations.SetProperty("MyCode.MyService.SomeProperty2", value))
				          			{
				          				base.SomeProperty2 = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeReadOnlyProperty" />
				          	protected override bool? SomeReadOnlyProperty
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<bool?>("MyCode.MyService.SomeReadOnlyProperty", () => MockRegistrations.Behavior.DefaultValue.Generate(default(bool?)!), () => base.SomeReadOnlyProperty);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeWriteOnlyProperty" />
				          	protected override bool? SomeWriteOnlyProperty
				          	{
				          		set
				          		{
				          			if (MockRegistrations.SetProperty("MyCode.MyService.SomeWriteOnlyProperty", value))
				          			{
				          				base.SomeWriteOnlyProperty = value;
				          			}
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("SomeNonVirtualProperty").Because("The property is not virtual!").And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyOtherService.SomeAdditionalProperty" />
				          	int MyCode.IMyOtherService.SomeAdditionalProperty
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyOtherService.SomeAdditionalProperty", () => MockRegistrations.Behavior.DefaultValue.Generate(default(int)!), null);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyOtherService.SomeAdditionalProperty", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}
	}
}
