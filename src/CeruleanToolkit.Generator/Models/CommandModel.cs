namespace CeruleanToolkit.Generator.Models;

internal sealed record class CommandModel(
    string TypeFullName,
    string DisplayName,
    string? ParentTypeFullName,
    string? ParentDisplayName,
    int Order,
    string? PlatformSymbol,
    string PlatformMember,
    LocationInfo? Location
);
