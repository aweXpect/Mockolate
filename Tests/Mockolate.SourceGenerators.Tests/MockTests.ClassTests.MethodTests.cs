namespace Mockolate.SourceGenerators.Tests;

public sealed partial class MockTests
{
	public sealed partial class ClassTests
	{
		public sealed class MethodTests
		{
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
					.Contains("MethodSetupResult<int> methodExecution1 = this.Registrations.InvokeMethod<int>(")
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
					     		_ = IMyService.CreateMock();
					         }
					     }

					     public interface IMyService
					     {
					         int ProcessResult(int result);
					     }
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("MethodSetupResult<int> methodExecution = this.Registrations.InvokeMethod<int>(")
					.IgnoringNewlineStyle().And
					.Contains("methodExecution.TriggerCallbacks(result)")
					.IgnoringNewlineStyle().And
					.Contains("return methodExecution.Result;")
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
					          			global::Mockolate.Setup.MethodSetupResult<bool> methodExecution = this.Registrations.InvokeMethod<bool>("global::MyCode.IMyService.MyMethod1", p => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!, p), new global::Mockolate.Parameters.NamedParameterValue("index", index));
					          			if (this.Wraps is not null)
					          			{
					          				var baseResult = this.Wraps.MyMethod1(index);
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
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod2(int, bool)" />
					          		public void MyMethod2(int index, bool isReadOnly)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult methodExecution = this.Registrations.InvokeMethod("global::MyCode.IMyService.MyMethod2", new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isReadOnly", isReadOnly));
					          			if (this.Wraps is not null)
					          			{
					          				this.Wraps.MyMethod2(index, isReadOnly);
					          			}
					          			methodExecution.TriggerCallbacks(index, isReadOnly);
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
					          			global::Mockolate.Setup.MethodSetupResult<int> methodExecution = this.Registrations.InvokeMethod<int>("global::MyCode.IMyService.MyDirectMethod", p => this.Registrations.Behavior.DefaultValue.Generate(default(int)!, p), new global::Mockolate.Parameters.NamedParameterValue("value", value));
					          			if (this.Wraps is not null)
					          			{
					          				var baseResult = this.Wraps.MyDirectMethod(value);
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
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase1.MyBaseMethod1(int)" />
					          		public int MyBaseMethod1(int value)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult<int> methodExecution = this.Registrations.InvokeMethod<int>("global::MyCode.IMyServiceBase1.MyBaseMethod1", p => this.Registrations.Behavior.DefaultValue.Generate(default(int)!, p), new global::Mockolate.Parameters.NamedParameterValue("value", value));
					          			if (this.Wraps is not null)
					          			{
					          				var baseResult = this.Wraps.MyBaseMethod1(value);
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
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase2.MyBaseMethod2(int)" />
					          		public int MyBaseMethod2(int value)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult<int> methodExecution = this.Registrations.InvokeMethod<int>("global::MyCode.IMyServiceBase2.MyBaseMethod2", p => this.Registrations.Behavior.DefaultValue.Generate(default(int)!, p), new global::Mockolate.Parameters.NamedParameterValue("value", value));
					          			if (this.Wraps is not null)
					          			{
					          				var baseResult = this.Wraps.MyBaseMethod2(value);
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
					          		/// <inheritdoc cref="global::MyCode.IMyServiceBase3.MyBaseMethod3(int)" />
					          		public int MyBaseMethod3(int value)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult<int> methodExecution = this.Registrations.InvokeMethod<int>("global::MyCode.IMyServiceBase3.MyBaseMethod3", p => this.Registrations.Behavior.DefaultValue.Generate(default(int)!, p), new global::Mockolate.Parameters.NamedParameterValue("value", value));
					          			if (this.Wraps is not null)
					          			{
					          				var baseResult = this.Wraps.MyBaseMethod3(value);
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
					          			global::Mockolate.Setup.MethodSetupResult methodExecution = this.Registrations.InvokeMethod("global::MyCode.MyService.MyMethod1", new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("value1", value1), new global::Mockolate.Parameters.NamedParameterValue("flag", null));
					          			if (!methodExecution.SkipBaseClass)
					          			{
					          				base.MyMethod1(index, ref value1, out flag);
					          			}
					          			methodExecution.TriggerCallbacks(index, value1, flag);

					          			value1 = methodExecution.SetRefParameter<int>("value1", value1);
					          			methodExecution.TriggerCallbacks(index, value1, flag);

					          			flag = methodExecution.SetOutParameter<bool>("flag", () => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!));
					          			methodExecution.TriggerCallbacks(index, value1, flag);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.MyService.MyMethod2(int, bool, ref int, out bool)" />
					          		protected override bool MyMethod2(int index, bool isReadOnly, ref int value1, out bool flag)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult<bool> methodExecution = this.Registrations.InvokeMethod<bool>("global::MyCode.MyService.MyMethod2", p => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!, p), new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isReadOnly", isReadOnly), new global::Mockolate.Parameters.NamedParameterValue("value1", value1), new global::Mockolate.Parameters.NamedParameterValue("flag", null));
					          			if (!methodExecution.SkipBaseClass)
					          			{
					          				var baseResult = base.MyMethod2(index, isReadOnly, ref value1, out flag);
					          				if (methodExecution.HasSetupResult == true)
					          				{
					          					value1 = methodExecution.SetRefParameter<int>("value1", value1);
					          				}

					          				if (methodExecution.HasSetupResult == true)
					          				{
					          					flag = methodExecution.SetOutParameter<bool>("flag", () => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!));
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
					          				flag = methodExecution.SetOutParameter<bool>("flag", () => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!));
					          			}

					          			methodExecution.TriggerCallbacks(index, isReadOnly, value1, flag);
					          			return methodExecution.Result;
					          		}
					          """).IgnoringNewlineStyle().And
					.DoesNotContain("MyNonVirtualMethod").Because("The method is not virtual!").And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyOtherService.SomeOtherMethod()" />
					          		int global::MyCode.IMyOtherService.SomeOtherMethod()
					          		{
					          			global::Mockolate.Setup.MethodSetupResult<int> methodExecution = this.Registrations.InvokeMethod<int>("global::MyCode.IMyOtherService.SomeOtherMethod", p => this.Registrations.Behavior.DefaultValue.Generate(default(int)!, p));
					          			methodExecution.TriggerCallbacks();
					          			return methodExecution.Result;
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
					          		global::Mockolate.Setup.IVoidMethodSetup<int, int, bool?, string> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int>? b = null, global::Mockolate.Parameters.IParameter<bool?>? c = null, global::Mockolate.Parameters.IParameter<string>? d = null)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int, int, bool?, string>("global::MyCode.IMyService.MyMethod1", new global::Mockolate.Parameters.NamedParameter("a", (global::Mockolate.Parameters.IParameter)(a ?? global::Mockolate.It.IsNull<int>())), new global::Mockolate.Parameters.NamedParameter("b", (global::Mockolate.Parameters.IParameter)(b ?? global::Mockolate.It.Is<int>(1))), new global::Mockolate.Parameters.NamedParameter("c", (global::Mockolate.Parameters.IParameter)(c ?? global::Mockolate.It.Is<bool?>(null))), new global::Mockolate.Parameters.NamedParameter("d", (global::Mockolate.Parameters.IParameter)(d ?? global::Mockolate.It.Is<string>("default"))));
					          			this.Registrations.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int>? b = null, global::Mockolate.Parameters.IParameter<bool?>? c = null, global::Mockolate.Parameters.IParameter<string>? d = null)
					          			=> this.Registrations.Method<IMockVerifyForIMyService>(this, new global::Mockolate.Setup.MethodParameterMatch("global::MyCode.IMyService.MyMethod1", [ new global::Mockolate.Parameters.NamedParameter("a", (global::Mockolate.Parameters.IParameter)(a ?? global::Mockolate.It.IsNull<int>())), new global::Mockolate.Parameters.NamedParameter("b", (global::Mockolate.Parameters.IParameter)(b ?? global::Mockolate.It.Is<int>(1))), new global::Mockolate.Parameters.NamedParameter("c", (global::Mockolate.Parameters.IParameter)(c ?? global::Mockolate.It.Is<bool?>(null))), new global::Mockolate.Parameters.NamedParameter("d", (global::Mockolate.Parameters.IParameter)(d ?? global::Mockolate.It.Is<string>("default"))), ]));
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
					          		global::Mockolate.Setup.IVoidMethodSetup<int, int[]> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int[]>? b)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<int, int[]>("global::MyCode.IMyService.MyMethod1", new global::Mockolate.Parameters.NamedParameter("a", (global::Mockolate.Parameters.IParameter)(a ?? global::Mockolate.It.IsNull<int>())), new global::Mockolate.Parameters.NamedParameter("b", (global::Mockolate.Parameters.IParameter)(b ?? global::Mockolate.It.IsNull<int[]>())));
					          			this.Registrations.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod1(global::Mockolate.Parameters.IParameter<int>? a, global::Mockolate.Parameters.IParameter<int[]>? b)
					          			=> this.Registrations.Method<IMockVerifyForIMyService>(this, new global::Mockolate.Setup.MethodParameterMatch("global::MyCode.IMyService.MyMethod1", [ new global::Mockolate.Parameters.NamedParameter("a", (global::Mockolate.Parameters.IParameter)(a ?? global::Mockolate.It.IsNull<int>())), new global::Mockolate.Parameters.NamedParameter("b", (global::Mockolate.Parameters.IParameter)(b ?? global::Mockolate.It.IsNull<int[]>())), ]));
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
					     """);

				await That(result.Sources).ContainsKey("Mock.IMyService.g.cs").WhoseValue
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod1(ref int)" />
					          		public void MyMethod1(ref int index)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult methodExecution = this.Registrations.InvokeMethod("global::MyCode.IMyService.MyMethod1", new global::Mockolate.Parameters.NamedParameterValue("index", index));
					          			if (this.Wraps is not null)
					          			{
					          				this.Wraps.MyMethod1(ref index);
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
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod2(int, out bool)" />
					          		public bool MyMethod2(int index, out bool isReadOnly)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult<bool> methodExecution = this.Registrations.InvokeMethod<bool>("global::MyCode.IMyService.MyMethod2", p => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!, p), new global::Mockolate.Parameters.NamedParameterValue("index", index), new global::Mockolate.Parameters.NamedParameterValue("isReadOnly", null));
					          			if (this.Wraps is not null)
					          			{
					          				var baseResult = this.Wraps.MyMethod2(index, out isReadOnly);
					          				if (methodExecution.HasSetupResult == true)
					          				{
					          					isReadOnly = methodExecution.SetOutParameter<bool>("isReadOnly", () => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!));
					          				}

					          				if (!methodExecution.HasSetupResult)
					          				{
					          					methodExecution.TriggerCallbacks(index, isReadOnly);
					          					return baseResult;
					          				}
					          			}
					          			isReadOnly = methodExecution.SetOutParameter<bool>("isReadOnly", () => this.Registrations.Behavior.DefaultValue.Generate(default(bool)!));
					          			methodExecution.TriggerCallbacks(index, isReadOnly);
					          			return methodExecution.Result;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod3(in MyReadonlyStruct)" />
					          		public void MyMethod3(in MyReadonlyStruct p1)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult methodExecution = this.Registrations.InvokeMethod("global::MyCode.IMyService.MyMethod3", new global::Mockolate.Parameters.NamedParameterValue("p1", p1));
					          			if (this.Wraps is not null)
					          			{
					          				this.Wraps.MyMethod3(in p1);
					          			}
					          			methodExecution.TriggerCallbacks(p1);
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		/// <inheritdoc cref="global::MyCode.IMyService.MyMethod4(ref readonly MyReadonlyStruct)" />
					          		public void MyMethod4(ref readonly MyReadonlyStruct p1)
					          		{
					          			global::Mockolate.Setup.MethodSetupResult methodExecution = this.Registrations.InvokeMethod("global::MyCode.IMyService.MyMethod4", new global::Mockolate.Parameters.NamedParameterValue("p1", p1));
					          			if (this.Wraps is not null)
					          			{
					          				this.Wraps.MyMethod4(in p1);
					          			}
					          			methodExecution.TriggerCallbacks(p1);
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
					          		global::Mockolate.Setup.IVoidMethodSetup<global::Mockolate.Setup.SpanWrapper<char>> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod1(global::Mockolate.Parameters.ISpanParameter<char> buffer)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.VoidMethodSetup<global::Mockolate.Setup.SpanWrapper<char>>("global::MyCode.IMyService.MyMethod1", new global::Mockolate.Parameters.NamedParameter("buffer", (global::Mockolate.Parameters.IParameter)(buffer)));
					          			this.Registrations.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Setup.IReturnMethodSetup<bool, global::Mockolate.Setup.ReadOnlySpanWrapper<int>> global::Mockolate.Mock.IMockSetupForIMyService.MyMethod2(global::Mockolate.Parameters.IReadOnlySpanParameter<int> values)
					          		{
					          			var methodSetup = new global::Mockolate.Setup.ReturnMethodSetup<bool, global::Mockolate.Setup.ReadOnlySpanWrapper<int>>("global::MyCode.IMyService.MyMethod2", new global::Mockolate.Parameters.NamedParameter("values", (global::Mockolate.Parameters.IParameter)(values)));
					          			this.Registrations.SetupMethod(methodSetup);
					          			return methodSetup;
					          		}
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod1(global::Mockolate.Parameters.IVerifySpanParameter<char> buffer)
					          			=> this.Registrations.Method<IMockVerifyForIMyService>(this, new global::Mockolate.Setup.MethodParameterMatch("global::MyCode.IMyService.MyMethod1", [ new global::Mockolate.Parameters.NamedParameter("buffer", (global::Mockolate.Parameters.IParameter)(buffer)), ]));
					          """).IgnoringNewlineStyle().And
					.Contains("""
					          		global::Mockolate.Verify.VerificationResult<IMockVerifyForIMyService> IMockVerifyForIMyService.MyMethod2(global::Mockolate.Parameters.IVerifyReadOnlySpanParameter<int> values)
					          			=> this.Registrations.Method<IMockVerifyForIMyService>(this, new global::Mockolate.Setup.MethodParameterMatch("global::MyCode.IMyService.MyMethod2", [ new global::Mockolate.Parameters.NamedParameter("values", (global::Mockolate.Parameters.IParameter)(values)), ]));
					          """).IgnoringNewlineStyle();
			}
		}
	}
}
