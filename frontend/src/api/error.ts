export type ApiErrorResponse = {
  code?: string;
  message?: string; // custom error shape
  detail?: string; // RFC 7807 ProblemDetails.detail
  title?: string; // RFC 7807 ProblemDetails.title
  error?: string; // some libs use this
  errors?: Record<string, string[] | string>; // ValidationProblemDetails
};

export function extractApiError(err: unknown, preferredField?: string): string {
  const anyErr = err as any;
  const status: number | undefined = anyErr?.response?.status;
  const data: ApiErrorResponse | undefined = anyErr?.response?.data;

  // 1) Field-specific (e.g., "email")
  if (preferredField && data?.errors && data.errors[preferredField]) {
    const val = data.errors[preferredField];
    if (Array.isArray(val) && val.length) return val[0];
    if (typeof val === 'string' && val.trim()) return val;
  }

  // 2) Any validation error
  if (data?.errors) {
    const firstKey = Object.keys(data.errors)[0];
    const val = (data.errors as any)[firstKey];
    if (Array.isArray(val) && val.length) return val[0];
    if (typeof val === 'string' && val.trim()) return val;
  }

  // 3) RFC7807 / custom shapes
  if (data?.detail && data.detail.trim()) return data.detail;
  if (data?.message && data.message.trim()) return data.message;
  if (data?.title && data.title.trim()) return data.title;
  if (data?.error && typeof data.error === 'string' && data.error.trim()) return data.error;

  // 4) Status-based friendly fallbacks
  if (status === 409) return 'An account with this email already exists.';
  if (status === 401) return 'Invalid credentials.';
  if (status === 400) return 'Please check the form and try again.';

  // 5) Axios/network
  if (typeof anyErr?.message === 'string' && anyErr.message.trim()) return anyErr.message;

  return 'An unexpected error occurred.';
}
