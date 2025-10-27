// composables/useDateFormat.ts
export function useDateFormat() {
  const fmt = new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
  const formatUtc = (iso: string | null) => (iso ? fmt.format(new Date(iso)) : '—');

  return { formatUtc };
}
