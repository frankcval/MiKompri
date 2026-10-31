import React from 'react'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, expect, it, vi } from 'vitest'
import { IconAction, IconActionLink } from '../../../src/shared/ui/icons/IconAction'
import { appTheme } from '../../../src/shared/ui/theme'
afterEach(cleanup)
it('shows supplementary help on hover without replacing its accessible name', async () => {
  const user = userEvent.setup()
  render(<FluentProvider theme={appTheme}><IconAction name="menu" label="Abrir menú" tooltip="Navegación de MiKompri" /></FluentProvider>)
  const button = screen.getByRole('button', { name: 'Abrir menú' })
  await user.hover(button)
  await waitFor(() => expect(screen.getByRole('tooltip')).toHaveTextContent('Navegación de MiKompri'))
  expect(button).toHaveAccessibleName('Abrir menú')
})
it('has its name before the tooltip, opens on keyboard focus and dismisses Escape without moving focus', async () => {
  const user = userEvent.setup();const click = vi.fn()
  render(<FluentProvider theme={appTheme}><IconAction name="edit" label="Editar Tomates" onClick={click} /></FluentProvider>)
  const button = screen.getByRole('button', { name: 'Editar Tomates' })
  await user.tab(); expect(button).toHaveFocus()
  await waitFor(() => expect(screen.getByRole('tooltip')).toBeVisible())
  await user.keyboard('{Escape}')
  await waitFor(() => expect(screen.queryByRole('tooltip')).not.toBeInTheDocument())
  expect(button).toHaveFocus(); await user.keyboard('{Enter}'); expect(click).toHaveBeenCalledOnce()
})
it('does not activate disabled or busy actions and exposes busy semantics', async () => {
  const user=userEvent.setup();const click=vi.fn()
  render(<FluentProvider theme={appTheme}><IconAction name="add" label="Añadir producto" busy onClick={click} /></FluentProvider>)
  const button=screen.getByRole('button',{name:'Añadir producto'})
  expect(button).toBeDisabled();expect(button).toHaveAttribute('aria-busy','true');await user.click(button);expect(click).not.toHaveBeenCalled()
})
it('keeps link semantics and an independent name for a navigation action', () => {
  render(<FluentProvider theme={appTheme}><MemoryRouter><IconActionLink to="/lists" name="back" label="Volver a mis listas" /></MemoryRouter></FluentProvider>)
  expect(screen.getByRole('link',{name:'Volver a mis listas'})).toHaveAttribute('href','/lists')
  expect(screen.queryByRole('button')).not.toBeInTheDocument()
})
