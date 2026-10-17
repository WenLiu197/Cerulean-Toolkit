using System.Collections.Immutable;

namespace CeruleanToolkit.Generator.Models;

internal sealed record class ExtractionResult(
    ServiceModel? Model,
    ImmutableArray<ServiceDiagnostic> Diagnostics
);
