// Base fields shared between summary and detail views
export interface ClientBase {
  id: string;
  name: string | null;
  totalActiveProjects: number;
  totalProjects: number;
  clientCategoryId: string; // Guid as string
  clientTypeId: string; // Guid as string
  categoryName: string | null;
  typeName: string | null;
}
