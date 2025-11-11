using Mockolate.V2;

namespace Mockolate;

public interface IMockSubject<T>
{
	V2.IMock<T> Mock { get; }
}
