import React from 'react'
import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import {
  EmptyState,
  ErrorState,
  ForbiddenState,
  LoadingState,
  ServiceUnavailableState,
  UnauthorizedState,
} from '../../../src/shared/ui/states'

describe('UiStateViews', () => {
  it('renderiza estados globales base', () => {
    render(
      <div>
        <LoadingState label="Cargando" />
        <EmptyState title="Vacío" description="Sin datos" />
        <ErrorState title="Error" description="Algo falló" />
        <UnauthorizedState />
        <ForbiddenState />
        <ServiceUnavailableState />
      </div>,
    )

    expect(screen.getByRole('progressbar')).toBeInTheDocument()
    expect(screen.getByText('Vacío')).toBeInTheDocument()
    expect(screen.getByText('Error')).toBeInTheDocument()
  })
})
