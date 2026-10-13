# axe-Studio

## Centralized Authentication and Configuration Management System for ACP Suite

---

## Overview

**axe-Studio** is a centralized platform that unifies authentication and configuration management for the **ACP Suite**, a set of financial-domain applications that includes ACP Web, BRE, BRM, GUI, GLUE, and the upcoming BPM Web.

Previously, each application in the suite managed its own authentication and configuration, which led to duplicated logic, inconsistent user experiences, and operational overhead. **axe-Studio** addresses these issues by providing a single entry point that handles user authentication once and grants access to every application in the suite, while centralizing the configuration of each project in a unified administrative interface.

The platform is built around two core pillars:

- **Single Sign-On (SSO)** across all ACP Suite applications
- **Centralized configuration management** for every project in the domain

---

## Context

The ACP Suite is composed of multiple independent applications operating in the **finance domain** (ACP, BRE, BRM, GUI, GLUE, BPM Web, and others). Each application historically required:

- Its own login mechanism
- Its own session handling
- Its own project configuration files

This fragmentation created several problems:

- Users had to authenticate multiple times when navigating between applications
- Configuration was scattered and difficult to maintain
- Adding a new application to the suite required reimplementing authentication and configuration logic

**axe-Studio** was designed to solve these problems by introducing a **central authentication and configuration hub** that all applications rely on.

---

## Features

- **Unified Authentication**: A single login interface for every application in the ACP Suite.
- **Multi-Application Access**: After authentication, users select the application they want to access from a central portal.
- **Single Sign-On (SSO)**: Once authenticated, users can navigate between applications without re-entering credentials.
- **Centralized Configuration**: An administrative interface for managing project configurations dynamically.
- **Seamless Navigation**: Users can return to the central portal at any time without re-authentication.
- **Secure Token Transmission**: JWT tokens are propagated between applications to maintain session continuity.
- **Security**: OAuth2/OpenID Connect flow, JWT session management, and encrypted data transmission.

---

## Architecture

The system follows a **Clean Architecture** approach on the backend, ensuring a clear separation of concerns and long-term maintainability.

### Backend (.NET 9 - Clean Architecture)

The backend is organized into the following layers:

- **Domain**
  Contains the core business entities and interfaces, independent of any external framework.
  - `Entities/Auth` — `AuthCode`, `UserModel`
  - `Entities/Project` — `ProjectConfig`, `ProjectsConfig`, `ConfigKeyMapping`
  - `Interfaces/Services` — `IConfigFilterService`, `IConfigUpdateService`, `IProjectsConfigService`

- **ApplicationCore**
  Contains the application use cases, commands, queries, and DTOs.
  - `Auth` — login models, requests, responses
  - `Project/Commands` — `CreateProjectCommand`, `UpdateProjectCommand`, `DeleteProjectCommand` with their handlers
  - `Project/Queries` — `GetAllProjectsQuery`, `GetProjectByPathQuery` with their handlers
  - `Project/Requests` — configuration and project request models

- **Infrastructure**
  Implements the interfaces defined in the Domain layer.
  - `Repositories` — `IProjectsConfigRepository`, `ProjectsConfigRepository`
  - `Services` — `ConfigFilterService`, `ConfigUpdateService`, `ProjectsConfigService`

- **WebApi**
  Exposes the REST endpoints consumed by the frontend.
  - `AuthController` — authentication and token validation
  - `ProjectsConfigController` — project configuration management
  - `ProjectsCrudController` — CRUD operations on projects
  - `URLController` — URL and routing management

### Frontend (Angular 18)

The frontend is organized into two main areas:

- **Public**
  Handles everything related to authentication:
  - `login` component
  - `auth.service`, `AuthGuard`, `LoggedInGuard`
  - Login request/response models

- **Private**
  Handles the authenticated experience:
  - `dashboard`, `project`, `project-card`, `project-details`
  - `add-project-form`, `config-center`
  - `navbar`, `sidebar`, `popup-menu`
  - Services for project configuration and URL management

---

## Authentication Flow

1. The user opens the central portal (axe-Studio frontend).
2. The user authenticates through the unified login page.
3. The backend validates the credentials and issues a **JWT token**.
4. The token is stored and used to authorize access to the available applications.
5. When the user selects an application, the JWT token is **transmitted** to that application.
6. Each application validates the token (via the backend) and grants access without requiring a new login.
7. The user can return to the central portal at any time without re-authenticating.

This flow ensures a consistent SSO experience across all applications in the ACP Suite.

---

## Technical Stack

### Backend
- .NET 9 (ASP.NET Core)
- Clean Architecture
- RESTful APIs

### Frontend
- Angular 18
- TypeScript
- SCSS

### Authentication
- OAuth2 / OpenID Connect
- JWT session management
- Token transmission between applications

### Infrastructure
- RESTful communication between frontend and backend
- JSON-based configuration files for project settings

---


---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS version recommended)
- Angular CLI (optional but recommended)

### Backend

```bash
cd backend
dotnet restore
dotnet run --project WebApi
```


### Frontend 

http://localhost:4200




