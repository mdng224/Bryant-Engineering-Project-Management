export interface ClientCategoryDto {
  id: string;
  name: string;
}

export interface ClientTypeDto {
  id: string;
  name: string;
  description: string;
  categoryId: string;
}

export interface GetClientLookupsResponse {
  categories: ClientCategoryDto[];
  types: ClientTypeDto[];
}
