using System.Collections.Generic;

namespace Mockolate.SourceGenerators.Tests.TestHelpers;

public record GeneratorResult(Dictionary<string, string> Sources, string[] Diagnostics);
