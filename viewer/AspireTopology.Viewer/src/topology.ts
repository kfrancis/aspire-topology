/**
 * The shape AspireTopology.Isoflow writes. Only the parts the viewer touches are typed here;
 * everything else is handed to Isoflow untouched.
 */
export interface IsoflowIcon {
  id: string;
  name: string;
  url?: string;
  collection?: string;
  isIsometric?: boolean;
}

export interface IsoflowColor {
  id: string;
  value: string;
}

export interface IsoflowItem {
  id: string;
  name: string;
  description?: string | null;
  icon: string;
}

export interface IsoflowInitialData {
  version: string;
  title: string;
  icons: IsoflowIcon[];
  colors: IsoflowColor[];
  items: IsoflowItem[];
  views: IsoflowView[];
}

export interface IsoflowConnector {
  id: string;
  color?: string | null;
  style?: string | null;
}

export interface IsoflowView {
  id: string;
  name: string;
  items: { id: string; tile: { x: number; y: number } }[];
  connectors: IsoflowConnector[];
  rectangles: unknown[];
  textBoxes: unknown[];
}

/** A human-owned layout file: node id to grid position. */
export type LayoutOverrides = Record<string, { x: number; y: number }>;

/**
 * Merges icon packs with the icons the generator emitted.
 *
 * Pack icons win on identifier collisions, so a viewer that has @isoflow/isopacks installed gets
 * proper artwork, while a generated diagram opened anywhere still renders with the self-contained
 * SVG data URIs AspireTopology emits.
 */
export function mergeIcons(packIcons: IsoflowIcon[], documentIcons: IsoflowIcon[]): IsoflowIcon[] {
  const byId = new Map<string, IsoflowIcon>();

  for (const icon of documentIcons) {
    byId.set(icon.id, icon);
  }

  for (const icon of packIcons) {
    byId.set(icon.id, icon);
  }

  return [...byId.values()];
}

/**
 * Applies saved positions on top of the generated ones.
 *
 * This is the split the project is designed around: topology.json is generated and overwritten,
 * architecture.layout.json is owned by a human and survives regeneration.
 */
export function applyLayoutOverrides(
  data: IsoflowInitialData,
  overrides: LayoutOverrides,
): IsoflowInitialData {
  if (Object.keys(overrides).length === 0) {
    return data;
  }

  return {
    ...data,
    views: data.views.map((view) => ({
      ...view,
      items: view.items.map((item) =>
        overrides[item.id] ? { ...item, tile: overrides[item.id] } : item,
      ),
    })),
  };
}

/** Reduces an Isoflow model back to the positions worth persisting. */
export function extractLayoutOverrides(data: IsoflowInitialData): LayoutOverrides {
  const overrides: LayoutOverrides = {};

  for (const view of data.views ?? []) {
    for (const item of view.items ?? []) {
      overrides[item.id] = { x: item.tile.x, y: item.tile.y };
    }
  }

  return overrides;
}

export interface Legend {
  kinds: { id: string; name: string; color: string }[];
  edges: { id: string; name: string; color: string; dashed: boolean }[];
}

/**
 * Builds a legend from the document itself.
 *
 * Colours are read back out of the generated icons rather than duplicated here, so the renderer
 * stays the single source of truth for what a kind looks like. Only the kinds and edge kinds that
 * actually appear are listed.
 */
export function legendFor(data: IsoflowInitialData): Legend {
  const usedIcons = new Set((data.items ?? []).map((item) => item.icon));

  const kinds = (data.icons ?? [])
    .filter((icon) => usedIcons.has(icon.id))
    .map((icon) => ({ id: icon.id, name: icon.name, color: dominantColor(icon.url) }))
    .filter((kind): kind is { id: string; name: string; color: string } => kind.color !== null)
    .sort((a, b) => a.name.localeCompare(b.name));

  const usedColors = new Set<string>();
  const dashed = new Set<string>();

  for (const view of data.views ?? []) {
    for (const connector of view.connectors ?? []) {
      if (connector.color) {
        usedColors.add(connector.color);
        if (connector.style === 'DASHED') {
          dashed.add(connector.color);
        }
      }
    }
  }

  const edges = (data.colors ?? [])
    .filter((color) => usedColors.has(color.id))
    .map((color) => ({
      id: color.id,
      name: color.id.charAt(0).toUpperCase() + color.id.slice(1),
      color: color.value,
      dashed: dashed.has(color.id),
    }));

  return { kinds, edges };
}

/** Reads the first fill colour out of an inline SVG data URI. */
function dominantColor(url: string | undefined): string | null {
  if (!url?.startsWith('data:image/svg+xml;base64,')) {
    return null;
  }

  try {
    const svg = atob(url.slice('data:image/svg+xml;base64,'.length));
    return svg.match(/fill="(#[0-9a-fA-F]{3,8})"/)?.[1] ?? null;
  } catch {
    return null;
  }
}
