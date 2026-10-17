using System.Collections.Immutable;

namespace CeruleanToolkit.Generator.Models;

internal sealed record class CommandExtractionResult(
    CommandModel? Model,
    ImmutableArray<GeneratorDiagnostic> Diagnostics
);
