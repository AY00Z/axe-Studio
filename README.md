# axe-Studio

# Centralized Authentication and Configuration Management System for ACP Suite

## Overview

This project implements a centralized authentication and configuration management system for the ACP Suite, which includes multiple solutions like ACP Web, BRE, BRM, GUI, GLUE, and future BPM Web. The system provides a single sign-on (SSO) experience and centralized configuration management for all applications in the suite.

## Features

- **Unified Authentication**: Single login interface for all ACP Suite applications
- **Multi-Application Access**: After authentication, users can select which application to access
- **Centralized Configuration**: Administrative interface for dynamic configuration management
- **Seamless Navigation**: Ability to return to the central portal without re-authentication
- **Security Features**: JWT sessions and data encryption

## Technical Stack

### Backend
- .NET 9 (ASP.NET Core)

### Frontend
- Angular 18

### Authentication
- OAuth2/OpenID Connect
- JWT session management

### Infrastructure
- RESTful APIs

## Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js (for Angular


### Installation

```bash
# 1. Clone repo
git clone https://github.com/AY00Z/axe-Studio.git
cd axe-Studio



