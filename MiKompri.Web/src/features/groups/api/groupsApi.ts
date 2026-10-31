import { sessionFetch } from '../../../shared/api/request'
import { responseError } from '../../../shared/api/request'
export type GroupSummary = {
  id: string
  name: string
  myRole?: string
}

export type GroupMember = {
  userId: string
  displayName?: string
  email?: string | null
  role: 'Member' | 'Admin' | 'Owner' | string
}

export type CreateGroupInput = {
  name: string
}

export type AddGroupMemberInput = {
  userId: string
  role: 'Member' | 'Admin'
}

async function parseError(response: Response): Promise<Error & { status?: number }> {
  return responseError(response)
}

export async function getMyGroups(usersApiBaseUrl: string, accessToken: string): Promise<GroupSummary[]> {
  const response = await sessionFetch(`${usersApiBaseUrl}/api/v1/groups`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as GroupSummary[]
}

export async function createGroup(
  usersApiBaseUrl: string,
  accessToken: string,
  input: CreateGroupInput,
): Promise<GroupSummary> {
  const response = await sessionFetch(`${usersApiBaseUrl}/api/v1/groups`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(input),
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as GroupSummary
}

export async function getGroupMembers(
  usersApiBaseUrl: string,
  accessToken: string,
  groupId: string,
): Promise<GroupMember[]> {
  const response = await sessionFetch(`${usersApiBaseUrl}/api/v1/groups/${groupId}/members`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as GroupMember[]
}

export async function addGroupMember(
  usersApiBaseUrl: string,
  accessToken: string,
  groupId: string,
  input: AddGroupMemberInput,
): Promise<GroupMember> {
  const response = await sessionFetch(`${usersApiBaseUrl}/api/v1/groups/${groupId}/members`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(input),
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  return (await response.json()) as GroupMember
}
