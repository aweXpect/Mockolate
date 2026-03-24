namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		public sealed class EventsTests
		{
			[Fact]
			public async Task MultipleImplementations_ShouldOnlyHaveOneExplicitImplementation()
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
					     		_ = IMyService.CreateMock().Implementing<IMyServiceBase1>().Implementing<IMyServiceBase2>();
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

				await That(result.Sources).ContainsKey("Mock.IMyService__IMyServiceBase1__IMyServiceBase2.g.cs").WhoseValue
					.Contains("public event global::System.EventHandler<string>? SomeEvent").Once().And
					.Contains("event global::System.EventHandler<int>? global::MyCode.IMyServiceBase1.SomeEvent").Once().And
					.Contains("event global::System.EventHandler<long>? global::MyCode.IMyServiceBase2.SomeEvent").Once();
			}

			[Fact]
			public async Task ShouldImplementAllEventsFromInterfaces()
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
					         event EventHandler SomeEvent;
					         event EventHandler? SomeOtherEvent;
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomeEvent" />
					          		public event global::System.EventHandler? SomeEvent
					          		{
					          			add
					          			{
					          				this.MockRegistry.AddEvent("global::MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.SomeEvent += value;
					          				}
					          			}
					          			remove
					          			{
					          				this.MockRegistry.RemoveEvent("global::MyCode.IMyService.SomeEvent", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.SomeEvent -= value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomeOtherEvent" />
					          		public event global::System.EventHandler? SomeOtherEvent
					          		{
					          			add
					          			{
					          				this.MockRegistry.AddEvent("global::MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.SomeOtherEvent += value;
					          				}
					          			}
					          			remove
					          			{
					          				this.MockRegistry.RemoveEvent("global::MyCode.IMyService.SomeOtherEvent", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.SomeOtherEvent -= value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementImplicitlyInheritedEvents()
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

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyDirectEvent" />
					          		public event global::System.EventHandler? MyDirectEvent
					          		{
					          			add
					          			{
					          				this.MockRegistry.AddEvent("global::MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyDirectEvent += value;
					          				}
					          			}
					          			remove
					          			{
					          				this.MockRegistry.RemoveEvent("global::MyCode.IMyService.MyDirectEvent", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyDirectEvent -= value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase1.MyBaseEvent1" />
					          		public event global::System.EventHandler? MyBaseEvent1
					          		{
					          			add
					          			{
					          				this.MockRegistry.AddEvent("global::MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyBaseEvent1 += value;
					          				}
					          			}
					          			remove
					          			{
					          				this.MockRegistry.RemoveEvent("global::MyCode.IMyServiceBase1.MyBaseEvent1", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyBaseEvent1 -= value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase2.MyBaseEvent2" />
					          		public event global::System.EventHandler? MyBaseEvent2
					          		{
					          			add
					          			{
					          				this.MockRegistry.AddEvent("global::MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyBaseEvent2 += value;
					          				}
					          			}
					          			remove
					          			{
					          				this.MockRegistry.RemoveEvent("global::MyCode.IMyServiceBase2.MyBaseEvent2", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyBaseEvent2 -= value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase3.MyBaseEvent3" />
					          		public event global::System.EventHandler? MyBaseEvent3
					          		{
					          			add
					          			{
					          				this.MockRegistry.AddEvent("global::MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyBaseEvent3 += value;
					          				}
					          			}
					          			remove
					          			{
					          				this.MockRegistry.RemoveEvent("global::MyCode.IMyServiceBase3.MyBaseEvent3", value?.Target, value?.Method);
					          				if (this.Wraps is not null)
					          				{
					          					this.Wraps.MyBaseEvent3 -= value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementVirtualEventsOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
					         public virtual event EventHandler SomeEvent;
					         public event EventHandler? SomeOtherEvent;
					         protected virtual event EventHandler SomeProtectedEvent;
					     }

					     public class MyProtectedService
					     {
					         protected virtual event EventHandler SomeProtectedEvent;
					     }

					     public interface IMyOtherService
					     {
					         event EventHandler SomeThirdEvent;
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.MyService__IMyOtherService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.SomeEvent" />
					          		public override event global::System.EventHandler? SomeEvent
					          		{
					          			add => this.MockRegistry.AddEvent("global::MyCode.MyService.SomeEvent", value?.Target, value?.Method);
					          			remove => this.MockRegistry.RemoveEvent("global::MyCode.MyService.SomeEvent", value?.Target, value?.Method);
					          		}
					          """).IgnoringNewlineStyle().And
					.DoesNotContain("SomeOtherEvent").Because("The event is not virtual!").And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyOtherService.SomeThirdEvent" />
					          		event global::System.EventHandler? global::MyCode.IMyOtherService.SomeThirdEvent
					          		{
					          			add => this.MockRegistry.AddEvent("global::MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
					          			remove => this.MockRegistry.RemoveEvent("global::MyCode.IMyOtherService.SomeThirdEvent", value?.Target, value?.Method);
					          		}
					          """).IgnoringNewlineStyle();
			}
		}
	}
}
