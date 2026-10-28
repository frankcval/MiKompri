import type { ReactNode } from 'react'
import {
  EmptyState,
  ErrorState,
  ForbiddenState,
  LoadingState,
  ServiceUnavailableState,
  UnauthorizedState,
} from './index'

type ViewStateKind = 'loading' | 'empty' | 'error' | 'unauthorized' | 'forbidden' | 'serviceUnavailable' | 'ready'

type ViewStateOptions = {
  state: ViewStateKind
  content: ReactNode
  emptyTitle?: string
  emptyDescription?: string
  errorTitle?: string
  errorDescription?: string
}

export function withViewState({
  state,
  content,
  emptyTitle,
  emptyDescription,
  errorTitle,
  errorDescription,
}: ViewStateOptions): ReactNode {
  switch (state) {
    case 'loading':
      return <LoadingState />
    case 'empty':
      return <EmptyState title={emptyTitle} description={emptyDescription} />
    case 'error':
      return <ErrorState title={errorTitle} description={errorDescription} />
    case 'unauthorized':
      return <UnauthorizedState />
    case 'forbidden':
      return <ForbiddenState />
    case 'serviceUnavailable':
      return <ServiceUnavailableState />
    case 'ready':
    default:
      return content
  }
}
