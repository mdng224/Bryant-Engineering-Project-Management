// src/api/contacts/routes.ts

export const ContactsRoutes = Object.freeze({
  add: '/contacts',
  getDetails: (id: string) => `/contacts/${id}`,
  list: '/contacts',
  restore: (id: string) => `/contacts/${id}/restore`, // POST - restore contact
} as const);
