#if NET10_0_OR_GREATER
using System.Threading.Tasks;

namespace Mockolate.ExampleTests;

public class Foo
{
	[Fact]
	public async Task X()
	{
		//IChocolateDispenser? sut = IChocolateDispenser.CreateMock();
		var y = Mock.Create<IChocolateDispenser2>();
		
		var x = IChocolateDispenser3.CreateMock();
		_ = IChocolateDispenser.CreateMock2();

		x.DispenseChocolate("foo");
	}
}
#endif
