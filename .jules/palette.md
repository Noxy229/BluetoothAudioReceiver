## 2024-05-23 - Custom Window Controls Accessibility
**Learning:** Custom window chrome (title bars) often neglects accessibility basics. While replacing the standard OS chrome allows for beautiful design, it strips away native accessibility features like tooltips and screen reader names for Minimize/Maximize/Close buttons.
**Action:** Always verify that custom title bar buttons have explicit `ToolTip` and `AutomationProperties.Name` (or `aria-label`) attributes to match the native OS experience.

## 2024-05-24 - Keyboard Focus in Custom Styles
**Learning:** Completely replacing ControlTemplates for buttons (to achieve custom styling) removes default focus indicators (`FocusVisualStyle`), rendering the UI inaccessible to keyboard users unless explicitly re-added.
**Action:** Always add `IsKeyboardFocused` triggers to custom ControlTemplates or define a custom `FocusVisualStyle` to ensure keyboard navigation is visible.
