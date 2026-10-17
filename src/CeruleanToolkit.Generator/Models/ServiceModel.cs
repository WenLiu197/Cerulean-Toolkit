namespace CeruleanToolkit.Generator.Models;

internal sealed record class ServiceModel(
    string InterfaceFullName,
    string ImplementationFullName,
    string LifetimeMethodName,
    string? PlatformSymbol,
    bool UseTypeOverload
);