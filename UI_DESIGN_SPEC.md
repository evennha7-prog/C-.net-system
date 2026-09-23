# PCCFPI Store - UI Design System

## Overview
Modern POS & Store Management System with a cohesive design language built on Windows Forms (.NET Framework 4.8).

---

## Design Tokens

### Color System
| Token | Light Mode | Dark Mode | Usage |
|-------|-----------|-----------|-------|
| Background | `#F5F7FB` | `#111625` | Main background |
| Card Background | `#FFFFFF` | `#1B2236` | Cards, panels |
| Sidebar Background | `#FFFFFF` | `#181E30` | Sidebar panel |
| Border Color | `#EBF0F7` | `#28324E` | Borders, dividers |
| Text Primary | `#1E2238` | `#F5F7FF` | Headings, primary text |
| Text Secondary | `#8C93A4` | `#9AABB9` | Subtitles, labels |
| Text Muted | `#A3AE C2` | `#687594` | Disabled, hints |
| Accent Blue | `#3062F9` | `#3E72FF` | Primary actions, links |
| Accent Blue Hover | `#3C6EFF` | `#4B7DFF` | Hover state |
| Success Green | `#10B981` | `#10B981` | Success states |
| Warning Yellow | `#F59E0B` | `#F59E0B` | Warnings, alerts |
| Danger Red | `#EF4444` | `#EF4444` | Errors, destructive |

### Typography
- **Font Family**: `Roboto` (English), `Khmer OS Battambang` (Khmer)
- **Font Sizes**:
  - Display: 24px / Bold
  - H1: 18px / Bold
  - H2: 15px / Bold
  - Body: 9.5px / Regular
  - Small: 8px / Regular
  - Caption: 7.5px / Bold

### Spacing System
- Base unit: `4px`
- Scale: 4, 8, 12, 16, 20, 24, 32, 48
- Gap between sections: `14-16px`
- Card padding: `12px`
- Border radius: `8-12px` (cards), `6px` (buttons), `16px` (KPI cards)

---

## Layout Architecture

### Main Frame (Form1)
```
┌──────────────────────────────────────────────────┐
│  [Sidebar 236px]  [TopHeader 64px]               │
│                                                   │
│  ┌──────────────────────────────────────────┐     │
│  │                                          │     │
│  │         Views Container                  │     │
│  │         (Dashboard / Products / etc.)    │     │
│  │                                          │     │
│  └──────────────────────────────────────────┘     │
└──────────────────────────────────────────────────┘
```

### Sidebar Navigation (236px wide)
- **Logo**: Centered PCCFPI crest (40x40px) at top
- **Brand**: "PCCFPI STORE" centered below logo
- **Main Menu Items** (38px height, 3px gap):
  - Dashboard, Products, Categories, Sale (expandable), Orders, Customers, Reports, Users
- **System Menu** (34px height):
  - Appearance, Settings, Logout
- **Active state**: Accent blue background + left accent bar (3px)
- **Hover state**: Subtle background highlight
- **Section separator**: Divider line between Main and System

### Top Header (64px height)
- **Left**: Store status pill ("🏪 PCCFPI STORE ● Live")
- **Center**: User greeting ("Welcome back, [Name] [Role]")
- **Right**: Search → Language → Theme → Bell → Avatar
- **Avatar**: 34x34px with gradient ring + online indicator

---

## Component Specifications

### KPI Cards (120x120px)
- Rounded corners: 12px
- Icon box: 42x42px rounded square
- Title: 8.5px / Secondary color
- Value: 13px / Bold / Primary
- Badge pill: Top-right, accent colored
- Hover: Accent blue border

### Modern Buttons
- **Primary**: Blue background, white text, 8px radius
- **Secondary**: Card background, border on hover, 8px radius
- **Danger**: Red background, white text
- **Success**: Green background, white text
- **Ghost**: Transparent, colored on hover
- Size: 110-185px wide, 34-38px height
- Icon: 16x16px, 8px gap from text

### Modern Search Box
- Rounded: 9px
- Border: 1px, turns accent blue when focused
- Icon: Search 16x16px, 11px from left
- Placeholder: Cue banner text
- Header style: White with subtle border
- Height: 34px

### Data Tables
- Header: 44px height, 9px Bold, secondary color
- Row height: 44px
- Alternating rows: Subtle color difference
- Status pills: Rounded badges with colored dot
- Action buttons: Edit (blue), Delete (red), 26x76px each
- Grid lines: Single horizontal, subtle
- Empty state: Centered, muted text

### Rounded Panels
- Background: Card background color
- Border: 1px, border color
- Border radius: 12-16px
- Padding: 12px
- Theme-aware colors

### Management Stat Cards (78px height)
- Icon box: 42x42px rounded square
- Title: 8.5px / Secondary
- Value: 13px / Bold / Primary
- Badge: Top-right pill
- Layout: 4 cards per row, 12px gap

---

## View Specifications

