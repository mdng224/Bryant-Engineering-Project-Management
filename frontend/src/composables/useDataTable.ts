// composables/useDataTable.ts
import { getCoreRowModel, useVueTable, type ColumnDef } from '@tanstack/vue-table';
import { reactive, ref, watchEffect } from 'vue';

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
 * Unifies server fetch + TanStack table.
 */
export function useDataTable<TItem, TQuery = unknown>(
  columns: ColumnDef<TItem>[],
  fetcher: ServerFetcher<TItem, TQuery>,
  initialQuery?: TQuery,
  initialPage: PageState = { pageIndex: 0, pageSize: 25 },
  resolveRowId?: (row: TItem, index: number) => string,
) {
  const pagination = reactive<PageState>({ ...initialPage });
  const query = ref<TQuery | undefined>(initialQuery);
  const rows = ref<TItem[]>([]);
  const totalCount = ref(0);
  const totalPages = ref(0);
  const loading = ref(false);

  let reqSeq = 0;

  const fetchNow = async () => {
    loading.value = true;
    const seq = ++reqSeq;

    try {
      const page = pagination.pageIndex + 1;
      const res = await fetcher({ page, pageSize: pagination.pageSize, query: query.value });

      if (seq !== reqSeq) return; // stale guard

      rows.value = res.items;
      totalCount.value = res.totalCount;
      totalPages.value = res.totalPages;
      pagination.pageIndex = Math.max(0, (res.page ?? page) - 1);
      pagination.pageSize = res.pageSize ?? pagination.pageSize;
    } finally {
      loading.value = false;
    }
  };

  // Build table instance
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
    onPaginationChange: (updater: unknown) => {
      if (typeof updater === 'function') Object.assign(pagination, updater(pagination));
      else Object.assign(pagination, updater);
    },
    getCoreRowModel: getCoreRowModel(),
    getRowId: (row, index) => {
      if (resolveRowId) return resolveRowId(row as TItem, index);
      const candidate = (row as unknown as { id?: string | number }).id;
      return String(candidate ?? index);
    },
  });

  // Auto fetch on page/query change
  watchEffect(fetchNow);

  // Helpers
  const setQuery = (q: TQuery | undefined) => {
    query.value = q;
    pagination.pageIndex = 0;
  };
  const setPageSize = (size: number) => {
    pagination.pageSize = size;
    pagination.pageIndex = 0;
  };

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
  };
}
