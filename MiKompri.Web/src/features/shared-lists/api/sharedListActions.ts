import { request } from '../../../shared/api/request'
export const updateSharedList = (base: string, token: string, id: string, name: string) => request<void>(`${base}/api/v1/shared-lists/${id}`, token, 'PATCH', { name })
export const removeSharedList = (base: string, token: string, id: string) => request<void>(`${base}/api/v1/shared-lists/${id}`, token, 'DELETE')
export const closeSharedList = (base: string, token: string, id: string) => request<void>(`${base}/api/v1/shared-lists/${id}/close`, token, 'PATCH')
export const updateSharedItem = (base: string, token: string, id: string, itemId: string, quantity: number, estimatedPrice: number | null) => request<void>(`${base}/api/v1/shared-lists/${id}/items/${itemId}`, token, 'PATCH', { quantity, estimatedPrice })
export const removeSharedItem = (base: string, token: string, id: string, itemId: string) => request<void>(`${base}/api/v1/shared-lists/${id}/items/${itemId}`, token, 'DELETE')
