import '../../../setup/jsdom-dialogs'
import React from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { act, cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { appTheme } from '../../../../src/shared/ui/theme'
import { CreateListDialog } from '../../../../src/features/shopping-lists/components/CreateListDialog'

afterEach(cleanup)
describe('creación breve de lista', () => {
  it('rechaza un nombre de espacios sin enviar una escritura', async () => {
    const create = vi.fn()
    const user = userEvent.setup()
    render(<FluentProvider theme={appTheme}><CreateListDialog title="Nueva lista" onCreate={create} /></FluentProvider>)
    await user.click(screen.getByRole('button', { name: 'Crear lista' }))
    const dialog = within(await screen.findByRole('dialog'))
    await user.type(dialog.getByRole('textbox', { name: 'Nombre de lista' }), '   ')
    await user.click(dialog.getByRole('button', { name: 'Crear lista' }))
    expect(await dialog.findByRole('alert')).toHaveTextContent('Escribe un nombre')
    expect(create).not.toHaveBeenCalled()
  })
  it('conserva nombre ante error y permite reintentar con el nombre recortado', async () => {
    const create = vi.fn().mockRejectedValueOnce(new Error('No disponible')).mockResolvedValueOnce(undefined)
    const user = userEvent.setup()
    render(<FluentProvider theme={appTheme}><CreateListDialog title="Nueva lista" onCreate={create} /></FluentProvider>)
    await user.click(screen.getByRole('button', { name: 'Crear lista' }))
    const dialog = within(await screen.findByRole('dialog'))
    await user.type(dialog.getByRole('textbox', { name: 'Nombre de lista' }), ' Compra ')
    await user.click(dialog.getByRole('button', { name: 'Crear lista' }))
    await dialog.findByText('No disponible')
    expect(dialog.getByRole('textbox', { name: 'Nombre de lista' })).toHaveValue(' Compra ')
    await user.click(dialog.getByRole('button', { name: 'Crear lista' }))
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
    expect(create).toHaveBeenLastCalledWith('Compra')
  })
  it('bloquea duplicados y cierre mientras espera la escritura', async () => {
    let release!: () => void
    const create = vi.fn(() => new Promise<void>(resolve => { release = resolve }))
    const user = userEvent.setup()
    render(<FluentProvider theme={appTheme}><CreateListDialog title="Nueva lista" onCreate={create} /></FluentProvider>)
    await user.click(screen.getByRole('button', { name: 'Crear lista' }))
    const dialog = within(await screen.findByRole('dialog'))
    await user.type(dialog.getByRole('textbox', { name: 'Nombre de lista' }), 'Compra')
    const submit = dialog.getByRole('button', { name: 'Crear lista' })
    await user.click(submit)
    fireEvent.submit(submit.closest('form')!)
    await user.keyboard('{Escape}')
    expect(create).toHaveBeenCalledTimes(1)
    expect(dialog.getByRole('button', { name: 'Cancelar' })).toBeDisabled()
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    await act(async () => release())
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
  })
})
