// src/api/contacts/services.ts

import api from '..';
import type {
  AddContactRequest,
  GetContactDetailsResponse,
  ListContactsRequest,
  ListContactsResponse,
} from './contracts';

import { ContactsRoutes } from './routes';

const getDetails = async (id: string): Promise<GetContactDetailsResponse> => {
  const { data } = await api.get<GetContactDetailsResponse>(ContactsRoutes.getDetails(id));
  return data;
};

const list = async (params: ListContactsRequest): Promise<ListContactsResponse> => {
  const { data } = await api.get<ListContactsResponse>(ContactsRoutes.list, { params });
  return data;
};

/* ------------------------------ POST (add) ------------------------------ */
const add = async (payload: AddContactRequest): Promise<AddContactRequest> => {
  const { data } = await api.post<AddContactRequest>(ContactsRoutes.add, payload);
  return data;
};

/* ------------------------------ POST (restore) ------------------------------ */
const restore = async (id: string): Promise<void> => {
  await api.post<string>(ContactsRoutes.restore(id));
};

export const contactService = { add, getDetails, list, restore };
