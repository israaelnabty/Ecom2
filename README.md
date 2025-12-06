# E-Commerce API (.NET 8)

A robust, enterprise-grade E-Commerce backend API built with .NET 8 and Entity Framework Core. This project demonstrates a clean 3-Tier Architecture (PL, BLL, DAL) and integrates advanced features like Biometric Authentication (Face ID), OAuth, and Background Job processing.

---

## 🚀 Features

### 🔐 Authentication & Identity

- **JWT Authentication**: Secure stateless authentication using JSON Web Tokens.
- **ASP.NET Core Identity**: Complete user management (Roles, Claims, Password Hashing).
- **Face ID Login**: Biometric authentication using FaceRecognitionDotNet (Dlib). Users can register their face via webcam and log in without passwords.
- **Google OAuth**: External social login integration.
- **Email Confirmation**: Account verification using SendGrid API.

---

### 🛒 E-Commerce Core

- **Product Management**: CRUD operations for Products, Brands, and Categories.
- **Shopping Cart**: Persistent cart management stored in SQL.
- **Order System**: Order placement, tracking, and history.
- **Address Book**: User-specific address management with GPS coordinates.

---

### ⚙️ Advanced Features

#### **Background Jobs (Hangfire)**

- **Abandoned Cart Recovery**: Automatically emails users who haven't checked out in 2 days.
- **Order Cleanup**: System tasks to maintain database health.

#### **Automated Seeding**

- Fetches real data from dummyjson.com on first run.
- Seeds roles, admin user, and default products.
- Applies pending migrations automatically.

#### **AutoMapper**

- Clean mapping between Entities and DTOs/ViewModels.

---

## 🏗 Architecture

The solution follows a Modular Monolith / 3-Tier Architecture:

### **Ecom.DAL (Data Access Layer)**

- Database Context (DbContext)
- Domain Entities (POCOs)
- Repositories (Generic and Specific)
- Migrations

### **Ecom.BLL (Business Logic Layer)**

- Services (AccountService, FaceIdService, etc.)
- Business validation logic
- DTOs / ViewModels
- AutoMapper Profiles
- Integrations (SendGrid, FaceRecognition)

### **Ecom.PL (Presentation Layer)**

- ASP.NET Core Web API Controllers
- Hangfire Dashboard configuration
- Dependency Injection setup

---

## 🛠 Tech Stack

| Feature | Technology |
|--------|------------|
| Framework | .NET 8 |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | JWT Bearer & Google OAuth |
| AI/ML | FaceRecognitionDotNet (Dlib wrapper) |
| Background Jobs | Hangfire |
| Email | SendGrid |
| Documentation | Swagger / OpenAPI |

---

## 🚀 Getting Started

### **Prerequisites**

- .NET 8 SDK  
- SQL Server (Express or Developer)  
- Visual Studio 2022 / VS Code  

---

### **1. Clone the Repository**

```bash

2. Configure appsettings.json

Navigate to Ecom.PL and update the settings:
```
{
  "ConnectionStrings": {
    "defaultConnection": "Server=.;Database=EcomDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "Key": "YOUR_SUPER_SECRET_LONG_KEY_HERE",
    "Issuer": "https://localhost:7123",
    "Audience": "https://localhost:4200"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "EmailSettings": {
    "ApiKey": "YOUR_SENDGRID_API_KEY",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "E-Com Support"
  },
  "ClientUrl": "http://localhost:4200"
}
```

3. Run the Application

No manual migrations required.
The built-in DbSeeder will:

Apply migrations

Create the Database

Seed Admin & Customer roles

Fetch and seed products

cd Ecom.PL
dotnet run

4. Access the API
Tool	URL
Swagger UI	https://localhost:7123/swagger

Hangfire Dashboard	https://localhost:7123/hangfire
👤 Default Users (Seeded)
Role	Email	Password
Admin	admin@ecom.com
	P@ssword123
Reviewer	reviewer@ecom.com
	P@ssword123
📦 Face ID Setup Notes

This project uses FaceRecognitionDotNet, which depends on native Dlib binaries.
git clone https://github.com/YourUsername/Ecom-Backend.git
cd Ecom-Backend
