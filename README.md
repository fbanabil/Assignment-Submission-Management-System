# 🎓 Assignment & Submission Management System

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Next.js 16](https://img.shields.io/badge/Next.js-16_App_Router-000000?style=for-the-badge&logo=nextdotjs&logoColor=white)
![React 19](https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![PostgreSQL 17](https://img.shields.io/badge/PostgreSQL-17-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

An enterprise-grade, role-based web application for educational institutions that enables **Teachers** to publish and grade assignments, **Students** to track deadlines and submit coursework, and **Admins** to manage users, classes, subjects, and teacher allocations.

Engineered with a decoupled architecture featuring an **ASP.NET Core 9 Web API** backend (using CQRS handlers, FluentValidation, and EF Core 8) paired with a **Next.js 16 (App Router)** frontend, fully containerized using **Docker Compose**.

---

## 📋 Table of Contents

- [🎓 Assignment \& Submission Management System](#-assignment--submission-management-system)
  - [📋 Table of Contents](#-table-of-contents)
  - [✨ Key Features](#-key-features)
    - [🛡️ Role-Based Access Control (RBAC)](#️-role-based-access-control-rbac)
    - [👑 Admin Management Portal](#-admin-management-portal)
    - [👨‍🏫 Teacher Workstation](#-teacher-workstation)
    - [🎓 Student Portal](#-student-portal)
  - [🏛️ System Architecture](#️-system-architecture)
  - [🛠️ Tech Stack](#️-tech-stack)
  - [📁 Project Structure](#-project-structure)
  - [🔑 Demo Credentials](#-demo-credentials)
  - [🚀 Quick Start \& Deployment](#-quick-start--deployment)
    - [Prerequisites](#prerequisites)
    - [Option A: Docker Compose Deployment (Recommended)](#option-a-docker-compose-deployment-recommended)
    - [Option B: Manual Local Development Setup](#option-b-manual-local-development-setup)
      - [1. Start Database Container](#1-start-database-container)
      - [2. Run ASP.NET Core Backend API](#2-run-aspnet-core-backend-api)
      - [3. Run Next.js Frontend](#3-run-nextjs-frontend)
  - [⚙️ Environment Variables](#️-environment-variables)
    - [Docker / Backend Configuration (`src/docker-compose.yml`)](#docker--backend-configuration-srcdocker-composeyml)
    - [Frontend Configuration (`src/frontend/.env.local`)](#frontend-configuration-srcfrontendenvlocal)
  - [📡 API Documentation](#-api-documentation)
    - [Key API Endpoint Summary](#key-api-endpoint-summary)
      - [Authentication (`/api/Auth`)](#authentication-apiauth)
      - [Admin Management (`/api/Admin`)](#admin-management-apiadmin)
      - [Teacher Operations (`/api/Teacher`)](#teacher-operations-apiteacher)
      - [Student Operations (`/api/Student`)](#student-operations-apistudent)
  - [🧪 Testing \& Quality Assurance](#-testing--quality-assurance)
    - [Test Suite Structure](#test-suite-structure)
    - [Executing Tests](#executing-tests)
  - [🔒 Security Architecture](#-security-architecture)
  - [💡 Design Rationale \& Domain Assumptions](#-design-rationale--domain-assumptions)
  - [📄 License](#-license)

---

## ✨ Key Features

### 🛡️ Role-Based Access Control (RBAC)

- **Server-Side Enforcement**: Role claims (`Admin`, `Teacher`, `Student`) are verified on every protected API endpoint via JWT Bearer policies.
- **Asymmetric Security**: JWT tokens are signed using **RS256 RSA key-pair cryptography** (2048-bit Private/Public keys).
- **Token Invalidation**: Built-in `TokenBlacklistMiddleware` supports instantaneous server-side token revocation and logout.

### 👑 Admin Management Portal

- **User Management**: Complete CRUD interface to onboard, update, and manage Admins, Teachers, and Students.
- **Academic Structure**: Provision Classes, Subjects, and map Class-Subject relationships.
- **Workload Allocation**: Assign Teachers to specific Class-Subject combinations.
- **Student Enrollment**: Assign Students to their respective academic Classes.
- **System Metrics**: Real-time overview of user counts, active assignments, and submission activity.

### 👨‍🏫 Teacher Workstation

- **Class & Subject Workspace**: Scoped view displaying only assigned classes and subjects.
- **Assignment Lifecycle**: Create draft assignments, edit due dates/instructions, and publish when ready for students.
- **Custom Assignment Rules**: Configure `AllowLateSubmission` and `AllowResubmission` on a per-assignment basis.
- **Grading Engine**: Review student submissions, assign marks (validated against maximum marks server-side), and provide qualitative feedback.

### 🎓 Student Portal

- **Personalized Dashboard**: View published assignments relevant only to the student's enrolled class.
- **Submission Workflow**: Submit answers directly through the portal before deadlines.
- **Resubmission Control**: Update existing submissions prior to due dates when enabled by the teacher.
- **Grade & Feedback Tracker**: Monitor marks scored, class rank, late submission indicators, and teacher feedback.

---

## 🏛️ System Architecture

```
                                  +-----------------------+
                                  |    Client Browser     |
                                  +-----------+-----------+
                                              |
                                              | HTTP / JSON (Port 3000)
                                              v
                                  +-----------------------+
                                  |   Next.js 16 App      |
                                  |   (React 19 + Zod)    |
                                  +-----------+-----------+
                                              |
                                              | REST API Requests (Port 8080)
                                              v
 +-----------------------------------------------------------------------------------+
 |  ASP.NET Core 9 Web API Backend                                                   |
 |                                                                                   |
 |  +-----------------------+     +------------------------+     +-----------------+ |
 |  | Auth / RS256 Middleware| --> |   Route Handlers  | --> | Service Layer   | |
 |  +-----------------------+     +------------------------+     +--------+--------+ |
 |                                                                        |          |
 |                                                                        v          |
 |                                                               +-----------------+ |
 |                                                               | EF Core 8 ORM   | |
 |                                                               +--------+--------+ |
 +------------------------------------------------------------------------|----------+
                                                                          |
                                                                          v
                                                               +---------------------+
                                                               | PostgreSQL 17 DB    |
                                                               +---------------------+
```

---

## 🛠️ Tech Stack

| Layer                        | Technology                 | Version              | Description                                        |
| :--------------------------- | :------------------------- | :------------------- | :------------------------------------------------- |
| **Frontend Framework** | Next.js (App Router)       | `v16.3`            | React 19 server/client components with TypeScript  |
| **Frontend Styling**   | Tailwind CSS               | `v4.0`             | Responsive, accessible UI styling                  |
| **Form & Validation**  | React Hook Form & Zod      | `v7.84` / `v4.4` | Client-side schema validation and state management |
| **API Client**         | Axios                      | `v1.19`            | HTTP request client with auth token interceptors   |
| **Backend Framework**  | ASP.NET Core Web API       | `.NET 9.0`         | High-performance C# RESTful web service            |
| **Data Access**        | Entity Framework Core      | `v8.0`             | Code-first ORM with automatic migration seeding    |
| **Database**           | PostgreSQL                 | `v17-alpine`       | Relational database engine                         |
| **Database GUI**       | pgAdmin 4                  | `latest`           | Web-based database management portal               |
| **Security & Auth**    | JWT (RS256 RSA) + BCrypt   | `v8.22` / `v4.2` | Asymmetric token authentication & password hashing |
| **Validation & Audit** | FluentValidation & Serilog | `v12.1`            | DTO request validation & structured error logging  |
| **API Documentation**  | Swashbuckle / Swagger UI   | `v8.1`             | OpenAPI specification rendering                    |
| **Testing**            | xUnit & Moq                | `v2.9` / `v4.20` | Unit, integration, and database test suites        |
| **Containerization**   | Docker & Docker Compose    | `v2`               | Multi-container orchestration                      |

---

## 📁 Project Structure

```
AssignmentManager/
├── src/
│   ├── backend/                        # ASP.NET Core 9 Web API
│   │   ├── ConfigurationExtension/     # Service collection & JWT registration
│   │   ├── Controllers/                # Admin, Auth, Teacher, Student endpoints
│   │   ├── Data/                       # DbContext, EF Configurations & JSON Seed data
│   │   │   └── SeedData/               # 8-step JSON seed dataset (Users, Classes, etc.)
│   │   ├── DTOs/                       # Strongly-typed Data Transfer Objects
│   │   ├── Handlers/                   # CQRS-style business domain request handlers
│   │   ├── Helpers/                    # Auth, Password hashing & Seed helpers
│   │   ├── Middlewares/                # Exception handling, Serilog, Token Revocation
│   │   ├── Migrations/                 # EF Core database schema migrations
│   │   ├── Models/                     # Entity models and Enums
│   │   ├── Services/                   # Core business logic interfaces & implementations
│   │   ├── Validators/                 # FluentValidation request rules
│   │   ├── Dockerfile                  # Multi-stage backend build file
│   │   ├── Backend.csproj              # .NET 9 Project dependencies
│   │   ├── private_key.pem             # RSA Private key for JWT signing
│   │   └── public_key.pem              # RSA Public key for JWT verification
│   │
│   ├── frontend/                       # Next.js 16 Application
│   │   ├── src/
│   │   │   ├── app/                    # Next.js App Router (admin, teacher, student, login)
│   │   │   ├── components/             # Reusable UI components & layouts
│   │   │   ├── lib/                    # Axios API instance & Auth Context provider
│   │   │   └── types/                  # TypeScript interface definitions
│   │   ├── Dockerfile                  # Multi-stage frontend container setup
│   │   └── package.json                # Frontend dependencies & scripts
│   │
│   ├── Tests/                          # xUnit Automated Test Suite
│   │   ├── ControllerTests/            # Controller HTTP status & payload tests
│   │   ├── DatabaseTests/              # EF Core CRUD & relational constraint tests
│   │   ├── HandlerTests/               # Business workflow & permission logic tests
│   │   ├── ServiceTests/               # Core service unit tests
│   │   └── Tests.csproj                # xUnit, Moq & In-Memory DB test configuration
│   │
│   ├── docker-compose.yml              # Container orchestration (API, Frontend, Postgres, pgAdmin)
│   └── docker-compose.yml.override     # Local development docker settings
├── LICENSE                             # MIT License
└── README.md                           # Documentation
```

---

## 🔑 Demo Credentials

The database is pre-seeded on first boot with rich demo accounts across all three user roles:

| Role              | Name         | Email                     | Password                | Access / Scope                                    |
| :---------------- | :----------- | :------------------------ | :---------------------- | :------------------------------------------------ |
| **Admin**   | Demo Admin   | `demoadmin@gmail.com`   | `demoadminpassword`   | System-wide management & audit access             |
| **Teacher** | Demo Teacher | `demoteacher@gmail.com` | `demoteacherpassword` | Manages assignments & grades for assigned classes |
| **Student** | Demo Student | `demostudent@gmail.com` | `demostudentpassword` | Submits assignments & tracks class grades         |

---

## 🚀 Quick Start & Deployment

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (with Docker Compose v2)
- Alternatively for manual setup: [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and [Node.js 18+](https://nodejs.org/)

---

### Option A: Docker Compose Deployment (Recommended)

Run the full platform (Database, API, Frontend, pgAdmin) in isolated containers with a single command:

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/fbanabil/Assignment-Submission-Management-System.git
   cd Assignment-Submission-Management-System
   ```
2. **Put RSA public and private key in docker-compose.yml files backend environment (Make sure to save the file before later actions)**

   **As these keys are not used any other project keys are given to compose-file. Don't use it in another project as they are not safe to use anymore.**
   **In case of production please follow instructions below.** 

   **To generate RSA keys: In terminal**
   ```bash
   openssl genrsa -out private.pem 2048
   ```
   It creates private.pem
   ```bash
   openssl rsa -in private.pem -pubout -out public.pem
   ```
   It generate public.pem.
   **While getting key from this file and put into docker-compose it is needed to escape the newlines.**
   

3. **Launch the Container Stack**:
   From the repository root directory, execute:

   ```bash
   docker compose -f src/docker-compose.yml up -d --build
   ```
4. **Verify Running Services**:
   The containers will automatically run EF Core migrations and apply seed data:

   | Service                             | Access URL                                                    | Port     |
   | :---------------------------------- | :------------------------------------------------------------ | :------- |
   | **Frontend Web App**          | [http://localhost:3000](http://localhost:3000)                 | `3000` |
   | **ASP.NET Core Web API**      | [http://localhost:8080](http://localhost:8080)                 | `8080` |
   | **Swagger API Documentation** | [http://localhost:8080/swagger](http://localhost:8080/swagger) | `8080` |
   | **pgAdmin Web Portal**        | [http://localhost:5050](http://localhost:5050)                 | `5050` |
   | **PostgreSQL Database**       | `localhost:5432`                                            | `5432` |
5. **Shutdown Services**:

   ```bash
   docker compose -f src/docker-compose.yml down
   ```

---

### Option B: Manual Local Development Setup

If you prefer running the backend and frontend directly on your host machine:

#### 1. Start Database Container

```bash
cd src
docker compose up -d postgres pgadmin
```

#### 2. Run ASP.NET Core Backend API

```bash
cd src/backend
dotnet restore
dotnet ef database update
dotnet run
```

The API will start at `http://localhost:8080` (or `http://localhost:5000 or https://localhost:7209` depending on `appsettings.json` launch profiles).

#### 3. Run Next.js Frontend

```bash
cd src/frontend
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.
**Make sure that running ports are set perfectly in appsettings.json in frontend and .env in backend**

---

## ⚙️ Environment Variables

### Docker / Backend Configuration (`src/docker-compose.yml`)

| Variable                                 | Default Value                    | Description                                  |
| :--------------------------------------- | :------------------------------- | :------------------------------------------- |
| `POSTGRES_USER`                        | `postgres`                     | Database superuser account                   |
| `POSTGRES_PASSWORD`                    | `mysecretpassword`             | Database superuser password                  |
| `POSTGRES_DB`                          | `mydb`                         | Primary application database name            |
| `ConnectionStrings__DefaultConnection` | `Host=assignment_postgres;...` | Npgsql EF Core connection string             |
| `JwtSettings__Issuer`                  | `AssignmentManager.API`        | Token issuer domain string                   |
| `JwtSettings__Audience`                | `AssignmentManager.UI`         | Token target audience string                 |
| `JwtSettings__PrivateKey`              | *[RSA PEM String]*             | 2048-bit Private Key for RS256 token signing |
| `JwtSettings__PublicKey`               | *[RSA PEM String]*             | 2048-bit Public Key for token verification   |

### Frontend Configuration (`src/frontend/.env.local`)

| Variable                     | Default Value                 | Description                       |
| :--------------------------- | :---------------------------- | :-------------------------------- |
| `NEXT_PUBLIC_API_URL`      | `http://localhost:8080/api` | Base API URL used by Axios client |
| `NEXT_PUBLIC_API_BASE_URL` | `http://localhost:8080/api` | Fallback API base URL endpoint    |

---

## 📡 API Documentation

Once the backend service is running, interactive OpenAPI/Swagger documentation is available at `http://localhost:8080/swagger`.

### Key API Endpoint Summary

#### Authentication (`/api/Auth`)

- `POST /api/Auth/Login` — Authenticate user and issue JWT token + HttpOnly refresh cookie.
- `POST /api/Auth/RefreshToken` — Rotate refresh token and obtain new JWT access token.
- `POST /api/Auth/Logout` — Revoke access token via blacklist middleware and clear cookies.

#### Admin Management (`/api/Admin`)

- `GET /api/Admin/Dashboard` — Get overall system analytics (counts of users, classes, assignments).
- `POST /api/admin/users` — Onboard a new user (`Admin`, `Teacher`, `Student`).
- `PUT /api/Admin/Users/{id}` — Modify user details and active status.
- `DELETE /api/Admin/Users/{id}` — Remove a user record.
- `GET / POST / PUT / DELETE /api/Admin/Classes` — Manage institutional classes.
- `GET / POST / PUT / DELETE /api/Admin/Subjects` — Manage academic subjects.
- `POST /api/Admin/AssignTeacher` — Map a teacher to a class and subject.
- `POST /api/Admin/EnrollStudent` — Enroll a student into a designated class.

#### Teacher Operations (`/api/Teacher`)

- `GET /api/Teacher/Dashboard` — Retrieve teacher summary and active class schedules.
- `GET / POST / PUT / DELETE /api/Teacher/Assignments` — Draft, edit, and delete assignments.
- `POST /api/Teacher/PublishAssignment/{id}` — Publish draft assignment to enrolled students.
- `GET /api/Teacher/Submissions` — View student submissions for teacher's assigned subjects.
- `POST /api/Teacher/GradeSubmission` — Evaluate a submission, award marks, and provide feedback.

#### Student Operations (`/api/Student`)

- `GET /api/Student/Dashboard` — View pending/completed assignments and overall performance.
- `GET /api/Student/Assignments` — List published assignments filtered by status.
- `POST /api/Student/Submit` — Submit coursework answer text for an active assignment.
- `PUT /api/Student/Resubmit` — Update submission content before the deadline (if allowed).

---

## 🧪 Testing & Quality Assurance

The repository includes a comprehensive unit and integration test suite built with **xUnit**, **Moq**, and **EF Core In-Memory / SQLite**.

### Test Suite Structure

- **ControllerTests**: Validates HTTP response status codes, route protections, and payload formats.
- **DatabaseTests**: Verifies EF Core relationships, unique constraints, and CRUD operations.
- **HandlerTests**: Tests CQRS domain workflow handlers and business validation logic.
- **ServiceTests**: Verifies underlying business logic services in isolation using mocked dependencies.

### Executing Tests

To run the complete test suite:

```bash
cd src/Tests
dotnet test --logger "console;verbosity=detailed"
```

To run with code coverage report:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

---

## 🔒 Security Architecture

1. **RS256 Asymmetric Cryptography**:
   - Authentication tokens are signed using standard RSA 2048-bit private keys (`private_key.pem`) and verified using matching public keys (`public_key.pem`).
2. **HttpOnly Cookies**:
   - Refresh tokens are delivered via Secure, HttpOnly cookies to mitigate Cross-Site Scripting (XSS) risks.
3. **Instantaneous Token Revocation**:
   - Logout triggers token fingerprint blacklisting via `TokenBlacklistMiddleware`, immediately rejecting subsequent calls with revoked tokens.
4. **Input Sanitization & Schema Validation**:
   - All API incoming request DTOs are validated using **FluentValidation** rules server-side and **Zod** client-side.
5. **Centralized Exception Middleware**:
   - Errors are intercepted by `GlobalExceptionHandler` to sanitize internal stack traces before returning formatted problem details to clients.

---

## 💡 Design Rationale & Domain Assumptions

- **Relational Integrity over Document Store**:
  - PostgreSQL was selected over NoSQL databases because academic entities (Users, Classes, Subjects, Enrollments, Assignments, Submissions) rely heavily on foreign-key constraints and relational integrity.
- **Single Active Class Enrollment**:
  - A student belongs to one primary academic class per term/semester.
- **Assignment Publication Lifecycle**:
  - Assignments created in `Draft` state are hidden from students until explicitly transitioned to `Published` by the teacher.
- **Per-Assignment Governance**:
  - Due dates, `AllowLateSubmission`, and `AllowResubmission` toggles are enforced strictly on the backend API layer during submission calls.
- **Grade Boundary Enforcement**:
  - Awarded marks cannot exceed an assignment's `MaxMarks` parameter, enforced by FluentValidation and database checks.

---

## 📄 License

Distributed under the MIT License. See [`LICENSE`](LICENSE) for more details.
