export type MyProfileDto = {
  id: string
  displayName: string
  email: string
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
    const error = new Error(`profile_request_failed_${response.status}`) as Error & { status?: number }
    error.status = response.status
    throw error
  }

  return (await response.json()) as MyProfileDto
}
