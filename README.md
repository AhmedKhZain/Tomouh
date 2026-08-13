# 🌟 Tomouh (طموح) - Academic Scholarships Platform

> **A modern, event-driven microservices platform built to simplify exploring, applying for, and managing academic scholarships.**

---

## 📌 Project Overview

**Tomouh** is a personal side project designed to showcase scalable microservice architecture, multi-database strategies (Polyglot Persistence), and security standards. 

Since this platform is an experimental project focused on architectural excellence and modern backend practices, it serves as a testing ground for learning and implementing advanced patterns such as **Single Sign-On (SSO)** with **Asymmetric Key Encryption**, **Distributed Messaging**, **AI Integration**, and **Hybrid Cloud Deployment**.

---

## 🏛️ Architectural Highlights & Engineering Decisions

* **Microservices Architecture:** Fully decoupled services, each with its own domain logic and data boundary.
* **Polyglot Persistence:** Using the best database for each domain's requirements (SQL for structured relational data, NoSQL for high-throughput or flexible schema data).
* **Asymmetric Security:** Auth service handles SSO issuing JWT tokens signed with private keys, verified asynchronously by child services via public keys.
* **Event-Driven Communication:** Asynchronous messaging via RabbitMQ for decoupled cross-service interactions and audit workflows.
* **Hybrid Cloud Deployment:** Cost-optimized deployment utilizing Azure alongside free-tier container hosting solutions.

---

## 🧩 Core Services Architecture

```text
                  ┌────────────────────────┐
                  │   React Web Frontend   │
                  └───────────┬────────────┘
                              │
                    ┌─────────┴─────────┐
                    │    API Gateway    │
                    └─────────┬─────────┘
                              │
     ┌──────────────┬─────────┼─────────┬──────────────┐
     │              │                   │              │
┌────▼─────┐  ┌─────▼──────┐      ┌─────▼─────┐  ┌─────▼─────┐
│   Auth   │  │Scholarship │      │ Custom    │  │ Comments  │
│ Service  │  │  Service   │      │ Notebook  │  │ Service   │
└────┬─────┘  └─────┬──────┘      └─────┬─────┘  └─────┬─────┘
     │              │                   │              │
  [MongoDB]   [MS SQL Server]       [MongoDB]      [MongoDB]
                                                       │
                                                 [AI Moderation]



1. 🔐 Auth Service (SSO & Identity)Purpose: Handles central authentication, single sign-on (SSO), profile contexts, and dynamic permissions.Security: Uses Asymmetric JWT Encryption (RS256 / RSA Public-Private key pair).Database: MongoDB (NoSQL) for high scalability and flexible user metadata.2. 🎓 Scholarship & Eligibility ServicePurpose: Manages scholarship listings, application criteria, and funding requirements.Database: Microsoft SQL Server for strict relational consistency and structured querying.3. 🏢 Funding Organizations ServicePurpose: Manages partner profiles, foundations, and institutional scholarship providers.Database: Relational storage for structured organizational profiles.4. 📝 Custom Notebook ServicePurpose: Personal user space for tracking scholarship applications, deadlines, and private research notes.Database: MongoDB (NoSQL) for unstructured document storage per user.5. 💬 Comments & Community Service (AI Moderated)Purpose: Enables community discussions and Q&A on scholarship pages.AI Feature: Integrated with AI Agents / Content Moderation API to evaluate reported comments automatically and take moderation actions.Database: MongoDB (NoSQL) for optimized write throughput.🛠️ Technology StackLayerTechnologies UsedBackend Framework.NET 10 / C#ArchitectureDomain-Driven Design (DDD), CQRS, Clean ArchitectureFrontendReact.jsDatabasesMicrosoft SQL Server, MongoDBMessage BrokerRabbitMQTestingxUnit, Moq, FluentAssertions, Selenium / PlaywrightDevOps & CloudAzure, Docker, GitHub Actions CI/CD☁️ Deployment Strategy & Cost OptimizationTo manage cloud resources efficiently without exceeding Azure's free limits:Core Compute: Hosted on Azure App Services / Container Apps (Free Tier).Secondary Services: Distributed across cost-effective / free container hosting providers (e.g., Render, Railway, Vercel for Frontend).Databases: Cloud-managed free tiers (Azure SQL Free / MongoDB Atlas).🧪 Testing StrategyUnit Testing: Comprehensive domain logic coverage via xUnit and FluentAssertions.CI/CD Pipeline: Automated build and test workflows running via GitHub Actions on every pull request.End-to-End Testing: Automated browser and E2E testing using Selenium / Playwright.👨‍💻 AuthorBuilt with ❤️ by Ahmed Zain as a solo project exploring advanced backend engineering and system design.