### Dashboard
```
┌─────────────────────────────────────┐
│  [KPI Card 1] [KPI 2] [KPI 3] [KPI]│ 120px
│                                     │
│  [Bar Chart (40%)] [Spline (60%)]   │ 260px
│                                     │
│  [Latest Posts] [Recent Comments]   │ 240px
└─────────────────────────────────────┘
```

### Products View
```
┌─────────────────────────────────────┐
│  Header: Title + Add Button         │ 48px
│  [Metric Cards x4]                  │ 78px
│  [Toolbar: Search | Category | Btn] │ 56px
│  [Data Grid]                        │ Remaining
└─────────────────────────────────────┘
```

### Orders View
```
┌─────────────────────────────────────┐
│  Filter Tabs: All | Pending | ...   │ 34px
│  [Refresh Button]                   │
│  [Data Grid]                        │ Remaining
└─────────────────────────────────────┘
```

---

## Navigation Structure

### Main Navigation
| Menu Item | Icon | Badge | Route |
|-----------|------|-------|-------|
| Dashboard | dashboard | - | _dashboardView |
| Products | products | - | _productsView |
| Categories | folder | - | _categoriesView |
| Sale | sale | - | _saleParentBtn (expandable) |
| └ POS | pos | - | _posView |
| └ List Sale | listsale | - | _salesListView |
| Orders | order | - | _ordersView |
| Customers | customer | - | _customersView |
| Reports | report | "7" | _reportsView |
| Users | users | - | _usersView |

### System
| Menu Item | Icon | Route |
|-----------|------|-------|
| Appearance | appearance | _appearanceView |
| Settings | settings | _settingsView |
| Logout | logout | HandleLogout |

---

## Theme System

### Light Mode
- Background: `#F5F7FB`
- Cards: `#FFFFFF`
- Sidebar: `#FFFFFF`
- Borders: `#EBF0F7`
- Text Primary: `#1E2238`
- Text Secondary: `#8C93A4`
- Accent Blue: `#3062F9`

### Dark Mode
- Background: `#111625`
- Cards: `#1B2236`
- Sidebar: `#181E30`
- Borders: `#28324E`
- Text Primary: `#F5F7FF`
- Text Secondary: `#9AABB9`
- Accent Blue: `#3E72FF`

### Theme Toggle
- Toggle switch in top header
- Smooth transition on all controls
- Persists preference

---

## Icon System

All icons are vector-drawn using `GraphicsHelper.DrawIcon()`. Available icons:

**Navigation**: dashboard, products, folder, sale, pos, listsale, order, customer, users, report, appearance, settings, logout

**Actions**: plus, refresh, search, edit, delete, check, filter, download, upload, share

**Status**: bell, notification, mail, lock, shield, star, dollar, phone

**UI**: sun, moon, cheverondown, chevronup, chevronright, globe, menu, dots, wave, fire

---

## Interaction Design

### Hover States
- Buttons: Scale effect with color change
- Nav items: Background highlight + text color change
- Table rows: Subtle background change
- Cards: Border color change to accent blue
- Icons: Color shift

### Active States
- Buttons: Pressed effect (darker shade)
- Nav items: Accent blue background + left bar indicator
- Checkboxes: Filled state

### Transitions
- All state changes: 150ms ease-out
- Theme switching: Smooth color interpolation
- View transitions: Fade in/out

### Responsive Behavior
- Grid auto-sizes based on container width
- KPI cards wrap on narrow screens
- Table columns fill available space
- Search box adjusts width dynamically

---

## Accessibility

- High contrast mode support
- Keyboard navigation (Tab, Enter, Escape)
- Screen reader compatible labels
- Font size scales with system settings
- Color-blind friendly status indicators

---

## Implementation Notes

1. **All custom controls** use double-buffering for smooth rendering
2. **ThemeManager** handles all color changes with event-based updates
3. **TranslationManager** supports English and Khmer with font auto-detection
4. **DataGridViewStyleHelper** provides consistent table styling with pill badges
5. **ModernButton** supports 5 types with icon and translation support
6. **FontHelper** resolves Roboto or Khmer fonts based on text content

---

## File Structure
```
assignment_code/
├── UI/
│   ├── Controls/           # Custom controls
│   │   ├── ModernButton.cs
│   │   ├── ModernSearchBox.cs
│   │   ├── ModernComboBox.cs
│   │   ├── NavItemButton.cs
│   │   ├── SidebarControl.cs
│   │   ├── TopHeaderControl.cs
│   │   ├── KpiCardControl.cs
│   │   ├── ManagementStatCard.cs
│   │   ├── RoundedPanel.cs
│   │   └── ...
│   ├── Views/              # View modules
│   │   ├── DashboardView.cs
│   │   ├── ProductsView.cs
│   │   ├── OrdersView.cs
│   │   ├── CustomersView.cs
│   │   └── ...
│   ├── ThemeManager.cs     # Color tokens
│   ├── FontHelper.cs       # Typography
│   ├── GraphicsHelper.cs   # Icons & drawing
│   └── DataGridViewStyleHelper.cs
├── Models/                 # Data models
├── Services/              # Business logic
└── Form1.cs               # Main frame
```
