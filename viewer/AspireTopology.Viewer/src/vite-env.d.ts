/// <reference types="vite/client" />

declare module 'isoflow';
declare module '@isoflow/isopacks/dist/isoflow';
declare module '@isoflow/isopacks/dist/aws';
declare module '@isoflow/isopacks/dist/azure';
declare module '@isoflow/isopacks/dist/gcp';
declare module '@isoflow/isopacks/dist/kubernetes';
declare module '@isoflow/isopacks/dist/utils';

interface ImportMetaEnv {
  readonly VITE_TOPOLOGY_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
