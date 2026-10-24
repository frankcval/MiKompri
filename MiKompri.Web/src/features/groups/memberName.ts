import type { GroupMember } from './api/groupsApi'

export function memberName(members: GroupMember[], userId: string): string {
  const index = members.findIndex((member) => member.userId === userId)
  if (index < 0) return 'Miembro anterior o no disponible'
  const label = (member: GroupMember) => member.displayName?.trim() || member.email?.trim() || ''
  const name = label(members[index])
  if (!name) return `Miembro del grupo ${index + 1}`
  return members.filter((member) => label(member) === name).length > 1 ? `${name} (${index + 1})` : name
}
