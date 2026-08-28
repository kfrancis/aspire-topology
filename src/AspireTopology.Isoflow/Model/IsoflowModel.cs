using System.Text.Json.Serialization;

namespace AspireTopology.Isoflow.Model;

/// <summary>
/// The <c>initialData</c> document consumed by the Isoflow React component.
/// </summary>
/// <remarks>
/// This is our local statement of Isoflow's contract. Keeping it as typed records rather than
/// anonymous objects means a change on Isoflow's side shows up as a compile error in one place.
/// </remarks>
/// <param name="Version">Schema version string written into the document.</param>
/// <param name="Title">Diagram title.</param>
/// <param name="Icons">Icons available to items.</param>
/// <param name="Colors">Colours available to connectors and rectangles.</param>
/// <param name="Items">The model items, independent of any view.</param>
/// <param name="Views">Arrangements of the items on the isometric grid.</param>
public sealed record IsoflowDocument(
    string Version,
    string Title,
    IReadOnlyList<IsoflowIcon> Icons,
    IReadOnlyList<IsoflowColor> Colors,
    IReadOnlyList<IsoflowItem> Items,
    IReadOnlyList<IsoflowView> Views);

/// <summary>An icon an item can be drawn with.</summary>
/// <param name="Id">Identifier referenced by <see cref="IsoflowItem.Icon"/>.</param>
/// <param name="Name">Display name.</param>
/// <param name="Url">Image URL. AspireTopology emits self-contained data URIs.</param>
/// <param name="Collection">Optional collection the icon belongs to.</param>
/// <param name="IsIsometric">Whether the image is already drawn in isometric projection.</param>
public sealed record IsoflowIcon(
    string Id,
    string Name,
    string Url,
    string? Collection = null,
    [property: JsonPropertyName("isIsometric")] bool IsIsometric = false);

/// <summary>A colour in the document palette.</summary>
/// <param name="Id">Identifier referenced by connectors and rectangles.</param>
/// <param name="Value">CSS colour value.</param>
public sealed record IsoflowColor(string Id, string Value);

/// <summary>A node in the Isoflow model.</summary>
/// <param name="Id">Identifier referenced by view items.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">Free text shown in the item details panel.</param>
/// <param name="Icon">Identifier of the icon to draw.</param>
public sealed record IsoflowItem(
    string Id,
    string Name,
    string? Description,
    string Icon);

/// <summary>An arrangement of items on the isometric grid.</summary>
/// <param name="Id">View identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Items">Placed items.</param>
/// <param name="Connectors">Lines between placed items.</param>
/// <param name="Rectangles">Background regions.</param>
/// <param name="TextBoxes">Free-standing labels.</param>
public sealed record IsoflowView(
    string Id,
    string Name,
    IReadOnlyList<IsoflowViewItem> Items,
    IReadOnlyList<IsoflowConnector> Connectors,
    IReadOnlyList<IsoflowRectangle> Rectangles,
    IReadOnlyList<IsoflowTextBox> TextBoxes);

/// <summary>An item placed on the grid.</summary>
/// <param name="Id">Identifier of the <see cref="IsoflowItem"/> being placed.</param>
/// <param name="Tile">Grid position.</param>
/// <param name="LabelHeight">Height of the label stem above the tile.</param>
public sealed record IsoflowViewItem(
    string Id,
    IsoflowTile Tile,
    int LabelHeight = 60);

/// <summary>A position on the isometric grid.</summary>
/// <param name="X">Grid column.</param>
/// <param name="Y">Grid row.</param>
public sealed record IsoflowTile(int X, int Y);

/// <summary>A line between two placed items.</summary>
/// <param name="Id">Connector identifier.</param>
/// <param name="Anchors">The points the line runs between, in order.</param>
/// <param name="Description">Free text label.</param>
/// <param name="Color">Identifier of a colour in the document palette.</param>
/// <param name="Style">Line style, for example <c>SOLID</c> or <c>DASHED</c>.</param>
/// <param name="Width">Line width.</param>
public sealed record IsoflowConnector(
    string Id,
    IReadOnlyList<IsoflowAnchor> Anchors,
    string? Description = null,
    string? Color = null,
    string? Style = null,
    int Width = 10);

/// <summary>One end of a connector.</summary>
/// <param name="Id">Anchor identifier, unique within the connector.</param>
/// <param name="Ref">What the anchor is attached to.</param>
public sealed record IsoflowAnchor(string Id, IsoflowAnchorRef Ref);

/// <summary>The target of a connector anchor.</summary>
/// <param name="Item">Identifier of a placed item.</param>
/// <param name="Tile">A bare grid position, used when the anchor is not attached to an item.</param>
public sealed record IsoflowAnchorRef(string? Item = null, IsoflowTile? Tile = null);

/// <summary>A background region on the grid.</summary>
/// <param name="Id">Rectangle identifier.</param>
/// <param name="From">One corner.</param>
/// <param name="To">The opposite corner.</param>
/// <param name="Color">Identifier of a colour in the document palette.</param>
public sealed record IsoflowRectangle(
    string Id,
    IsoflowTile From,
    IsoflowTile To,
    string? Color = null);

/// <summary>A free-standing label on the grid.</summary>
/// <param name="Id">Text box identifier.</param>
/// <param name="Tile">Grid position.</param>
/// <param name="Content">The text.</param>
/// <param name="FontSize">Relative font size.</param>
public sealed record IsoflowTextBox(
    string Id,
    IsoflowTile Tile,
    string Content,
    double FontSize = 0.6);
