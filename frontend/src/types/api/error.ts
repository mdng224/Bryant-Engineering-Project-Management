export type ApiErrorResponse = {
  code?: string;
  message?: string; // for non-ProblemDetails fallbacks
  detail?: string; // ProblemDetails.detail
  error?: string; // optional legacy shape some libs use
  errors?: Record<string, string[] | string>;
};
