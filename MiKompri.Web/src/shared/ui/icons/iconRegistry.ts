import type { IconType } from 'react-icons'
import {
  PiClipboardText, PiClipboardTextFill, PiClockCounterClockwise, PiClockCounterClockwiseFill,
  PiUsersThree, PiUsersThreeFill, PiBasket, PiBasketFill, PiStorefront,
  PiUserCircle, PiUser,
  PiPlus, PiMagnifyingGlass, PiPencilSimple, PiTrash, PiDotsThree, PiArrowLeft, PiX,
  PiInfo, PiWarningCircle, PiList, PiSignOut,
} from 'react-icons/pi'

// App-owned symbols only. Fluent controls and provider/brand logos keep their glyphs.
export const iconRegistry = {
  lists: [PiClipboardText, PiClipboardTextFill], history: [PiClockCounterClockwise, PiClockCounterClockwiseFill],
  groups: [PiUsersThree, PiUsersThreeFill], products: [PiBasket, PiBasketFill], markets: [PiStorefront, PiStorefront],
  account: [PiUserCircle, PiUserCircle], 'personal-context': [PiUser, PiUser],
  add: [PiPlus, PiPlus], search: [PiMagnifyingGlass, PiMagnifyingGlass],
  edit: [PiPencilSimple, PiPencilSimple], delete: [PiTrash, PiTrash], more: [PiDotsThree, PiDotsThree],
  back: [PiArrowLeft, PiArrowLeft], close: [PiX, PiX],
  information: [PiInfo, PiInfo], error: [PiWarningCircle, PiWarningCircle], menu: [PiList, PiList], logout: [PiSignOut, PiSignOut],
} satisfies Record<string, readonly [IconType, IconType]>
export type IconName = keyof typeof iconRegistry
