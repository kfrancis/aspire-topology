import { useEffect, useState } from 'react';
import Isoflow from 'isoflow';
import isoflowIsopack from '@isoflow/isopacks/dist/isoflow';
import awsIsopack from '@isoflow/isopacks/dist/aws';
import azureIsopack from '@isoflow/isopacks/dist/azure';
import gcpIsopack from '@isoflow/isopacks/dist/gcp';
import kubernetesIsopack from '@isoflow/isopacks/dist/kubernetes';
import { flattenCollections } from '@isoflow/isopacks/dist/utils';
import {
  applyLayoutOverrides,
  extractLayoutOverrides,
  mergeIcons,
  type IsoflowInitialData,
  type LayoutOverrides,
} from './topology';

/** Where the generated diagram is served from. Override with VITE_TOPOLOGY_URL. */
const topologyUrl = import.meta.env.VITE_TOPOLOGY_URL ?? '/topology.isoflow.json';

/**
 * Saved positions are kept separately from the generated topology, so regenerating the diagram
 * does not throw away a hand-tidied layout. This viewer keeps them in local storage; writing them
 * to architecture.layout.json is the next step.
 */
const layoutStorageKey = 'aspire-topology.layout';

function readSavedLayout(): LayoutOverrides {
  try {
    const raw = window.localStorage.getItem(layoutStorageKey);
    return raw ? (JSON.parse(raw) as LayoutOverrides) : {};
  } catch {
    return {};
  }
}

const packIcons = flattenCollections([
  isoflowIsopack,
  awsIsopack,
  azureIsopack,
  gcpIsopack,
  kubernetesIsopack,
]);

export function App() {
  const [data, setData] = useState<IsoflowInitialData | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetch(topologyUrl)
      .then(async (response) => {
        if (!response.ok) {
          throw new Error(`${response.status} ${response.statusText}`);
        }

        return (await response.json()) as IsoflowInitialData;
      })
      .then((document) => {
        if (cancelled) {
          return;
        }

        setData(
          applyLayoutOverrides(
            { ...document, icons: mergeIcons(packIcons, document.icons ?? []) },
            readSavedLayout(),
          ),
        );
      })
      .catch((cause: unknown) => {
        if (!cancelled) {
          setError(cause instanceof Error ? cause.message : String(cause));
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  if (error) {
    return (
      <main style={{ padding: 24 }}>
        <h1>Could not load the topology</h1>
        <p>
          Tried <code>{topologyUrl}</code>: {error}
        </p>
        <p>
          Run <code>aspire do topology</code> in the AppHost, then copy
          <code> topology.isoflow.json</code> into <code>public/</code>.
        </p>
      </main>
    );
  }

  if (!data) {
    return <main style={{ padding: 24 }}>Loading…</main>;
  }

  return (
    <Isoflow
      initialData={data}
      editorMode="EDITABLE"
      onModelUpdate={(model: IsoflowInitialData) => {
        // Isoflow reports every model change. Persisting only the positions keeps the generated
        // topology authoritative for what exists, and the human file authoritative for placement.
        window.localStorage.setItem(
          layoutStorageKey,
          JSON.stringify(extractLayoutOverrides(model), null, 2),
        );
      }}
    />
  );
}
