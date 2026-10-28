import { request, responseError } from '../../../shared/api/request'

export type MyProfileDto = {
  id: string
  displayName: string
  email: string
}

export async function syncMyProfile(apiBaseUrl: string, token: string): Promise<MyProfileDto> {
  return request<MyProfileDto>(`${apiBaseUrl}/api/v1/users/me/sync`, token, 'POST')
}

export async function fetchMyProfile(apiBaseUrl: string, accessToken: string): Promise<MyProfileDto> {
  const response = await fetch(`${apiBaseUrl}/api/v1/users/me`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
  })

  if (!response.ok) {
    throw await responseError(response)
  }

  return (await response.json()) as MyProfileDto
}
