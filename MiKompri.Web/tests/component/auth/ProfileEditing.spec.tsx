import React from 'react'
import { afterEach, expect, it, vi } from 'vitest'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { ProfilePage } from '../../../src/features/profile/pages/ProfilePage'

const { fetchProfile, saveProfile } = vi.hoisted(() => ({ fetchProfile: vi.fn(), saveProfile: vi.fn() }))
vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [{ homeAccountId: 'A', tenantId: '00000000-0000-0000-0000-000000000000' }], instance: {} }) }))
vi.mock('../../../src/auth/session/authSessionService', () => ({ createAuthSessionService: () => ({ getAccessToken: async () => 'test-only' }) }))
vi.mock('../../../src/features/profile/api/profileApi', () => ({ fetchMyProfile: fetchProfile, updateMyProfile: saveProfile }))
afterEach(cleanup)
it('edits owned fields with a version and permits an explicit clear, without methods or photo controls', async () => {
  const profile = { id: 'user-A', displayName: 'Owned', username: 'owned_user', givenName: 'Given', familyName: null, preferredLanguage: 'es', defaultListSort: 'newest', profileVersion: 2 }
  fetchProfile.mockResolvedValue(profile)
  saveProfile.mockResolvedValue({ ...profile, displayName: 'Mine', givenName: null, profileVersion: 3 })
  render(<QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter><ProfilePage /></MemoryRouter></QueryClientProvider>)
  const name = await screen.findByRole('textbox', { name: 'Nombre visible' })
  fireEvent.change(name, { target: { value: 'Mine' } })
  fireEvent.change(screen.getByRole('textbox', { name: 'Nombre' }), { target: { value: '' } })
  fireEvent.click(screen.getByRole('button', { name: 'Guardar perfil' }))
  await waitFor(() => expect(saveProfile).toHaveBeenCalledWith(expect.any(String), 'test-only', expect.objectContaining({ displayName: 'Mine', givenName: null }), 2))
  expect(screen.queryByRole('button', { name: /Vincular|Desvincular|Subir foto|Añadir método/i })).not.toBeInTheDocument()
})
