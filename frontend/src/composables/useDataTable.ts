// composables/useDataTable.ts  (updated)
import { getCoreRowModel, useVueTable, type ColumnDef } from '@tanstack/vue-table';
import { reactive, ref, watch, type WatchStopHandle } from 'vue';

export type PageState = { pageIndex: number; pageSize: number };
export type FetchParams<TQuery> = { page: number; pageSize: number; query?: TQuery };
export type FetchResult<TItem> = {
  items: TItem[];
  totalCount: number;
  totalPages: number;
  page?: number;
  pageSize?: number;
};

export type ServerFetcher<TItem, TQuery> = (
  params: FetchParams<TQuery>,
) => Promise<FetchResult<TItem>>;

/**
 * Unified server-side table + pagination.
 */
export function useDataTable<TItem, TQuery = unknown>(
  columns: ColumnDef<TItem>[],
  fetcher: ServerFetcher<TItem, TQuery>,
  /** Reactive query – can be a plain object, a computed, or a ref */
  reactiveQuery: { readonly value?: TQuery },
  initialPage: PageState = { pageIndex: 0, pageSize: 25 },
  resolveRowId?: (row: TItem, index: number) => string,
) {
  const pagination = reactive<PageState>({ ...initialPage });
  const rows = ref<TItem[]>([]);
  const totalCount = ref(0);
  const totalPages = ref(0);
  const loading = ref(false);

  let reqSeq = 0;
  let stopWatch: WatchStopHandle | null = null;

  const fetchNow = async () => {
    loading.value = true;
    const seq = ++reqSeq;

    try {
      const page = pagination.pageIndex + 1;
      const res = await fetcher({
        page,
        pageSize: pagination.pageSize,
        query: reactiveQuery.value,
      });

      if (seq !== reqSeq) return; // stale response

      rows.value = res.items;
      totalCount.value = res.totalCount;
      totalPages.value = res.totalPages;
    } finally {
      loading.value = false;
    }
  };

  // -----------------------------------------------------------------
  // 1. Watch *only* pagination + the *external* query object
  // -----------------------------------------------------------------
  stopWatch = watch(
    [() => pagination.pageIndex, () => pagination.pageSize, () => reactiveQuery.value],
    fetchNow,
    { immediate: true }, // do ONE initial fetch
  );

  // Build TanStack table
  const table = useVueTable<TItem>({
    get data() {
      return rows.value as unknown as TItem[];
    },
    columns,
    get pageCount() {
      return totalPages.value;
    },
    state: { pagination },
    manualPagination: true,
    onPaginationChange: updater => {
      if (typeof updater === 'function') Object.assign(pagination, updater(pagination));
      else Object.assign(pagination, updater);
    },
    getCoreRowModel: getCoreRowModel(),
    getRowId: (row, index) => {
      if (resolveRowId) return resolveRowId(row as TItem, index);
      const id = (row as any).id;
      return id != null ? String(id) : String(index);
    },
  });

  // -----------------------------------------------------------------
  // Helpers – reset page when query changes
  // -----------------------------------------------------------------
  const setQuery = (q: TQuery | undefined) => {
    // The caller must mutate the *external* reactive object.
    // We only reset the page here.
    pagination.pageIndex = 0;
  };

  const setPageSize = (size: number) => {
    pagination.pageSize = size;
    pagination.pageIndex = 0;
  };

  // -----------------------------------------------------------------
  // Cleanup
  // -----------------------------------------------------------------
  const destroy = () => stopWatch?.();

  return {
    table,
    rows,
    loading,
    totalCount,
    totalPages,
    pagination,
    setQuery,
    setPageSize,
    fetchNow,
    destroy,
  };
}
