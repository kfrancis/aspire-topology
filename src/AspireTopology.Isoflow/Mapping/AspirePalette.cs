namespace AspireTopology.Isoflow.Mapping;

/// <summary>
/// The colours Aspire uses for its own site and documentation.
/// </summary>
/// <remarks>
/// Taken from the custom properties published by aspire.dev: <c>--aspire-color-*</c> for the brand
/// palette, and <c>--api-kind-*</c> / <c>--api-param-type-*</c> for the hues it already uses to
/// tell kinds of things apart. Reusing them means a generated diagram sits next to Aspire's own
/// material without looking like a different product.
/// </remarks>
public static class AspirePalette
{
    /// <summary>.NET purple. Aspire's primary action colour.</summary>
    public const string Purple = "#512bd4";

    /// <summary>The lighter purple Aspire uses for primary surfaces.</summary>
    public const string Primary = "#7455dd";

    /// <summary>The pale purple Aspire uses for secondary surfaces.</summary>
    public const string Secondary = "#b9aaee";

    /// <summary>
    /// A very pale purple, for the background of a group.
    /// </summary>
    /// <remarks>
    /// Isoflow fills rectangles at close to full opacity, so a group drawn in the secondary purple
    /// reads as a slab of colour laid over the diagram rather than as a region behind it.
    /// </remarks>
    public const string GroupTint = "#ece7fa";

    /// <summary>Aspire's near-black.</summary>
    public const string Black = "#1f1e33";

    /// <summary>Aspire's neutral grey.</summary>
    public const string Grey = "#dce0e8";

    /// <summary>Aspire's muted text colour.</summary>
    public const string Muted = "#66697e";

    /// <summary>The green Aspire uses for structs in API documentation.</summary>
    public const string Green = "#0a7d56";

    /// <summary>The teal Aspire uses for records in API documentation.</summary>
    public const string Teal = "#035e6e";

    /// <summary>The magenta Aspire uses for delegates in API documentation.</summary>
    public const string Magenta = "#9a2d55";

    /// <summary>The amber Aspire uses for enums in API documentation.</summary>
    public const string Amber = "#7d5200";

    /// <summary>The blue Aspire uses for parameter types in API documentation.</summary>
    public const string Blue = "#217a83";
}
