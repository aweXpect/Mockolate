namespace Mockolate.SourceGenerators.Tests.Sources;

public sealed partial class ForMockTests
{
	public sealed class ImplementClassTests
	{
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeEvent" />
				          		public event System.EventHandler? SomeEvent
				          		{
				          			add => _mock?.Raise.AddEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          			remove => _mock?.Raise.RemoveEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeOtherEvent" />
				          		public event System.EventHandler? SomeOtherEvent
				          		{
				          			add => _mock?.Raise.AddEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
				          			remove => _mock?.Raise.RemoveEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
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

			await That(result.Sources).ContainsKey("ForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeEvent" />
				          		public override event System.EventHandler? SomeEvent
				          		{
				          			add => (_mock ?? _mockProvider.Value)?.Raise.AddEvent("MyCode.MyService.SomeEvent", value?.Target, value?.Method);
				          			remove => (_mock ?? _mockProvider.Value)?.Raise.RemoveEvent("MyCode.MyService.SomeEvent", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("SomeOtherEvent").Because("The event is not virtual!").And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyOtherService.SomeThirdEvent" />
				          		event System.EventHandler? MyCode.IMyOtherService.SomeThirdEvent
				          		{
				          			add => (_mock ?? _mockProvider.Value)?.Raise.AddEvent("MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
				          			remove => (_mock ?? _mockProvider.Value)?.Raise.RemoveEvent("MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyDirectEvent" />
				          		public event System.EventHandler? MyDirectEvent
				          		{
				          			add => _mock?.Raise.AddEvent("MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
				          			remove => _mock?.Raise.RemoveEvent("MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseEvent1" />
				          		public event System.EventHandler? MyBaseEvent1
				          		{
				          			add => _mock?.Raise.AddEvent("MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
				          			remove => _mock?.Raise.RemoveEvent("MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseEvent2" />
				          		public event System.EventHandler? MyBaseEvent2
				          		{
				          			add => _mock?.Raise.AddEvent("MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
				          			remove => _mock?.Raise.RemoveEvent("MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseEvent3" />
				          		public event System.EventHandler? MyBaseEvent3
				          		{
				          			add => _mock?.Raise.AddEvent("MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
				          			remove => _mock?.Raise.RemoveEvent("MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.this[int]" />
				          		public int this[int index]
				          		{
				          			get
				          			{
				          				return _mock?.GetIndexer<int>(index)
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				_mock?.SetIndexer<int>(value, index);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.this[int, bool]" />
				          		public int this[int index, bool isReadOnly]
				          		{
				          			get
				          			{
				          				return _mock?.GetIndexer<int>(index, isReadOnly)
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.this[int, string]" />
				          		public int this[int index, string isWriteOnly]
				          		{
				          			set
				          			{
				          				_mock?.SetIndexer<int>(value, index, isWriteOnly);
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

			await That(result.Sources).ContainsKey("ForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.this[int]" />
				          		public override int this[int index]
				          		{
				          			get
				          			{
				          				return (_mock ?? _mockProvider.Value)?.GetIndexer<int>(index)
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				(_mock ?? _mockProvider.Value)?.SetIndexer<int>(value, index);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.this[int, bool]" />
				          		protected override int this[int index, bool isReadOnly]
				          		{
				          			get
				          			{
				          				return (_mock ?? _mockProvider.Value)?.GetIndexer<int>(index, isReadOnly)
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.this[int, string]" />
				          		protected override int this[int index, string isWriteOnly]
				          		{
				          			set
				          			{
				          				(_mock ?? _mockProvider.Value)?.SetIndexer<int>(value, index, isWriteOnly);
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
				          				return (_mock ?? _mockProvider.Value)?.GetIndexer<int>(someAdditionalIndex)
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				(_mock ?? _mockProvider.Value)?.SetIndexer<int>(value, someAdditionalIndex);
				          			}
				          		}
				          """).IgnoringNewlineStyle();
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyMethod1(int)" />
				          		public bool MyMethod1(int index)
				          		{
				          			MethodSetupResult<bool>? methodExecution = _mock?.Execute<bool>("MyCode.IMyService.MyMethod1", index);
				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
				          			}

				          			return methodExecution.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, bool)" />
				          		public void MyMethod2(int index, bool isReadOnly)
				          		{
				          			MethodSetupResult? methodExecution = _mock?.Execute("MyCode.IMyService.MyMethod2", index, isReadOnly);
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
		[InlineData("notnull, new()")]
		[InlineData("MyCode.IMyInterface?")]
		[InlineData("allows ref struct")]
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains($$"""
				          		/// <inheritdoc cref="MyCode.IMyService.MyMethod1{T, U}(int)" />
				          		public bool MyMethod1<T, U>(int index)
				          		{
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

			await That(result.Sources).ContainsKey("ForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          		public override void MyMethod1(int index)
				          		{
				          			MethodSetupResult? methodExecution = (_mock ?? _mockProvider.Value)?.Execute("MyCode.MyService.MyMethod1", index);
				          			if (_mock is not null && _mock.Behavior.BaseClassBehavior != BaseClassBehavior.DoNotCallBaseClass)
				          			{
				          				base.MyMethod1(index);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.MyMethod2(int, bool)" />
				          		protected override bool MyMethod2(int index, bool isReadOnly)
				          		{
				          			MethodSetupResult<bool>? methodExecution = (_mock ?? _mockProvider.Value)?.Execute<bool>("MyCode.MyService.MyMethod2", index, isReadOnly);
				          			if (_mock is not null && _mock.Behavior.BaseClassBehavior != BaseClassBehavior.DoNotCallBaseClass)
				          			{
				          				var baseResult = base.MyMethod2(index, isReadOnly);
				          				if (methodExecution?.HasSetup != true && _mock.Behavior.BaseClassBehavior == BaseClassBehavior.UseBaseClassAsDefaultValue)
				          				{
				          					return baseResult;
				          				}
				          			}

				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
				          			}

				          			return methodExecution.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("MyNonVirtualMethod").Because("The method is not virtual!").And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyOtherService.SomeOtherMethod()" />
				          		int MyCode.IMyOtherService.SomeOtherMethod()
				          		{
				          			MethodSetupResult<int>? methodExecution = (_mock ?? _mockProvider.Value)?.Execute<int>("MyCode.IMyOtherService.SomeOtherMethod");
				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyMethod1(ref int)" />
				          		public void MyMethod1(ref int index)
				          		{
				          			MethodSetupResult? methodExecution = _mock?.Execute("MyCode.IMyService.MyMethod1", index);
				          			if (methodExecution is null)
				          			{
				          				index = (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
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
				          			MethodSetupResult<bool>? methodExecution = _mock?.Execute<bool>("MyCode.IMyService.MyMethod2", index, null);
				          			if (methodExecution is null)
				          			{
				          				isReadOnly = (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
				          			}
				          			else
				          			{
				          				isReadOnly = methodExecution.SetOutParameter<bool>("isReadOnly");
				          			}

				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool>();
				          			}

				          			return methodExecution.Result;
				          		}
				          """).IgnoringNewlineStyle();
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeProperty" />
				          		public int SomeProperty
				          		{
				          			get
				          			{
				          				return _mock?.Get<int>("MyCode.IMyService.SomeProperty")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				_mock?.Set("MyCode.IMyService.SomeProperty", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeReadOnlyProperty" />
				          		public bool? SomeReadOnlyProperty
				          		{
				          			get
				          			{
				          				return _mock?.Get<bool?>("MyCode.IMyService.SomeReadOnlyProperty")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool?>();
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeWriteOnlyProperty" />
				          		public bool? SomeWriteOnlyProperty
				          		{
				          			set
				          			{
				          				_mock?.Set("MyCode.IMyService.SomeWriteOnlyProperty", value);
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

			await That(result.Sources).ContainsKey("ForMyService_IMyOtherService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeProperty1" />
				          		public override int SomeProperty1
				          		{
				          			protected get
				          			{
				          				return (_mock ?? _mockProvider.Value)?.Get<int>("MyCode.MyService.SomeProperty1")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				(_mock ?? _mockProvider.Value)?.Set("MyCode.MyService.SomeProperty1", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeProperty2" />
				          		public override int SomeProperty2
				          		{
				          			get
				          			{
				          				return (_mock ?? _mockProvider.Value)?.Get<int>("MyCode.MyService.SomeProperty2")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			protected set
				          			{
				          				(_mock ?? _mockProvider.Value)?.Set("MyCode.MyService.SomeProperty2", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeReadOnlyProperty" />
				          		protected override bool? SomeReadOnlyProperty
				          		{
				          			get
				          			{
				          				return (_mock ?? _mockProvider.Value)?.Get<bool?>("MyCode.MyService.SomeReadOnlyProperty")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<bool?>();
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeWriteOnlyProperty" />
				          		protected override bool? SomeWriteOnlyProperty
				          		{
				          			set
				          			{
				          				(_mock ?? _mockProvider.Value)?.Set("MyCode.MyService.SomeWriteOnlyProperty", value);
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
				          				return (_mock ?? _mockProvider.Value)?.Get<int>("MyCode.IMyOtherService.SomeAdditionalProperty")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				(_mock ?? _mockProvider.Value)?.Set("MyCode.IMyOtherService.SomeAdditionalProperty", value);
				          			}
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyDirectMethod(int)" />
				          		public int MyDirectMethod(int value)
				          		{
				          			MethodSetupResult<int>? methodExecution = _mock?.Execute<int>("MyCode.IMyService.MyDirectMethod", value);
				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}

				          			return methodExecution.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseMethod1(int)" />
				          		public int MyBaseMethod1(int value)
				          		{
				          			MethodSetupResult<int>? methodExecution = _mock?.Execute<int>("MyCode.IMyServiceBase1.MyBaseMethod1", value);
				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          
				          			return methodExecution.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseMethod2(int)" />
				          		public int MyBaseMethod2(int value)
				          		{
				          			MethodSetupResult<int>? methodExecution = _mock?.Execute<int>("MyCode.IMyServiceBase2.MyBaseMethod2", value);
				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          
				          			return methodExecution.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseMethod3(int)" />
				          		public int MyBaseMethod3(int value)
				          		{
				          			MethodSetupResult<int>? methodExecution = _mock?.Execute<int>("MyCode.IMyServiceBase3.MyBaseMethod3", value);
				          			if (methodExecution is null)
				          			{
				          				return (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          
				          			return methodExecution.Result;
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

			await That(result.Sources).ContainsKey("ForIMyService.g.cs").WhoseValue
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyDirectProperty" />
				          		public int MyDirectProperty
				          		{
				          			get
				          			{
				          				return _mock?.Get<int>("MyCode.IMyService.MyDirectProperty")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				_mock?.Set("MyCode.IMyService.MyDirectProperty", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase1.MyBaseProperty1" />
				          		public int MyBaseProperty1
				          		{
				          			get
				          			{
				          				return _mock?.Get<int>("MyCode.IMyServiceBase1.MyBaseProperty1")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				_mock?.Set("MyCode.IMyServiceBase1.MyBaseProperty1", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase2.MyBaseProperty2" />
				          		public int MyBaseProperty2
				          		{
				          			get
				          			{
				          				return _mock?.Get<int>("MyCode.IMyServiceBase2.MyBaseProperty2")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				_mock?.Set("MyCode.IMyServiceBase2.MyBaseProperty2", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyServiceBase3.MyBaseProperty3" />
				          		public int MyBaseProperty3
				          		{
				          			get
				          			{
				          				return _mock?.Get<int>("MyCode.IMyServiceBase3.MyBaseProperty3")
				          					?? (_mock?.Behavior ?? MockBehavior.Default).DefaultValue.Generate<int>();
				          			}
				          			set
				          			{
				          				_mock?.Set("MyCode.IMyServiceBase3.MyBaseProperty3", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle();
		}
	}
}
