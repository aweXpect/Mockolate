namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		public sealed class PropertiesTests
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

				await That(result.Sources).ContainsKey("Mock.IMyService__IMyServiceBase1__IMyServiceBase2.g.cs").WhoseValue
					.Contains("public string Value").Once().And
					.Contains("int global::MyCode.IMyServiceBase1.Value").Once().And
					.Contains("long global::MyCode.IMyServiceBase2.Value").Once();
			}

			[Fact]
			public async Task ShouldImplementAllPropertiesFromInterfaces()
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
					         int SomeProperty { get; set; }
					         bool? SomeReadOnlyProperty { get; }
					         bool? SomeWriteOnlyProperty { set; }
					         internal int SomeInternalProperty { get; set; }
					         private int SomePrivateProperty { get; set; }
					         private protected int SomePrivateProtectedProperty { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomeProperty" />
					          		public int SomeProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyService.SomeProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.SomeProperty);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyService.SomeProperty", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.SomeProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomeReadOnlyProperty" />
					          		public bool? SomeReadOnlyProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<bool?>("global::MyCode.IMyService.SomeReadOnlyProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(bool?)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.SomeReadOnlyProperty);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomeWriteOnlyProperty" />
					          		public bool? SomeWriteOnlyProperty
					          		{
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyService.SomeWriteOnlyProperty", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.SomeWriteOnlyProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomeInternalProperty" />
					          		internal int SomeInternalProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyService.SomeInternalProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.SomeInternalProperty);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyService.SomeInternalProperty", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.SomeInternalProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomePrivateProperty" />
					          		private int SomePrivateProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyService.SomePrivateProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.SomePrivateProperty);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyService.SomePrivateProperty", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.SomePrivateProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.SomePrivateProtectedProperty" />
					          		private protected int SomePrivateProtectedProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyService.SomePrivateProtectedProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.SomePrivateProtectedProperty);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyService.SomePrivateProtectedProperty", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.SomePrivateProtectedProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementImplicitlyInheritedProperties()
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

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyDirectProperty" />
					          		public int MyDirectProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyService.MyDirectProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.MyDirectProperty);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyService.MyDirectProperty", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyDirectProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase1.MyBaseProperty1" />
					          		public int MyBaseProperty1
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyServiceBase1.MyBaseProperty1", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.MyBaseProperty1);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyServiceBase1.MyBaseProperty1", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyBaseProperty1 = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase2.MyBaseProperty2" />
					          		public int MyBaseProperty2
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyServiceBase2.MyBaseProperty2", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.MyBaseProperty2);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyServiceBase2.MyBaseProperty2", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyBaseProperty2 = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase3.MyBaseProperty3" />
					          		public int MyBaseProperty3
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyServiceBase3.MyBaseProperty3", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is not global::MyCode.IMyService wraps ? null : () => wraps.MyBaseProperty3);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyServiceBase3.MyBaseProperty3", value);
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyBaseProperty3 = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task
				ShouldImplementVirtualPropertiesOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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
					         public virtual int SomeProperty1 { protected get; set; }
					         public virtual int SomeProperty2 { get; protected set; }
					         protected virtual bool? SomeReadOnlyProperty { get; }
					         protected virtual bool? SomeWriteOnlyProperty { set; }
					         public bool? SomeNonVirtualProperty { get; set; }
					     }

					     public class MyProtectedService
					     {
					         protected virtual bool? SomeProperty { get; set; }
					     }

					     public interface IMyOtherService
					     {
					         int SomeAdditionalProperty { get; set; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.MyService__IMyOtherService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.SomeProperty1" />
					          		public override int SomeProperty1
					          		{
					          			protected get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.MyService.SomeProperty1", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), () => base.SomeProperty1);
					          			}
					          			set
					          			{
					          				if (!this.MockRegistry.SetProperty("global::MyCode.MyService.SomeProperty1", value))
					          				{
					          					if (this.MockRegistry.Wraps is global::MyCode.MyService wraps)
					          					{
					          						wraps.SomeProperty1 = value;
					          					}
					          					else
					          					{
					          						base.SomeProperty1 = value;
					          					}
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.SomeProperty2" />
					          		public override int SomeProperty2
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.MyService.SomeProperty2", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), this.MockRegistry.Wraps is global::MyCode.MyService wraps ? () => wraps.SomeProperty2 : () => base.SomeProperty2);
					          			}
					          			protected set
					          			{
					          				if (!this.MockRegistry.SetProperty("global::MyCode.MyService.SomeProperty2", value))
					          				{
					          					base.SomeProperty2 = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.SomeReadOnlyProperty" />
					          		protected override bool? SomeReadOnlyProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<bool?>("global::MyCode.MyService.SomeReadOnlyProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(bool?)!), () => base.SomeReadOnlyProperty);
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.SomeWriteOnlyProperty" />
					          		protected override bool? SomeWriteOnlyProperty
					          		{
					          			set
					          			{
					          				if (!this.MockRegistry.SetProperty("global::MyCode.MyService.SomeWriteOnlyProperty", value))
					          				{
					          					base.SomeWriteOnlyProperty = value;
					          				}
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.DoesNotContain("SomeNonVirtualProperty").Because("The property is not virtual!").And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyOtherService.SomeAdditionalProperty" />
					          		int global::MyCode.IMyOtherService.SomeAdditionalProperty
					          		{
					          			get
					          			{
					          				return this.MockRegistry.GetProperty<int>("global::MyCode.IMyOtherService.SomeAdditionalProperty", () => this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!), null);
					          			}
					          			set
					          			{
					          				this.MockRegistry.SetProperty("global::MyCode.IMyOtherService.SomeAdditionalProperty", value);
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}
		}
	}
}
