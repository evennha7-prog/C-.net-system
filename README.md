# PCCFPI Store - Modern POS & Store Management System

A modern Point of Sale (POS) and Store Management desktop application built with **C#** and **Windows Forms (.NET Framework 4.8)**, featuring PostgreSQL cloud database integration, dark/light theme switching, KPI dashboards, and automated installer packaging.

---

## Key Features

- **Dashboard & Analytics:** Real-time KPI summary cards, sales spline charts, product category bar charts, and recent activity feeds.
- **Point of Sale (POS):** Fast checkout interface with barcode search, cart management, receipt generation, and discount calculations.
- **Inventory & Products Management:** Product catalog, pricing, category classification, and stock tracking.
- **Customer & Order Management:** Customer contact records, purchase history, and detailed sales transactions.
- **Role-Based Access Control:** Configurable user roles (Admin, Cashier, Manager) with secure authentication.
- **Multi-Theme Support:** Sleek modern UI supporting dynamic Light and Dark modes with custom-rendered controls.
- **Multi-Language Support:** Localized interface supporting English and Khmer.
- **Database Integration:** Cloud PostgreSQL backend with integrated database migration runner (`DatabaseMigrator`).
- **Installer & Packaging:** Automated PowerShell build scripts for producing both portable `.zip` archives and standalone `.exe` setup installers.

---

## Tech Stack

- **Language:** C# 8.0+
- **Framework:** .NET Framework 4.8 (Windows Forms)
- **Database:** PostgreSQL (via `Npgsql`)
- **JSON & Data Serialization:** `System.Text.Json`
- **IDE:** Visual Studio 2022 / 2019

---

## Project Structure

```
├── .gitignore                      # Git ignore rules for .NET / Visual Studio
├── README.md                       # Project documentation
├── assignment_code.slnx            # Visual Studio Solution file
├── build_dist.bat / ps1            # Automated build and installer packager
├── migrate.bat / ps1               # Database migration execution script
├── installer/                      # Installer setup and uninstaller source code
│   ├── SetupProgram.cs
│   └── UninstallProgram.cs
└── assignment_code/                # Main application project
    ├── assignment_code.csproj      # Project file
    ├── App.config                  # Runtime configuration and assembly bindings
    ├── Program.cs                  # Application entry point
    ├── Models/                     # Data models (Product, Order, Customer, etc.)
    ├── Services/                   # Business logic, DB migrator, EnvLoader, Translation
    ├── UI/                         # Windows Forms, Views, and Custom Controls
    └── icons/                      # Application icons and branding assets
```

---

## Getting Started

### Prerequisites
- Windows 10 (version 1903+) or Windows 11
- .NET Framework 4.8 Developer Pack or Runtime
- Visual Studio 2022 (with *.NET desktop development* workload)
- Active PostgreSQL Database instance

### Environment Configuration
1. Create a `.env` file in the `assignment_code` directory with your PostgreSQL credentials and store settings:
   ```ini
   DB_CONNECTION=pgsql
   DB_HOST=your_host
   DB_PORT=5432
   DB_DATABASE=your_database
   DB_USERNAME=your_username
   DB_PASSWORD=your_password
   ```

### Building & Running
- **Visual Studio:** Open `assignment_code.slnx` or `assignment_code\assignment_code.csproj` in Visual Studio and press **F5** or **Ctrl + F5**.
- **Database Migration:** Run `migrate.bat` or `migrate.ps1` to execute initial schema migrations and seed initial data.
- **Build Installer:** Run `build_dist.bat` to automatically build Release binaries and package the standalone setup installer.

---

## License

This project is developed for PCCFP Institute academic and educational purposes.