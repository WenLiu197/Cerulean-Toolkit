namespace CeruleanToolkit.Generator.Models;

internal sealed record class ServiceDiagnostic(
    string Id,
    LocationInfo? Location,
    string? Arg0 = null,
    string? Arg1 = null,
    string? Arg2 = null
);
