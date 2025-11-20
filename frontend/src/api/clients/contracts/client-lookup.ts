export interface ClientCategoryResponse {
  id: string;
  name: string;
}

export interface ClientTypeResponse {
  id: string;
  name: string;
  description: string;
  categoryId: string;
}

export interface ListClientLookupsResponse {
  categories: ClientCategoryResponse[];
  types: ClientTypeResponse[];
}
