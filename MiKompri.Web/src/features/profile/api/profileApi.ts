import { request } from '../../../shared/api/request'

export type MyProfileDto = {
  id: string
  displayName: string
  email?: string | null
  username?: string | null
  givenName?: string | null
  familyName?: string | null
  preferredLanguage?: string | null
  defaultListSort?: string
  profileVersion?: number
}
export type ProfileFields = {
  displayName: string; username: string | null; givenName: string | null; familyName: string | null
  preferredLanguage: string | null; defaultListSort: string
}
export function syncMyProfile(apiBaseUrl: string, token: string): Promise<MyProfileDto> {
  return request(apiBaseUrl + '/api/v1/users/me/sync', token, 'POST')
}
export function fetchMyProfile(apiBaseUrl: string, token: string): Promise<MyProfileDto> {
  return request(apiBaseUrl + '/api/v1/users/me', token)
}
export function updateMyProfile(apiBaseUrl: string, token: string, fields: ProfileFields, version: number): Promise<MyProfileDto> {
  return request(apiBaseUrl + '/api/v1/users/me', token, 'PUT', fields, { 'If-Match': '"' + version + '"' })
}
