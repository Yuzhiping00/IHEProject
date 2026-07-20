import type { BundleEntry } from "./BundleEntry";

export interface Bundle<T> {
    resourceType : "Bundle";
    type: string;
    total: number;
    entry?: BundleEntry<T>[];
}