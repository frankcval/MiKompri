import React from 'react'
import { cleanup, render } from '@testing-library/react'
import { afterEach, expect, it } from 'vitest'
import { AppIcon } from '../../../src/shared/ui/icons/AppIcon'
import { iconRegistry, type IconName } from '../../../src/shared/ui/icons/iconRegistry'
afterEach(cleanup)
it('renders every application meaning as a silent, non-focusable SVG at the requested size', () => {
  for (const name of Object.keys(iconRegistry) as IconName[]) {
    const view = render(<AppIcon name={name} size={24} />)
    const svg = view.container.querySelector('svg')!
    expect(svg).toHaveAttribute('width', '24'); expect(svg).toHaveAttribute('height', '24')
    expect(svg).toHaveAttribute('aria-hidden', 'true'); expect(svg).toHaveAttribute('focusable', 'false')
    expect(svg.querySelector('path')).not.toBeNull(); expect(svg).not.toHaveAttribute('tabindex')
    view.unmount()
  }
})
it('changes selected navigation glyphs without introducing a second accessible element', () => {
  const view = render(<AppIcon name="lists" />)
  const before = view.container.querySelector('path')!.getAttribute('d')
  view.rerender(<AppIcon name="lists" selected />)
  expect(view.container.querySelector('path')!.getAttribute('d')).not.toBe(before)
  expect(view.container.querySelector('svg')).toHaveAttribute('aria-hidden', 'true')
})
