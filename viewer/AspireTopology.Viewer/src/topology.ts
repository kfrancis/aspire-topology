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

export interface IsoflowInitialData {
  version: string;
  title: string;
  icons: IsoflowIcon[];
  colors: unknown[];
  items: unknown[];
  views: IsoflowView[];
}

export interface IsoflowView {
  id: string;
  name: string;
  items: { id: string; tile: { x: number; y: number } }[];
  connectors: unknown[];
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
