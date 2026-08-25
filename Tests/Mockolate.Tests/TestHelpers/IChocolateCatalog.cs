using System.Collections.Generic;

namespace Mockolate.Tests.TestHelpers;

public interface IChocolateSource
{
	IEnumerable<T> Get<T>() where T : notnull;
}

public interface IChocolateCatalog : IChocolateSource
{
	new IList<T> Get<T>() where T : notnull;
}
