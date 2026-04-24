namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		public sealed class MethodTests
		{
			[Fact]
			public async Task ExplicitInterfaceImplementation_WithUnconstrainedGeneric_ShouldHaveDefaultConstraint()
			{
				GeneratorResult result = Generator
					.Run("""
					     using System.Threading.Tasks;
					     using Mockolate;

					     namespace MyCode;
					     public class Program
					     {
					         public static void Main(string[] args)
					         {
					     		_ = IMyService.CreateMock().Implementing<IMyOtherService>();
					         }
					     }

					     public interface IMyService
					     {
					         void MyMethod();
					     }

					     public interface IMyOtherService
					     {
					         Task<T?> DoSomethingAsync<T>();
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService__IMyOtherService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyOtherService.DoSomethingAsync{T}()" />
					          		global::System.Threading.Tasks.Task<T?> global::MyCode.IMyOtherService.DoSomethingAsync<T>()
					          			where T : default
					          		{
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task VirtualMethodOverride_WithConstrainedGeneric_ShouldNotRepeatConstraints()
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
					     		_ = MyService.CreateMock();
					         }
					     }

					     public interface IMyInterface
					     {
					     }

					     public class MyService
					     {
					         public virtual bool MyMethod<T>(T entity)
					             where T : IMyInterface
					         {
					             return true;
					         }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.MyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.MyMethod{T}(T)" />
					          		public override bool MyMethod<T>(T entity)
					          		{
					          """).IgnoringNewlineStyle().And
					.DoesNotContain("public override bool MyMethod<T>(T entity)\n\t\t\twhere T :").IgnoringNewlineStyle().Because("CS0460: constraints on override methods are inherited from the base method");
			}

			[Theory]
			[InlineData("class, T")]
			[InlineData("struct")]
			[InlineData("class")]
			[InlineData("class, notnull")]
			[InlineData("notnull")]
			[InlineData("unmanaged")]
			[InlineData("class?")]
			[InlineData("global::MyCode.IMyInterface")]
			[InlineData("new()")]
			[InlineData("global::MyCode.IMyInterface?")]
			[InlineData("allows ref struct")]
			[InlineData("class, allows ref struct")]
			[InlineData("global::MyCode.IMyInterface, new()")]
			public async Task Generic_ShouldApplyAllConstraints(string constraint)
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
					       		_ = IMyService.CreateMock();
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

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains($$"""
					            		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod1{T, U}(int)" />
					            		public bool MyMethod1<T, U>(int index)
					            			where T : notnull, new()
					            			where U : {{constraint}}
					            """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task Generic_WithoutConstraints_ShouldNotHaveWhereClause()
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

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod1{T, U}(int)" />
					          		public bool MyMethod1<T, U>(int index)
					          		{
					          """).IgnoringNewlineStyle();
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
					     		_ = IMyService.CreateMock();
					         }
					     }

					     public interface IMyService
					     {
					         void TriggerCallbacks(object?[] parameters);
					         int ProcessData(int methodExecution);
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>>(\"global::MyCode.IMyService.ProcessData\", m => m.Matches(\"methodExecution\", methodExecution));")
					.IgnoringNewlineStyle().And
					.Contains("methodSetup?.TriggerCallbacks(methodExecution);")
					.IgnoringNewlineStyle().And
					.Contains("return methodSetup?.TryGetReturnValue(methodExecution, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, methodExecution);")
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
					     		_ = IMyService.CreateMock();
					         }
					     }

					     public interface IMyService
					     {
					         int ProcessResult(int result);
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>>(\"global::MyCode.IMyService.ProcessResult\", m => m.Matches(\"result\", result));")
					.IgnoringNewlineStyle().And
					.Contains("methodSetup?.TriggerCallbacks(result);")
					.IgnoringNewlineStyle().And
					.Contains("return methodSetup?.TryGetReturnValue(result, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, result);")
					.IgnoringNewlineStyle();
			}

			[Fact]
			public async Task MethodWithEnumDefaultValue_ShouldGenerateCastExpression()
			{
				GeneratorResult result = Generator
					.Run("""
					     using Mockolate;

					     namespace MyCode;

					     public class Program
					     {
					         public static void Main(string[] args)
					         {
					     		_ = IService.CreateMock();
					         }
					     }

					     public enum Status { Active = 0, Inactive = 1 }

					     public interface IService
					     {
					         void Process(Status status = Status.Inactive);
					     }
					     """);

				await That(result.Sources)
					.ContainsKey("Mock.IService.g.cs").WhoseValue
					.Contains("(global::MyCode.Status)1")
					.IgnoringNewlineStyle();
			}

			[Fact]
			public async Task MethodWithStructDefaultValue_ShouldGenerateDefaultKeyword()
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
					     		_ = IService.CreateMock();
					         }
					     }

					     public interface IService
					     {
					         void Process(DateTime time = default);
					     }
					     """, typeof(DateTime));

				await That(result.Sources)
					.ContainsKey("Mock.IService.g.cs").WhoseValue
					.Contains("DateTime time = default")
					.IgnoringNewlineStyle();
			}

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

				await That(result.Sources).ContainsKey("Mock.IMyService__IMyServiceBase1__IMyServiceBase2.g.cs").WhoseValue
					.Contains("public string Value()").Once().And
					.Contains("public string Value(string p1)").Once().And
					.Contains("int global::MyCode.IMyServiceBase1.Value()").Once().And
					.Contains("public int Value(int p1)").Once().And
					.Contains("long global::MyCode.IMyServiceBase2.Value()").Once();
			}

			[Fact]
			public async Task ShouldImplementAllMethodsFromInterfaces()
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
					         bool MyMethod1(int index);
					         void MyMethod2(int index, bool isReadOnly);
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod1(int)" />
					          		public bool MyMethod1(int index)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<bool, int>, int>(0, "global::MyCode.IMyService.MyMethod1", index);
					          			bool hasWrappedResult = false;
					          			bool wrappedResult = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>("global::MyCode.IMyService.MyMethod1", "index", index));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wrappedResult = wraps.MyMethod1(index);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(index);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod1(int)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(index, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!, index);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod2(int, bool)" />
					          		public void MyMethod2(int index, bool isReadOnly)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.VoidMethodSetup<int, bool>, int, bool>(1, "global::MyCode.IMyService.MyMethod2", index, isReadOnly);
					          			bool hasWrappedResult = false;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int, bool>("global::MyCode.IMyService.MyMethod2", "index", index, "isReadOnly", isReadOnly));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyMethod2(index, isReadOnly);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(index, isReadOnly);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod2(int, bool)' was invoked without prior setup.");
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementImplicitlyInheritedMethods()
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

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyDirectMethod(int)" />
					          		public int MyDirectMethod(int value)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>, int>(0, "global::MyCode.IMyService.MyDirectMethod", value);
					          			bool hasWrappedResult = false;
					          			int wrappedResult = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>("global::MyCode.IMyService.MyDirectMethod", "value", value));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wrappedResult = wraps.MyDirectMethod(value);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(value);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyDirectMethod(int)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(value, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, value);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase1.MyBaseMethod1(int)" />
					          		public int MyBaseMethod1(int value)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>, int>(1, "global::MyCode.IMyServiceBase1.MyBaseMethod1", value);
					          			bool hasWrappedResult = false;
					          			int wrappedResult = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>("global::MyCode.IMyServiceBase1.MyBaseMethod1", "value", value));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wrappedResult = wraps.MyBaseMethod1(value);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(value);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyServiceBase1.MyBaseMethod1(int)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(value, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, value);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase2.MyBaseMethod2(int)" />
					          		public int MyBaseMethod2(int value)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>, int>(2, "global::MyCode.IMyServiceBase2.MyBaseMethod2", value);
					          			bool hasWrappedResult = false;
					          			int wrappedResult = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>("global::MyCode.IMyServiceBase2.MyBaseMethod2", "value", value));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wrappedResult = wraps.MyBaseMethod2(value);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(value);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyServiceBase2.MyBaseMethod2(int)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(value, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, value);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase3.MyBaseMethod3(int)" />
					          		public int MyBaseMethod3(int value)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int, int>, int>(3, "global::MyCode.IMyServiceBase3.MyBaseMethod3", value);
					          			bool hasWrappedResult = false;
					          			int wrappedResult = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>("global::MyCode.IMyServiceBase3.MyBaseMethod3", "value", value));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wrappedResult = wraps.MyBaseMethod3(value);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(value);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyServiceBase3.MyBaseMethod3(int)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(value, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(int)!, value);
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldImplementVirtualMethodsOfClassesAndAllExplicitlyFromAdditionalInterfaces()
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

					     public class MyProtectedService
					     {
					         protected virtual bool MyMethod(int index, bool isReadOnly, ref int value1, out bool flag)
					         {
					             flag = true;
					         }
					     }

					     public interface IMyOtherService
					     {
					         int SomeOtherMethod();
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.MyService__IMyOtherService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.MyMethod1(int, ref int, out bool)" />
					          		public override void MyMethod1(int index, ref int value1, out bool flag)
					          		{
					          			var ref_value1 = value1;
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.VoidMethodSetup<int, int, bool>, int, int, bool>(0, "global::MyCode.MyService.MyMethod1", index, ref_value1, default);
					          			bool hasWrappedResult = false;
					          			flag = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int, int, bool>("global::MyCode.MyService.MyMethod1", "index", index, "value1", value1, "flag", flag));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.MyService wraps)
					          				{
					          					wraps.MyMethod1(index, ref value1, out flag);
					          					hasWrappedResult = true;
					          				}
					          				if (!(methodSetup?.SkipBaseClass(this.MockRegistry.Behavior) ?? this.MockRegistry.Behavior.SkipBaseClass) && !hasWrappedResult)
					          				{
					          					base.MyMethod1(index, ref value1, out flag);
					          					hasWrappedResult = true;
					          				}
					          				if (!hasWrappedResult || methodSetup is global::Mockolate.Setup.VoidMethodSetup<int, int, bool>.WithParameterCollection)
					          				{
					          					if (methodSetup is global::Mockolate.Setup.VoidMethodSetup<int, int, bool>.WithParameterCollection wpc)
					          					{
					          						if (wpc.Parameter2 is global::Mockolate.Parameters.IRefParameter<int> refParam2)
					          						{
					          							value1 = refParam2.GetValue(value1);
					          						}
					          						if (wpc.Parameter3 is not global::Mockolate.Parameters.IOutParameter<bool> outParam3 || !outParam3.TryGetValue(out flag))
					          						{
					          							flag = this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!);
					          						}
					          					}
					          					else
					          					{
					          						flag = this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!);
					          					}
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(index, value1, flag);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.MyService.MyMethod1(int, int, bool)' was invoked without prior setup.");
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.MyMethod2(int, bool, ref int, out bool)" />
					          		protected override bool MyMethod2(int index, bool isReadOnly, ref int value1, out bool flag)
					          		{
					          			var ref_value1 = value1;
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<bool, int, bool, int, bool>, int, bool, int, bool>(1, "global::MyCode.MyService.MyMethod2", index, isReadOnly, ref_value1, default);
					          			bool hasWrappedResult = false;
					          			bool wrappedResult = default!;
					          			flag = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int, bool, int, bool>("global::MyCode.MyService.MyMethod2", "index", index, "isReadOnly", isReadOnly, "value1", value1, "flag", flag));
					          			}
					          			try
					          			{
					          				if (!(methodSetup?.SkipBaseClass(this.MockRegistry.Behavior) ?? this.MockRegistry.Behavior.SkipBaseClass))
					          				{
					          					wrappedResult = base.MyMethod2(index, isReadOnly, ref value1, out flag);
					          					hasWrappedResult = true;
					          				}
					          				if (!hasWrappedResult || methodSetup is global::Mockolate.Setup.ReturnMethodSetup<bool, int, bool, int, bool>.WithParameterCollection)
					          				{
					          					if (methodSetup is global::Mockolate.Setup.ReturnMethodSetup<bool, int, bool, int, bool>.WithParameterCollection wpc)
					          					{
					          						if (wpc.Parameter3 is global::Mockolate.Parameters.IRefParameter<int> refParam3)
					          						{
					          							value1 = refParam3.GetValue(value1);
					          						}
					          						if (wpc.Parameter4 is not global::Mockolate.Parameters.IOutParameter<bool> outParam4 || !outParam4.TryGetValue(out flag))
					          						{
					          							flag = this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!);
					          						}
					          					}
					          					else
					          					{
					          						flag = this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!);
					          					}
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(index, isReadOnly, value1, flag);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.MyService.MyMethod2(int, bool, int, bool)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(index, isReadOnly, value1, flag, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!, index, isReadOnly, value1, flag);
					          		}
					          """).IgnoringNewlineStyle().And
					.DoesNotContain("MyNonVirtualMethod").Because("The method is not virtual!").And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyOtherService.SomeOtherMethod()" />
					          		int global::MyCode.IMyOtherService.SomeOtherMethod()
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<int>>(0, "global::MyCode.IMyOtherService.SomeOtherMethod");
					          			bool hasWrappedResult = false;
					          			int wrappedResult = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation("global::MyCode.IMyOtherService.SomeOtherMethod"));
					          			}
					          			try
					          			{
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks();
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyOtherService.SomeOtherMethod()' was invoked without prior setup.");
					          			}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldSupportOptionalParameters()
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
					         void MyMethod1(int a, int b = 1, bool? c = null, string d = "default");
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          public void MyMethod1(int a, int b = 1, bool? c = null, string d = "default")
					          """);
				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		global::Mockolate.Setup.IVoidMethodSetupWithCallback<int, int, bool?, string> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int>? b, global::Mockolate.Parameters.IParameter<bool?>? c, global::Mockolate.Parameters.IParameter<string>? d)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int, int, bool?, string>.WithParameterCollection(MockRegistry, 0, "global::MyCode.IMyService.MyMethod1", CovariantParameterAdapter<int>.Wrap(a ?? global::Mockolate.It.IsNull<int>("null")), CovariantParameterAdapter<int>.Wrap(b ?? global::Mockolate.It.Is<int>(1)), CovariantParameterAdapter<bool?>.Wrap(c ?? global::Mockolate.It.Is<bool?>(null)), CovariantParameterAdapter<string>.Wrap(d ?? global::Mockolate.It.Is<string>("default")));
					          			this.MockRegistry.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int>? b, global::Mockolate.Parameters.IParameter<bool?>? c, global::Mockolate.Parameters.IParameter<string>? d)
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldSupportParamsParameters()
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
					         void MyMethod1(int a, params int[] b);
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          public void MyMethod1(int a, params int[] b)
					          """);
				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		global::Mockolate.Setup.IVoidMethodSetupWithCallback<int, int[]> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int[]>? b)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int, int[]>.WithParameterCollection(MockRegistry, "global::MyCode.IMyService.MyMethod1", CovariantParameterAdapter<int>.Wrap(a ?? global::Mockolate.It.IsNull<int>("null")), CovariantParameterAdapter<int[]>.Wrap(b ?? global::Mockolate.It.IsNull<int[]>("null")));
					          			this.MockRegistry.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int[]>? b)
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldSupportRefInAndOutParameters()
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
					         void MyMethod1(ref int index);
					         bool MyMethod2(int index, out bool isReadOnly);
					         void MyMethod3(in MyReadonlyStruct p1);
					         void MyMethod4(ref readonly MyReadonlyStruct p1);
					     }
					     
					     public readonly struct MyReadonlyStruct
					     {
					         public int Value { get; init; }
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod1(ref int)" />
					          		public void MyMethod1(ref int index)
					          		{
					          			var ref_index = index;
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.VoidMethodSetup<int>>("global::MyCode.IMyService.MyMethod1", m => m.Matches("index", ref_index));
					          			bool hasWrappedResult = false;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int>("global::MyCode.IMyService.MyMethod1", "index", index));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyMethod1(ref index);
					          					hasWrappedResult = true;
					          				}
					          				if (!hasWrappedResult || methodSetup is global::Mockolate.Setup.VoidMethodSetup<int>.WithParameterCollection)
					          				{
					          					if (methodSetup is global::Mockolate.Setup.VoidMethodSetup<int>.WithParameterCollection wpc)
					          					{
					          						if (wpc.Parameter1 is global::Mockolate.Parameters.IRefParameter<int> refParam1)
					          						{
					          							index = refParam1.GetValue(index);
					          						}
					          					}
					          					else
					          					{
					          					}
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(index);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod1(int)' was invoked without prior setup.");
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod2(int, out bool)" />
					          		public bool MyMethod2(int index, out bool isReadOnly)
					          		{
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.ReturnMethodSetup<bool, int, bool>>("global::MyCode.IMyService.MyMethod2", m => m.Matches("index", index, "isReadOnly", default));
					          			bool hasWrappedResult = false;
					          			bool wrappedResult = default!;
					          			isReadOnly = default!;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<int, bool>("global::MyCode.IMyService.MyMethod2", "index", index, "isReadOnly", isReadOnly));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wrappedResult = wraps.MyMethod2(index, out isReadOnly);
					          					hasWrappedResult = true;
					          				}
					          				if (!hasWrappedResult || methodSetup is global::Mockolate.Setup.ReturnMethodSetup<bool, int, bool>.WithParameterCollection)
					          				{
					          					if (methodSetup is global::Mockolate.Setup.ReturnMethodSetup<bool, int, bool>.WithParameterCollection wpc)
					          					{
					          						if (wpc.Parameter2 is not global::Mockolate.Parameters.IOutParameter<bool> outParam2 || !outParam2.TryGetValue(out isReadOnly))
					          						{
					          							isReadOnly = this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!);
					          						}
					          					}
					          					else
					          					{
					          						isReadOnly = this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!);
					          					}
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(index, isReadOnly);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod2(int, bool)' was invoked without prior setup.");
					          			}
					          			if (methodSetup?.HasReturnCallbacks != true && hasWrappedResult)
					          			{
					          				return wrappedResult;
					          			}
					          			return methodSetup?.TryGetReturnValue(index, isReadOnly, out var returnValue) == true ? returnValue : this.MockRegistry.Behavior.DefaultValue.Generate(default(bool)!, index, isReadOnly);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod3(in global::MyCode.MyReadonlyStruct)" />
					          		public void MyMethod3(in global::MyCode.MyReadonlyStruct p1)
					          		{
					          			var ref_p1 = p1;
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.VoidMethodSetup<global::MyCode.MyReadonlyStruct>>("global::MyCode.IMyService.MyMethod3", m => m.Matches("p1", ref_p1));
					          			bool hasWrappedResult = false;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<global::MyCode.MyReadonlyStruct>("global::MyCode.IMyService.MyMethod3", "p1", p1));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyMethod3(in p1);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(p1);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod3(MyReadonlyStruct)' was invoked without prior setup.");
					          			}
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod4(ref readonly global::MyCode.MyReadonlyStruct)" />
					          		public void MyMethod4(ref readonly global::MyCode.MyReadonlyStruct p1)
					          		{
					          			var ref_p1 = p1;
					          			var methodSetup = this.MockRegistry.GetMethodSetup<global::Mockolate.Setup.VoidMethodSetup<global::MyCode.MyReadonlyStruct>>("global::MyCode.IMyService.MyMethod4", m => m.Matches("p1", ref_p1));
					          			bool hasWrappedResult = false;
					          			if (this.MockRegistry.Behavior.SkipInteractionRecording == false)
					          			{
					          				this.MockRegistry.RegisterInteraction(new global::Mockolate.Interactions.MethodInvocation<global::MyCode.MyReadonlyStruct>("global::MyCode.IMyService.MyMethod4", "p1", p1));
					          			}
					          			try
					          			{
					          				if (this.MockRegistry.Wraps is global::MyCode.IMyService wraps)
					          				{
					          					wraps.MyMethod4(in p1);
					          					hasWrappedResult = true;
					          				}
					          			}
					          			finally
					          			{
					          				methodSetup?.TriggerCallbacks(p1);
					          			}
					          			if (methodSetup is null && !hasWrappedResult && this.MockRegistry.Behavior.ThrowWhenNotSetup)
					          			{
					          				throw new global::Mockolate.Exceptions.MockNotSetupException("The method 'global::MyCode.IMyService.MyMethod4(MyReadonlyStruct)' was invoked without prior setup.");
					          			}
					          		}
					          """).IgnoringNewlineStyle();
			}

			[Fact]
			public async Task ShouldSupportSpanParameters()
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
					         void MyMethod1(Span<char> buffer);
					         bool MyMethod2(ReadOnlySpan<int> values);
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		global::Mockolate.Setup.IVoidMethodSetupWithCallback<global::Mockolate.Setup.SpanWrapper<char>> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod1(global::Mockolate.Parameters.ISpanParameter<char> buffer)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<global::Mockolate.Setup.SpanWrapper<char>>.WithParameterCollection(MockRegistry, "global::MyCode.IMyService.MyMethod1", CovariantParameterAdapter<global::Mockolate.Setup.SpanWrapper<char>>.Wrap(buffer));
					          			this.MockRegistry.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Setup.IReturnMethodSetupWithCallback<bool, global::Mockolate.Setup.ReadOnlySpanWrapper<int>> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod2(global::Mockolate.Parameters.IReadOnlySpanParameter<int> values)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.ReturnMethodSetup<bool, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>.WithParameterCollection(MockRegistry, "global::MyCode.IMyService.MyMethod2", CovariantParameterAdapter<global::Mockolate.Setup.ReadOnlySpanWrapper<int>>.Wrap(values));
					          			this.MockRegistry.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod1(global::Mockolate.Parameters.IVerifySpanParameter<char> buffer)
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod2(global::Mockolate.Parameters.IVerifyReadOnlySpanParameter<int> values)
					          """).IgnoringNewlineStyle();
			}
		}
	}
}
