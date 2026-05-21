# 🚚 Smart Transport & Load Optimization System

<div align="center">

![C#](https://img.shields.io/badge/C%23-.NET%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Console%20App-1B3A5C?style=for-the-badge&logo=windows-terminal&logoColor=white)
![OOP](https://img.shields.io/badge/Paradigm-Object%20Oriented-1D7B6E?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-C55A11?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Complete-2E7D32?style=for-the-badge)

**A fully interactive console-based transport and logistics management system built in C# using all four OOP principles.**

[Features](#-features) · [Getting Started](#-getting-started) · [How It Works](#-how-it-works) · [Project Structure](#-project-structure) · [OOP Design](#-oop-design) · [Screenshots](#-screenshots)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Getting Started](#-getting-started)
- [How It Works](#-how-it-works)
- [Pricing Model](#-pricing-model)
- [User Roles](#-user-roles)
- [Project Structure](#-project-structure)
- [OOP Design](#-oop-design)
- [Data Persistence](#-data-persistence)
- [Class Diagram](#-class-diagram)
- [Built With](#-built-with)
- [Authors](#-authors)

---

## 📌 Overview

The **Smart Transport & Load Optimization System** is a console application that simulates a real-world cargo and delivery management platform. It automates vehicle selection, calculates prices dynamically, manages a role-based user system, persists all data to JSON files, and includes a load-sharing optimizer that groups small deliveries together to reduce costs and trips.

This project was developed as the final project for an **Object Oriented Programming (OOP)** course, demonstrating encapsulation, abstraction, polymorphism, and the single responsibility principle in a complete working application.

### The Problem It Solves

Small transport companies often rely on manual processes — handwritten booking logs, guesswork vehicle selection, inconsistent pricing, and no delivery priority system. This leads to:

- 📋 Lost records and no audit trail
- 🚛 Oversized vehicles assigned to small loads (wasted fuel)
- 💰 Inconsistent pricing across customers
- 🔴 Urgent deliveries stuck behind normal ones
- 🚗 Vehicles running half-empty on every trip

This system solves all five problems in one application.

---

## ✨ Features

### 👤 User Management
- Register with automatic Customer role assignment
- Secure login with hashed password storage
- Recovery code system for forgotten password reset
- Update username, change password, update recovery code
- Three role tiers: Customer, Admin, Super User

### 🚗 Vehicle Management
- Four vehicle types: Bike (50 kg), Van (500 kg), Truck (5,000 kg), Heavy Truck (20,000 kg)
- Real-time load and availability tracking
- Admin: add, update, delete vehicles
- Automatic capacity enforcement — overloading is impossible

### 📦 Booking Management
- Create bookings by entering weight, distance, and priority
- Full lifecycle: `Pending → Approved → Assigned → InTransit → Delivered`
- Timestamped status history for every booking
- Customer: view own bookings, cancel, track
- Admin: view all bookings, approve, reject, update status

### 🧠 Smart Vehicle Assignment
- **Best Fit algorithm** — automatically selects the smallest vehicle that fits the cargo
- Prevents wasteful assignment (no truck sent for a 10 kg package)
- Falls back to Pending if no vehicle is available

### 💰 Dynamic Pricing
- Formula-based pricing: `Base Rate + (Weight × Per-kg Rate) + (Distance × Per-km Rate)`
- Different rates per vehicle type
- Price locked in at assignment — never changes

### 📊 Load Optimization *(unique feature)*
- Detect partial loads — flags vehicles running under 60% capacity
- Merge bookings — groups small pending orders onto a single vehicle
- Shared transport suggestions — calculates and displays exact savings amount

### 📍 Priority & Scheduling
- Four levels: Low, Normal, High, Urgent
- Priority queue — urgent deliveries always processed first
- Schedule all deliveries in one step, process one by one

### 📈 Reports
- Full business report: bookings, completions, cancellations, revenue, vehicle usage
- Revenue summary with average per-order value
- Vehicle usage breakdown by type

### 💾 Data Persistence
- All data saved to JSON files automatically on every change
- Survives program restarts — no data loss
- IDs restored on startup — no duplicate records ever

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Any terminal (Command Prompt, PowerShell, Windows Terminal)
- Visual Studio 2022 (recommended) or VS Code with C# extension

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/YourUsername/SmartTransportSystem.git
cd SmartTransportSystem
```

**2. Build the project**
```bash
dotnet build
```

**3. Run the application**
```bash
dotnet run
```

The `data/` folder and all JSON files are created automatically on first run. No database setup or configuration is needed.

### First Run

On first launch the application starts with a clean empty state. A hidden Super User account is created automatically in the background. To get started:

1. Select **Register** and create your first user account
2. Contact the Super User to be promoted to Admin if needed
3. As Admin, add vehicles to the fleet before customers can create bookings

> **Note:** The Super User account credentials are hardcoded and not displayed anywhere in the application interface by design.

---

## ⚙️ How It Works

### Booking Creation Flow

```
Customer enters weight, distance, priority
            ↓
Validation: weight > 0, distance > 0, weight ≤ 20,000 kg
            ↓
Booking created with status: Pending
            ↓
SmartAssignmentService scans available vehicles
            ↓
Filters: remaining capacity ≥ cargo weight
            ↓
Sorts: smallest capacity first (Best Fit)
            ↓
Picks first match → calculates price → assigns vehicle
            ↓
Status → Assigned | Price locked | Files saved
```

### Password Security Flow

```
User sets password → HashValue(password) → stored as hex string
       
Login attempt → HashValue(entered password) → compared to stored hash
       
Match = access granted | No match = access denied
       
Plain text password never stored anywhere
```

### Load Optimization Flow

```
Collect all Pending bookings
            ↓
Sort available vehicles: smallest → largest
            ↓
For each vehicle: pack as many bookings as fit
            ↓
Groups with 2+ bookings = merge candidates
            ↓
Calculate: shared price vs individual total
            ↓
Display savings → admin applies merge with one action
```

---

## 💰 Pricing Model

| Vehicle Type | Base Rate | Per km | Per kg |
|---|---|---|---|
| 🏍️ Bike | Rs 100 | Rs 5 / km | Rs 2 / kg |
| 🚐 Van | Rs 300 | Rs 10 / km | Rs 1.50 / kg |
| 🚛 Truck | Rs 700 | Rs 18 / km | Rs 1 / kg |
| 🏗️ Heavy Truck | Rs 1,500 | Rs 30 / km | Rs 0.70 / kg |

**Formula:**
```
Price = Base Rate + (Weight × Per-kg Rate) + (Distance × Per-km Rate)
```

**Example:**
```
30 kg cargo, 15 km distance, Bike assigned
Price = 100 + (30 × 2) + (15 × 5)
      = 100 + 60 + 75
      = Rs 235.00
```

---

## 👥 User Roles

| Feature | Customer | Admin | Super User |
|---|:---:|:---:|:---:|
| Register / Login | ✅ | ✅ | ✅ |
| Create Bookings | ✅ | ✅ | ❌ |
| View Own Bookings | ✅ | ✅ | ❌ |
| View All Bookings | ❌ | ✅ | ❌ |
| Cancel Own Booking | ✅ | ✅ | ❌ |
| Cancel Any Booking | ❌ | ✅ | ❌ |
| Approve / Reject Bookings | ❌ | ✅ | ❌ |
| Manage Vehicles | ❌ | ✅ | ❌ |
| Schedule Deliveries | ❌ | ✅ | ❌ |
| View Reports | ❌ | ✅ | ❌ |
| View All Users | ❌ | ❌ | ✅ |
| Promote to Admin | ❌ | ❌ | ✅ |
| Revoke Admin | ❌ | ❌ | ✅ |
| Delete Users | ❌ | ❌ | ✅ |
| Change Own Password | ✅ | ✅ | ✅ |
| Update Recovery Code | ✅ | ✅ | ✅ |

---

## 📁 Project Structure

```
SmartTransportSystem/
│
├── Program.cs                        # Entry point — wires services, starts main loop
│
├── Core/
│   ├── Enums.cs                      # BookingStatus, PriorityLevel, VehicleType, UserRole
│   └── Interfaces.cs                 # IBookingService, IPricingService
│
├── Models/
│   └── Models.cs                     # User, Vehicle, Booking, Report
│
├── Services/
│   ├── UserManager.cs                # Registration, login, role management, password reset
│   ├── VehicleManager.cs             # Fleet CRUD, availability tracking
│   ├── BookingManager.cs             # Booking lifecycle, status management
│   ├── PricingService.cs             # Dynamic price calculation
│   ├── SmartAssignmentService.cs     # Best Fit vehicle selection algorithm
│   ├── LoadOptimizationService.cs    # Merge bookings, shared transport suggestions
│   ├── BookingAndOthers.cs           # PriorityScheduler, TrackingService, AdminControl, ReportingService
│   ├── DataStore.cs                  # JSON file read/write persistence layer
│   └── Validation.cs                 # Input validation helpers
│
├── UI/
│   ├── UI.cs                         # Shared console helpers (Ask, Print, Confirm, etc.)
│   ├── AuthMenus.cs                  # Main menu, register, login, forgot password, account settings
│   ├── CustomerMenus.cs              # Customer menu + Super User menu
│   └── AdminMenus.cs                 # Full admin menu — bookings, vehicles, optimization, reports
│
├── ClassDiagram.svg                  # Full UML class diagram
├── README.md                         # This file
└── .gitignore                        # Excludes bin/, obj/, data/, .vs/
```

---

## 🏗️ OOP Design

### Encapsulation
Password hashes and recovery codes are **private fields** inside the `User` class. No external class can read or set them directly — only through controlled methods like `ValidatePassword()`, `ChangePassword()`, and `SetRecoveryCode()`. Vehicle load is managed through `LoadCargo()` and `UnloadCargo()` — the raw `CurrentLoadKg` field cannot be externally set.

### Abstraction
Two interfaces define contracts without implementation:
- `IBookingService` — defines what any booking service must support
- `IPricingService` — defines what any pricing service must support

`BookingManager` implements `IBookingService`. `PricingService` implements `IPricingService`. Consumers depend on the interface, not the concrete class.

### Polymorphism
- `CancelBooking(isAdmin)` — single method, different behaviour depending on caller role
- `ViewBookingDetails(isAdmin)` — shared between Customer and Admin menus
- `SuperChangeRole(promote)` — one method handles both promote and revoke flows
- `BookingApproveReject(approve)` — one method handles both approve and reject

### Single Responsibility Principle
Every class has exactly one job:

| Class | Single Responsibility |
|---|---|
| `UserManager` | User accounts, authentication, roles |
| `VehicleManager` | Vehicle fleet management |
| `BookingManager` | Booking lifecycle |
| `PricingService` | Price calculation only |
| `SmartAssignmentService` | Vehicle selection algorithm |
| `LoadOptimizationService` | Merge and sharing logic |
| `DataStore` | File read/write only |
| `ConsoleUI` | Console output formatting only |

### Dependency Injection
All services receive their dependencies through constructors — no class creates its own dependencies. `BookingManager` receives `UserManager`, `SmartAssignmentService`, and `VehicleManager` as constructor parameters. This keeps modules independent and testable.

---

## 💾 Data Persistence

Data is stored in three JSON files created automatically in a `data/` folder next to the executable:

```
data/
├── users.json       ← user accounts, hashed passwords, hashed recovery codes, roles
├── vehicles.json    ← vehicle fleet, current load, availability
└── bookings.json    ← all bookings, status history, pricing
```

**Key behaviours:**
- Files are created on first run automatically — no setup needed
- Every create/update/delete triggers an immediate full save
- Auto-increment IDs are restored on startup — no duplicates ever
- `data/` is excluded from Git via `.gitignore` — private data never pushed

---

## 📐 Class Diagram

The full UML class diagram is included in the repository as `ClassDiagram.svg`. It shows all 15+ classes with their attributes, methods, and relationships including composition, aggregation, association, and interface realization.

![Class Diagram](ClassDiagram.svg)

---

## 🛠️ Built With

| Technology | Purpose |
|---|---|
| C# 12 / .NET 8.0 | Core language and runtime |
| `System.Text.Json` | JSON serialization for data persistence |
| `System.Collections.Generic` | Dictionary, List, Queue data structures |
| Visual Studio 2022 | Development IDE |
| Git + GitHub | Version control and hosting |

**No external NuGet packages.** The entire project uses only the .NET standard library.

---

## 🚗 Supported Vehicle Models

Examples of real-world vehicles you can add to the fleet:

| Type | Example Models |
|---|---|
| Bike | Honda CD 70 2023, Yamaha YBR 125 2023, Suzuki GS 150 2022, United US 100 2023 |
| Van | Toyota HiAce 2022, Suzuki Carry 2023, Hyundai H100 2021, Kia Bongo 2022 |
| Truck | Isuzu NPR 2022, Hino 300 Series 2021, Mitsubishi Canter 2023, FAW Carrier 2022 |
| Heavy Truck | Mercedes Actros 2022, Volvo FH16 2023, MAN TGX 2022, Isuzu Giga 2021 |

---

## 👨‍💻 Authors

Developed as a group project for the **Object Oriented Programming (OOP)** course.

| Name | Role |
|---|---|
| Amir Ali | Project Lead, Core Architecture, Super User System |
| [Member 2] | Booking System, Smart Assignment, Pricing Engine |
| [Member 3] | Load Optimization, UI Layer, Reports |

---

## 📄 License

This project is licensed under the MIT License — feel free to use it for learning purposes.

---

<div align="center">

**Built with C# · Powered by OOP · No external dependencies**

</div>