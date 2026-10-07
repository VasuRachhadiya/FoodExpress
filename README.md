# 🍔 FoodExpress - Online Food Ordering & Restaurant Management System
> **Academic Project | Full-Stack Web Application**  
> Built with **ASP.NET Core 9.0 MVC**, **Entity Framework Core 9**, **Microsoft SQL Server**, and **Bootstrap 5**.

---

## 🌟 Executive Summary
**FoodExpress** is an end-to-end food ordering, restaurant partner management, and delivery tracking web application. The platform provides a seamless three-tier role-based experience:
1. **Customers:** Browse top-rated restaurants across cities, filter dishes, manage saved delivery addresses, apply discount coupons, and track orders with a real-time status stepper.
2. **Restaurant Owners:** Dedicated Partner Portal to manage restaurant profiles, food menu items, dish availability, and incoming kitchen orders.
3. **Platform Administrators:** Centralized governance console to oversee system-wide restaurants, food categories, user roles, coupons, customer reviews, and sales analytics.

---

## 🔑 Pre-Seeded Demo Credentials
The application automatically creates database tables and seeds demo accounts at initial startup:

| Role | Email Address | Password | Portal Access |
| :--- | :--- | :--- | :--- |
| **System Administrator** | `admin@foodexpress.com` | `Admin@123` | `/Admin/Dashboard` |
| **Restaurant Owner** | `owner@foodexpress.com` | `Owner@123` | `/Owner/Dashboard` |
| **Customer / Diner** | `customer@foodexpress.com` | `Customer@123` | Public Storefront |

---

## 📱 Universal Device Responsiveness
The application is designed to be fully responsive across all device viewports:
- **Mobile Phones (320px – 576px):**
  - Offcanvas slide-out navigation drawers for Admin and Owner panels (`#adminMobileSidebar`, `#ownerMobileSidebar`).
  - Compact responsive 5-step order tracking stepper with scaled icons and non-colliding labels.
  - Touch-friendly touch targets ($\ge 44\text{px}$) and responsive cart tables.
  - Demo login credentials banner visible and neatly formatted on mobile viewports.
- **Tablets & iPads (768px – 1024px):**
  - Fluid grid layouts, optimized card columns, and responsive forms.
- **Desktops & Laptops (1200px+):**
  - Full sidebar dashboards, sticky order summaries, and hover animations.

---

## 🛠️ Technology Stack
- **Framework:** ASP.NET Core 9.0 MVC (`net9.0`)
- **Language:** C# 13
- **ORM / Data Access:** Entity Framework Core 9.0 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Database:** Microsoft SQL Server / LocalDB
- **Authentication & Security:** ASP.NET Core Identity with role-based authorization cookies and Anti-Forgery tokens (CSRF)
- **Frontend / Styling:** Vanilla CSS3 + Bootstrap 5.3.2 + Bootstrap Icons + Plus Jakarta Sans Typography
- **Containerization & Deployment:** Multi-stage production `Dockerfile`, `vercel.json`

---

## 🚀 How to Run Locally

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB, SQL Express, or SQL Server 2019/2022)

### Steps
1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/FoodExpress.git
   cd FoodExpress
   ```

2. **Restore dependencies & build:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the application:**
   ```bash
   cd FoodExpress
   dotnet run
   ```

4. **Open in browser:**
   Navigate to `https://localhost:5001` or `http://localhost:5000`.
   The database will automatically be created and seeded with categories, restaurants, dishes, coupons, and test accounts on first launch.

---

## 🌐 Deployment Guide

### 1. Uploading to GitHub
```bash
git init
git add .
git commit -m "Initial commit: FoodExpress academic project"
git branch -M main
git remote add origin https://github.com/your-username/FoodExpress.git
git push -u origin main
```

---

### 2. Deploying to Vercel
Vercel is primarily built for static frontend applications, Next.js, and serverless Node.js/Python functions. It does not natively run long-running compiled C# ASP.NET Core servers or SQL Server databases.

To support Vercel deployments:
1. This repository includes a configured `vercel.json` pointing to `public/`.
2. `public/index.html` provides a **Live Academic Project Showcase** with interactive demonstrations, role credentials, architecture specs, and live links.
3. When you import this repo into Vercel, it deploys **with a green checkmark** without build errors.

---

### 3. Deploying the Full-Stack ASP.NET Core Backend Online (Free Options)

#### Option A: MonsterASP.NET (Recommended for Academic Evaluation)
- **Features:** 100% Free ASP.NET Core 9 + MSSQL database hosting with no credit card required.
- **Steps:**
  1. Register at [monsterasp.net](https://www.monsterasp.net/).
  2. Create a free website targeting **.NET 9**.
  3. Publish directly from Visual Studio via Web Deploy or FTP, or connect your Git repository.

#### Option B: Render.com (via Docker)
- **Features:** Free container hosting using the included `Dockerfile`.
- **Steps:**
  1. Log into [Render.com](https://render.com/).
  2. Click **New +** -> **Web Service**.
  3. Connect your GitHub repository.
  4. Select **Docker** environment. Render will automatically build the multi-stage `Dockerfile`.
  5. Add your database connection string as an environment variable `ConnectionStrings__DefaultConnection`.

#### Option C: Azure App Service (F1 Free Tier)
- **Features:** Microsoft's native cloud hosting for .NET Core applications.
- **Steps:**
  1. Open Visual Studio.
  2. Right-click the `FoodExpress` project -> **Publish** -> **Azure** -> **Azure App Service (Linux or Windows)**.
  3. Select the free **F1 Tier**.

---

## 📂 Project Architecture & Directory Structure
```
FoodExpress/
├── .gitignore                   # Visual Studio & .NET git ignore rules
├── .dockerignore                # Container build ignore rules
├── Dockerfile                   # Multi-stage production container build
├── FoodExpress.sln              # Root Visual Studio solution
├── README.md                    # Comprehensive documentation
├── vercel.json                  # Vercel deployment configuration
├── public/                      # Static showcase for Vercel
│   └── index.html               # Project overview & credential showcase
└── FoodExpress/                 # Main ASP.NET Core 9 Web Project
    ├── Areas/
    │   ├── Admin/               # Admin Portal (Controllers & Views)
    │   │   └── Views/Shared/_AdminLayout.cshtml  # Responsive Offcanvas Layout
    │   └── Owner/               # Owner Portal (Controllers & Views)
    │       └── Views/Shared/_OwnerLayout.cshtml  # Responsive Offcanvas Layout
    ├── Controllers/             # MVC Controllers (Cart, Orders, Food, Restaurants, etc.)
    ├── Data/                    # DbContext & Automatic DbInitializer
    ├── Migrations/              # Entity Framework Core migrations
    ├── Models/                  # Domain entities (User, Order, FoodItem, etc.)
    ├── Services/                # CartService, OrderService, CouponService
    ├── ViewModels/              # Strongly-typed Razor ViewModels
    ├── Views/                   # Public Razor Views (Home, Food, Cart, Checkout, etc.)
    │   └── Shared/_Layout.cshtml # Universal responsive layout with demo bar
    ├── wwwroot/
    │   ├── css/site.css         # Custom responsive styles, steppers, and animations
    │   └── js/site.js           # Client-side interactions & cart helpers
    └── Program.cs               # App startup, DI registration, & routing pipeline
```

---

## 📄 License & Attribution
Developed as an academic submission. All rights reserved.
