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
				          			add => _mock.Raise.AddEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          			remove => _mock.Raise.RemoveEvent("MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeOtherEvent" />
				          		public event System.EventHandler? SomeOtherEvent
				          		{
				          			add => _mock.Raise.AddEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
				          			remove => _mock.Raise.RemoveEvent("MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
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
				          			add => _mock.Raise.AddEvent("MyCode.MyService.SomeEvent", value?.Target, value?.Method);
				          			remove => _mock.Raise.RemoveEvent("MyCode.MyService.SomeEvent", value?.Target, value?.Method);
				          		}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("SomeOtherEvent").Because("The event is not virtual!").And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyOtherService.SomeThirdEvent" />
				          		event System.EventHandler? MyCode.IMyOtherService.SomeThirdEvent
				          		{
				          			add => _mock.Raise.AddEvent("MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
				          			remove => _mock.Raise.RemoveEvent("MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
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
				          				return _mock.GetIndexer<int>(index);
				          			}
				          			set
				          			{
				          				_mock.SetIndexer<int>(value, index);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.this[int, bool]" />
				          		public int this[int index, bool isReadOnly]
				          		{
				          			get
				          			{
				          				return _mock.GetIndexer<int>(index, isReadOnly);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.this[int, string]" />
				          		public int this[int index, string isWriteOnly]
				          		{
				          			set
				          			{
				          				_mock.SetIndexer<int>(value, index, isWriteOnly);
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
				          				return _mock.GetIndexer<int>(index);
				          			}
				          			set
				          			{
				          				_mock.SetIndexer<int>(value, index);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.this[int, bool]" />
				          		protected override int this[int index, bool isReadOnly]
				          		{
				          			get
				          			{
				          				return _mock.GetIndexer<int>(index, isReadOnly);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.this[int, string]" />
				          		protected override int this[int index, string isWriteOnly]
				          		{
				          			set
				          			{
				          				_mock.SetIndexer<int>(value, index, isWriteOnly);
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
				          				return _mock.GetIndexer<int>(someAdditionalIndex);
				          			}
				          			set
				          			{
				          				_mock.SetIndexer<int>(value, someAdditionalIndex);
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
				          			var result = _mock.Execute<bool>("MyCode.IMyService.MyMethod1", index);
				          			return result.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, bool)" />
				          		public void MyMethod2(int index, bool isReadOnly)
				          		{
				          			var result = _mock.Execute("MyCode.IMyService.MyMethod2", index, isReadOnly);
				          		}
				          """).IgnoringNewlineStyle();
		}

		[Theory]
		[InlineData("T")]
		[InlineData("struct")]
		[InlineData("class")]
		[InlineData("notnull")]
		[InlineData("class?")]
		[InlineData("MyCode.IMyInterface")]
		[InlineData("new()")]
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
				             where T : class?, notnull, new()
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
				          			where T : class?, notnull, new()
				          			where U : {{constraint}}
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
				          		/// <inheritdoc cref="MyCode.MyService.MyMethod1(int)" />
				          		public override void MyMethod1(int index)
				          		{
				          			var result = _mock.Execute("MyCode.MyService.MyMethod1", index);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.MyMethod2(int, bool)" />
				          		protected override bool MyMethod2(int index, bool isReadOnly)
				          		{
				          			var result = _mock.Execute<bool>("MyCode.MyService.MyMethod2", index, isReadOnly);
				          			return result.Result;
				          		}
				          """).IgnoringNewlineStyle().And
				.DoesNotContain("MyNonVirtualMethod").Because("The method is not virtual!").And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyOtherService.SomeOtherMethod()" />
				          		int MyCode.IMyOtherService.SomeOtherMethod()
				          		{
				          			var result = _mock.Execute<int>("MyCode.IMyOtherService.SomeOtherMethod");
				          			return result.Result;
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
				          			var result = _mock.Execute("MyCode.IMyService.MyMethod1", index);
				          			index = result.SetRefParameter<int>("index", index);
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.MyMethod2(int, out bool)" />
				          		public bool MyMethod2(int index, out bool isReadOnly)
				          		{
				          			var result = _mock.Execute<bool>("MyCode.IMyService.MyMethod2", index, null);
				          			isReadOnly = result.SetOutParameter<bool>("isReadOnly");
				          			return result.Result;
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
				          				return _mock.Get<int>("MyCode.IMyService.SomeProperty");
				          			}
				          			set
				          			{
				          				_mock.Set("MyCode.IMyService.SomeProperty", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeReadOnlyProperty" />
				          		public bool? SomeReadOnlyProperty
				          		{
				          			get
				          			{
				          				return _mock.Get<bool?>("MyCode.IMyService.SomeReadOnlyProperty");
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.IMyService.SomeWriteOnlyProperty" />
				          		public bool? SomeWriteOnlyProperty
				          		{
				          			set
				          			{
				          				_mock.Set("MyCode.IMyService.SomeWriteOnlyProperty", value);
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
				          				return _mock.Get<int>("MyCode.MyService.SomeProperty1");
				          			}
				          			set
				          			{
				          				_mock.Set("MyCode.MyService.SomeProperty1", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeProperty2" />
				          		public override int SomeProperty2
				          		{
				          			get
				          			{
				          				return _mock.Get<int>("MyCode.MyService.SomeProperty2");
				          			}
				          			protected set
				          			{
				          				_mock.Set("MyCode.MyService.SomeProperty2", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeReadOnlyProperty" />
				          		protected override bool? SomeReadOnlyProperty
				          		{
				          			get
				          			{
				          				return _mock.Get<bool?>("MyCode.MyService.SomeReadOnlyProperty");
				          			}
				          		}
				          """).IgnoringNewlineStyle().And
				.Contains("""
				          		/// <inheritdoc cref="MyCode.MyService.SomeWriteOnlyProperty" />
				          		protected override bool? SomeWriteOnlyProperty
				          		{
				          			set
				          			{
				          				_mock.Set("MyCode.MyService.SomeWriteOnlyProperty", value);
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
				          				return _mock.Get<int>("MyCode.IMyOtherService.SomeAdditionalProperty");
				          			}
				          			set
				          			{
				          				_mock.Set("MyCode.IMyOtherService.SomeAdditionalProperty", value);
				          			}
				          		}
				          """).IgnoringNewlineStyle();
		}
	}
}
