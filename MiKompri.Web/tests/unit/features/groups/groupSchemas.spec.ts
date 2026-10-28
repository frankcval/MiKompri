import { describe, expect, it } from 'vitest'

type GroupForm = { name: string }

type GroupMemberForm = { userId: string; role: 'Member' | 'Admin' | string }

function validateGroup(form: GroupForm) {
  const errors: string[] = []
  if (!form.name || form.name.trim().length < 3) {
    errors.push('name')
  }
  return { success: errors.length === 0, errors }
}

function validateGroupMember(form: GroupMemberForm) {
  const errors: string[] = []
  if (!form.userId || form.userId.trim().length === 0) {
    errors.push('userId')
  }
  if (form.role !== 'Member' && form.role !== 'Admin') {
    errors.push('role')
  }
  return { success: errors.length === 0, errors }
}

describe('groupSchemas', () => {
  it('valida nombre mínimo para crear grupo', () => {
    const result = validateGroup({ name: 'ab' })
    expect(result.success).toBe(false)
    expect(result.errors).toContain('name')
  })

  it('acepta rol Member/Admin para membresías', () => {
    expect(validateGroupMember({ userId: 'u-1', role: 'Member' }).success).toBe(true)
    expect(validateGroupMember({ userId: 'u-1', role: 'Admin' }).success).toBe(true)
    expect(validateGroupMember({ userId: 'u-1', role: 'Owner' }).success).toBe(false)
  })
})
