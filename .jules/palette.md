## 2024-05-23 - Custom Window Controls Accessibility
**Learning:** Custom window chrome (title bars) often neglects accessibility basics. While replacing the standard OS chrome allows for beautiful design, it strips away native accessibility features like tooltips and screen reader names for Minimize/Maximize/Close buttons.
**Action:** Always verify that custom title bar buttons have explicit `ToolTip` and `AutomationProperties.Name` (or `aria-label`) attributes to match the native OS experience.

## 2024-05-24 - Keyboard Focus in Custom Styles
**Learning:** Completely replacing ControlTemplates for buttons (to achieve custom styling) removes default focus indicators (`FocusVisualStyle`), rendering the UI inaccessible to keyboard users unless explicitly re-added.
**Action:** Always add `IsKeyboardFocused` triggers to custom ControlTemplates or define a custom `FocusVisualStyle` to ensure keyboard navigation is visible.

## 2024-05-25 - Combined Selection and Focus States
**Learning:** When styling list items, IsSelected and IsKeyboardFocused can occur simultaneously. A simple focus trigger might be overridden by the selection style or lack sufficient contrast against the selection background.
**Action:** Use MultiTrigger to define a specific style for items that are both selected and focused (e.g., white border on blue background) to ensure the focus indicator is always visible.
