import type { Address } from '@/api/common';

export interface AddClientRequest {
  name: string;
  namePrefix?: string | null;
  firstName: string;
  middleName: string; // sent as empty string if not provided
  lastName: string;
  nameSuffix?: string | null;
  email?: string | null;
  phone?: string | null;
  address: Address | null;
  note?: string | null;
  clientCategoryId: string; // Guid as string
  clientTypeId: string; // Guid as string
}
