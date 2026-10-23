import React from 'react'
import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { MainLayout } from '../../../src/app/layout/MainLayout'

describe('accessibility smoke', () => {
  it('tiene landmark main y navegación con labels visibles', () => {
    render(
      <MemoryRouter>
        <MainLayout>
          <h1>Contenido</h1>
        </MainLayout>
      </MemoryRouter>,
    )

    expect(screen.getByRole('main')).toBeInTheDocument()
    expect(screen.getByRole('navigation')).toBeInTheDocument()
  })
})
