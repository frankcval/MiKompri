import { request } from '../../../shared/api/request'

export type MyProfileDto = {
  id: string
  displayName: string
  email: string
}

export async function syncMyProfile(apiBaseUrl: string, token: string): Promise<MyProfileDto> {
  return request<MyProfileDto>(`${apiBaseUrl}/api/v1/users/me/sync`, token, 'POST')
}

export async function fetchMyProfile(apiBaseUrl: string, accessToken: string): Promise<MyProfileDto> {
  return request<MyProfileDto>(apiBaseUrl + '/api/v1/users/me', accessToken)
}
