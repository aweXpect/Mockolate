namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed partial class ForMockTests
{
	public sealed class ImplementClassTests
	{
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
				          		add => MockRegistrations.AddEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeOtherEvent" />
				          	public event System.EventHandler? SomeOtherEvent
				          	{
				          		add => MockRegistrations.AddEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
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
				          		add => MockRegistrations.AddEvent("MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseEvent1" />
				          	public event System.EventHandler? MyBaseEvent1
				          	{
				          		add => MockRegistrations.AddEvent("MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseEvent2" />
				          	public event System.EventHandler? MyBaseEvent2
				          	{
				          		add => MockRegistrations.AddEvent("MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseEvent3" />
				          	public event System.EventHandler? MyBaseEvent3
				          	{
				          		add => MockRegistrations.AddEvent("MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
				          		remove => MockRegistrations.RemoveEvent("MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Events_ShouldImplementVirtualEventsOfClassesAndAllExplicitelyFromAdditionalInterfaces()
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
				          			return MockRegistrations.GetIndexer<int>(null, index);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetIndexer<int>(value, index);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.this[int, bool]" />
				          	public int this[int index, bool isReadOnly]
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetIndexer<int>(null, index, isReadOnly);
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
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Indexers_ShouldImplementVirtualIndexersOfClassesAndAllExplicitelyFromAdditionalInterfaces()
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
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				var baseResult = base[index];
				          				return MockRegistrations.GetIndexer<int>(() => baseResult, index);
				          			}

				          			return MockRegistrations.GetIndexer<int>(null, index);
				          		}
				          		set
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				base[index] = value;
				          			}

				          			MockRegistrations.SetIndexer<int>(value, index);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.this[int, bool]" />
				          	protected override int this[int index, bool isReadOnly]
				          	{
				          		get
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				var baseResult = base[index, isReadOnly];
				          				return MockRegistrations.GetIndexer<int>(() => baseResult, index, isReadOnly);
				          			}

				          			return MockRegistrations.GetIndexer<int>(null, index, isReadOnly);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.this[int, string]" />
				          	protected override int this[int index, string isWriteOnly]
				          	{
				          		set
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				base[index, isWriteOnly] = value;
				          			}

				          			MockRegistrations.SetIndexer<int>(value, index, isWriteOnly);
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
				          			return MockRegistrations.GetIndexer<int>(null, someAdditionalIndex);
				          		}
				          		set
				          		{
				          			MockRegistrations.SetIndexer<int>(value, someAdditionalIndex);
				          		}
				          	}
				          """).IgnoringNewlineStyle();
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
				          		MethodSetupResult<bool>? methodExecution = MockRegistrations.InvokeMethod<bool>("MyCode.IMyService.MyMethod1", index);
				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<bool>();
				          		}

				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, bool)" />
				          	public void MyMethod2(int index, bool isReadOnly)
				          	{
				          		MethodSetupResult? methodExecution = MockRegistrations.InvokeMethod("MyCode.IMyService.MyMethod2", index, isReadOnly);
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
				          		MethodSetupResult<int>? methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyService.MyDirectMethod", value);
				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<int>();
				          		}

				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseMethod1(int)" />
				          	public int MyBaseMethod1(int value)
				          	{
				          		MethodSetupResult<int>? methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyServiceBase1.MyBaseMethod1", value);
				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<int>();
				          		}

				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseMethod2(int)" />
				          	public int MyBaseMethod2(int value)
				          	{
				          		MethodSetupResult<int>? methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyServiceBase2.MyBaseMethod2", value);
				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<int>();
				          		}

				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseMethod3(int)" />
				          	public int MyBaseMethod3(int value)
				          	{
				          		MethodSetupResult<int>? methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyServiceBase3.MyBaseMethod3", value);
				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<int>();
				          		}

				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task Methods_ShouldImplementVirtualMethodsOfClassesAndAllExplicitelyFromAdditionalInterfaces()
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
				         public virtual void MyMethod1(int index);
				         protected virtual bool MyMethod2(int index, bool isReadOnly);
				         public void MyNonVirtualMethod();
				     }

				     public interface IMyOtherService
				     {
				         int SomeOtherMethod();
				     }
				     """);

			await That(result.Sources).ContainsKey("MockForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          	public override void MyMethod1(int index)
				          	{
				          		MethodSetupResult? methodExecution = MockRegistrations.InvokeMethod("MyCode.MyService.MyMethod1", index);
				          		if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          		{
				          			base.MyMethod1(index);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.MyMethod2(int, bool)" />
				          	protected override bool MyMethod2(int index, bool isReadOnly)
				          	{
				          		MethodSetupResult<bool>? methodExecution = MockRegistrations.InvokeMethod<bool>("MyCode.MyService.MyMethod2", index, isReadOnly);
				          		if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          		{
				          			var baseResult = base.MyMethod2(index, isReadOnly);
				          			if (methodExecution?.HasSetup != true)
				          			{
				          				return baseResult;
				          			}
				          		}

				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<bool>();
				          		}

				          		return methodExecution.Result;
				          	}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("MyNonVirtualMethod").Because("The method is not virtual!").And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyOtherService.SomeOtherMethod()" />
				          	int MyCode.IMyOtherService.SomeOtherMethod()
				          	{
				          		MethodSetupResult<int>? methodExecution = MockRegistrations.InvokeMethod<int>("MyCode.IMyOtherService.SomeOtherMethod");
				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<int>();
				          		}

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
				          		MethodSetupResult? methodExecution = MockRegistrations.InvokeMethod("MyCode.IMyService.MyMethod1", index);
				          		if (methodExecution is null)
				          		{
				          			index = MockRegistrations.Behavior.DefaultValue.Generate<int>();
				          		}
				          		else
				          		{
				          			index = methodExecution.SetRefParameter<int>("index", index);
				          		}

				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, out bool)" />
				          	public bool MyMethod2(int index, out bool isReadOnly)
				          	{
				          		MethodSetupResult<bool>? methodExecution = MockRegistrations.InvokeMethod<bool>("MyCode.IMyService.MyMethod2", index, null);
				          		if (methodExecution is null)
				          		{
				          			isReadOnly = MockRegistrations.Behavior.DefaultValue.Generate<bool>();
				          		}
				          		else
				          		{
				          			isReadOnly = methodExecution.SetOutParameter<bool>("isReadOnly");
				          		}

				          		if (methodExecution is null)
				          		{
				          			return MockRegistrations.Behavior.DefaultValue.Generate<bool>();
				          		}

				          		return methodExecution.Result;
				          	}
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
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyService.SomeProperty");
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyService.SomeProperty", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyService.SomeReadOnlyProperty" />
				          	public bool? SomeReadOnlyProperty
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<bool?>("MyCode.IMyService.SomeReadOnlyProperty");
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
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyService.MyDirectProperty");
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyService.MyDirectProperty", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseProperty1" />
				          	public int MyBaseProperty1
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyServiceBase1.MyBaseProperty1");
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyServiceBase1.MyBaseProperty1", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseProperty2" />
				          	public int MyBaseProperty2
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyServiceBase2.MyBaseProperty2");
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyServiceBase2.MyBaseProperty2", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseProperty3" />
				          	public int MyBaseProperty3
				          	{
				          		get
				          		{
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyServiceBase3.MyBaseProperty3");
				          		}
				          		set
				          		{
				          			MockRegistrations.SetProperty("MyCode.IMyServiceBase3.MyBaseProperty3", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle();
		}

		[Fact]
		public async Task
			Properties_ShouldImplementVirtualPropertiesOfClassesAndAllExplicitelyFromAdditionalInterfaces()
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
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				var baseResult = base.SomeProperty1;
				          				return MockRegistrations.GetProperty<int>("MyCode.MyService.SomeProperty1", () => baseResult);
				          			}

				          			return MockRegistrations.GetProperty<int>("MyCode.MyService.SomeProperty1");
				          		}
				          		set
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				base.SomeProperty1 = value;
				          			}

				          			MockRegistrations.SetProperty("MyCode.MyService.SomeProperty1", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeProperty2" />
				          	public override int SomeProperty2
				          	{
				          		get
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				var baseResult = base.SomeProperty2;
				          				return MockRegistrations.GetProperty<int>("MyCode.MyService.SomeProperty2", () => baseResult);
				          			}

				          			return MockRegistrations.GetProperty<int>("MyCode.MyService.SomeProperty2");
				          		}
				          		protected set
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				base.SomeProperty2 = value;
				          			}

				          			MockRegistrations.SetProperty("MyCode.MyService.SomeProperty2", value);
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeReadOnlyProperty" />
				          	protected override bool? SomeReadOnlyProperty
				          	{
				          		get
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				var baseResult = base.SomeReadOnlyProperty;
				          				return MockRegistrations.GetProperty<bool?>("MyCode.MyService.SomeReadOnlyProperty", () => baseResult);
				          			}

				          			return MockRegistrations.GetProperty<bool?>("MyCode.MyService.SomeReadOnlyProperty");
				          		}
				          	}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          	/// <inheritdoc cref="MyCode.MyService.SomeWriteOnlyProperty" />
				          	protected override bool? SomeWriteOnlyProperty
				          	{
				          		set
				          		{
				          			if (MockRegistrations.Behavior.BaseClassBehavior != BaseClassBehavior.IgnoreBaseClass)
				          			{
				          				base.SomeWriteOnlyProperty = value;
				          			}

				          			MockRegistrations.SetProperty("MyCode.MyService.SomeWriteOnlyProperty", value);
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
				          			return MockRegistrations.GetProperty<int>("MyCode.IMyOtherService.SomeAdditionalProperty");
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
