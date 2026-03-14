#if NET10_0_OR_GREATER
using System.Threading.Tasks;

namespace Mockolate.ExampleTests;

public class Foo
{
	[Fact]
	public async Task X()
	{
		//IChocolateDispenser? sut = IChocolateDispenser.CreateMock();

		var x = IChocolateDispenser.CreateMock();
		_ = IChocolateDispenser3.CreateMock();

		x.DispenseChocolate("foo");
	}
}
#endif
