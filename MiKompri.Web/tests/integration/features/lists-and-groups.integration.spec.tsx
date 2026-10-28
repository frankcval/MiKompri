import { describe, expect, it, vi } from 'vitest'
import { createGroup } from '../../../src/features/groups/api/groupsApi'
import { createPersonalList } from '../../../src/features/shopping-lists/api/shoppingListsApi'

describe('lists and groups integration', () => {
  it('crea grupo y lista personal con token', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ id: 'g-1', name: 'Casa' }), {
          status: 201,
          headers: { 'Content-Type': 'application/json' },
        }),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify('l-1'), {
          status: 201,
          headers: { 'Content-Type': 'application/json' },
        }),
      )

    const group = await createGroup('http://localhost:5001', 'token-1', { name: 'Casa' })
    const listId = await createPersonalList('http://localhost:5002', 'token-1', { name: 'Semana' })

    expect(group.id).toBe('g-1')
    expect(listId).toBe('l-1')
    expect(fetchMock).toHaveBeenCalledTimes(2)

    fetchMock.mockRestore()
  })

  it('propaga conflicto 409 en creación', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(new Response('{}', { status: 409 }))

    await expect(createGroup('http://localhost:5001', 'token-1', { name: 'Casa' })).rejects.toBeDefined()

    fetchMock.mockRestore()
  })
})
