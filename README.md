# Assignment & Submission Management System

A role-based web application for schools/colleges that lets **Teachers** create and grade assignments, **Students** submit their work and track grades, and **Admins** manage users, classes, subjects, and teacher assignments.

Built for the OnnoRokom Projukti Limited Assistant Software Engineer recruitment project.

---

## Table of Contents

- [Assignment \& Submission Management System](#assignment--submission-management-system)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Features](#features)
  - [Tech Stack](#tech-stack)
  - [Project Structure](#project-structure)
  - [Prerequisites](#prerequisites)
  - [Getting Started](#getting-started)
    - [1. Clone the Repository](#1-clone-the-repository)
    - [2. Backend \& Database Setup (Docker)](#2-backend--database-setup-docker)
    - [3. Frontend Setup](#3-frontend-setup)
  - [Environment Variables](#environment-variables)
  - [Running Tests](#running-tests)
  - [API Documentation](#api-documentation)
  - [Demo Credentials](#demo-credentials)
  - [Assumptions](#assumptions)
  - [Known Limitations](#known-limitations)

---

## Overview

The system supports three roles — **Admin**, **Teacher**, and **Student** — each with a dedicated dashboard and permission set enforced both in the UI and on the backend API. Teachers create assignments scoped to a class and subject, publish them when ready, and grade student submissions. Students see only the assignments published for their own class, submit their answers, and track their marks and feedback.

## Features

- JWT-based authentication with role claims
- Server-side role-based authorization on every protected endpoint (not just frontend route guards)
- Admin: manage users, classes, subjects, and teacher-to-class-subject assignments; read-only visibility into all assignments and submissions
- Teacher: create/edit/delete/publish assignments; view and grade submissions for their own classes/subjects only
- Student: view published assignments for their class; submit and (where allowed) update submissions before the deadline; view marks and feedback
- Late submission and resubmission behavior configurable per assignment
- Swagger/OpenAPI documentation for all endpoints
- Request/error logging via Serilog
- Database schema managed with EF Core migrations, plus a seed script for demo data

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js (App Router), TypeScript, Tailwind CSS, React Hook Form, Zod, Axios |
| Backend | ASP.NET Core 8 Web API, C#, Entity Framework Core, FluentValidation, Serilog, Swashbuckle |
| Database | PostgreSQL |
| Auth | JWT, BCrypt.Net for password hashing |
| Testing | xUnit, Moq |
| Infrastructure | Docker (API + PostgreSQL containerized), frontend run locally |

## Project Structure

```
.
├── backend/                # ASP.NET Core Web API
│   ├── src/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── DTOs/
│   │   ├── Services/
│   │   ├── Middleware/
│   │   ├── Data/           # DbContext, Migrations, Seed
│   │   └── Program.cs
│   ├── tests/               # xUnit test project
│   └── Dockerfile
├── frontend/                # Next.js application
│   ├── src/
│   │   ├── app/
│   │   │   ├── admin/
│   │   │   ├── teacher/
│   │   │   ├── student/
│   │   │   └── login/
│   │   ├── components/
│   │   ├── lib/             # API client, auth context
│   │   └── types/
│   └── .env.local
├── docker-compose.yml        # API + PostgreSQL
├── .env.example
└── README.md
```

## Prerequisites

- [Docker](https://docs.docker.com/engine/install/) + Docker Compose plugin
- [Node.js](https://nodejs.org/) 18+ and npm
- Git

You do **not** need PostgreSQL or .NET installed locally — the API and database run in Docker.

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd <repository-folder>
```

### 2. Backend & Database Setup (Docker)

Copy the example environment file and adjust values if needed:

```bash
cp .env.example .env
```

Start the API and PostgreSQL containers:

```bash
docker compose up -d --build
```

This will:
- Start a PostgreSQL container
- Build and start the ASP.NET Core API container
- Apply EF Core migrations automatically on startup
- Run the seed script, creating demo Admin, Teacher, and Student accounts

The API will be available at `http://localhost:5000`.

To view logs:

```bash
docker compose logs -f api
```

To stop the containers:

```bash
docker compose down
```

### 3. Frontend Setup

The frontend runs locally (outside Docker) for faster development:

```bash
cd frontend
npm install
```

Create `frontend/.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

Start the dev server:

```bash
npm run dev
```

The app will be available at `http://localhost:3000`.

## Environment Variables

See `.env.example` at the project root for the backend/Docker variables, and `frontend/.env.local` for the frontend variable. No real secrets are committed — replace placeholder values locally.

| Variable | Location | Description |
|---|---|---|
| `POSTGRES_USER` | `.env` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `.env` | PostgreSQL password |
| `POSTGRES_DB` | `.env` | Database name |
| `ConnectionStrings__DefaultConnection` | `.env` | Full connection string used by the API |
| `Jwt__Key` | `.env` | Secret key used to sign JWTs |
| `Jwt__Issuer` / `Jwt__Audience` | `.env` | JWT issuer/audience values |
| `NEXT_PUBLIC_API_URL` | `frontend/.env.local` | Base URL the frontend uses to call the API |

## Running Tests

Backend unit tests (business rules, authorization, submission workflow):

```bash
cd backend
dotnet test
```

## API Documentation

Once the backend is running, Swagger UI is available at:

```
http://localhost:5000/swagger
```

## Demo Credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@example.com | _to be filled in_ |
| Teacher | teacher@example.com | _to be filled in_ |
| Student | student@example.com | _to be filled in_ |


## Assumptions

- A Student belongs to exactly one Class at a time.
- A Subject can be taught in multiple Classes; a Teacher is assigned per Class+Subject pair.
- "Update a submission before the deadline, if allowed" is implemented as an `AllowResubmission` flag on each Assignment, set by the Teacher.
- Late submission is disabled by default but toggleable per assignment via `AllowLateSubmission`.
- Draft assignments are invisible to Students; only Published assignments appear in their view.
- Marks cannot exceed an assignment's `MaxMarks` (enforced server-side).
- Admin has full read access system-wide and full write access to Users/Classes/Subjects/Assignments, but does not grade submissions — grading is a Teacher-only action.
- PostgreSQL was chosen over MongoDB because the domain's entities (users, classes, subjects, assignments, submissions) have clear relational structure and foreign-key relationships.

## Known Limitations

- File-upload submissions are not supported in this version; submissions are text-based.
- No email notifications for deadlines or grading (listed as optional in the brief).
- No pagination/advanced filtering on list views (listed as optional in the brief).
- Not deployed to a live URL; local setup only.
