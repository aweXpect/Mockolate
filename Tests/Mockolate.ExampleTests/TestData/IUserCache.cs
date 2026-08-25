using System.Collections.Generic;

namespace Mockolate.ExampleTests.TestData;

public interface IReadOnlyUserCache
{
	IEnumerable<User> Users { get; set; }
}

public interface IUserCache : IReadOnlyUserCache
{
	new IList<User> Users { get; set; }
}
