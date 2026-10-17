using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CeruleanToolkit.Generator.Models;

public sealed record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationInfo? From(Location location) =>
        location.SourceTree is null ? null : new(
            location.SourceTree.FilePath,
            location.SourceSpan,
            location.GetLineSpan().Span);
}