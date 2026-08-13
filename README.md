# 🌟 Tomouh (طموح) | Academic Scholarships Platform

> A modern, event-driven microservices platform built to simplify exploring, applying for, and managing academic scholarships.

---

## 📖 Project Overview

**Tomouh (طموح)** — Arabic for *"Ambition"* — is a personal project that showcases a production-grade, scalable microservice architecture combined with **Polyglot Persistence** (multi-database strategy) and modern security standards.

The platform connects students with funding organizations, enabling them to discover scholarships, verify eligibility, track applications, and engage in community discussions — all while being observed by an AI-assisted moderation layer.

### Highlights

- **SSO with Asymmetric Key Encryption** — Auth service signs JWTs with a private key (RS256); downstream services verify asynchronously with a public key.
- **Distributed Messaging** — RabbitMQ enables reliable, decoupled communication between services.
- **AI-Assisted Content Moderation** — Automated safety checks for community content.
- **Cost-Optimized Hybrid Cloud Deployment** — Azure + free-tier container platforms keep operating costs near zero.

---

## 🏗️ Architectural Highlights

| Capability | Approach |
|---|---|
| **Microservices Architecture** | Fully decoupled domain services with independent data stores and lifecycles. |
| **Polyglot Persistence** | MS SQL Server for relational/transactional data; MongoDB for high-throughput, flexible-schema data. |
| **Asymmetric Security** | The Auth service issues JWT access tokens signed via an **RS256 private key**. Every downstream service validates them using only the **public key** — no shared secrets, no network calls on each request. |
| **Event-Driven Communication** | Services communicate asynchronously via **RabbitMQ** topics/queues, improving resilience and scalability. |
| **Hybrid Cloud Deployment** | Cost-optimized setup blending Azure free tier with free-tier container platforms for long-running services. |

---

## 🖥️ System Architecture Diagram

```
                          ┌─────────────────────────┐
                          │     React Frontend      │
                          └───────────┬─────────────┘
                                      │ HTTPS
                          ┌───────────▼─────────────┐
                          │      API Gateway        │
                          │  (Routing / AuthN AuthZ) │
                          └──────┬───────┬──────────┘
                                 │       │
                 ┌───────────────┘       └───────────────┐
                 │                                       │
    ┌────────────▼─────────────┐            ┌────────────▼─────────────┐
    │       Auth Service       │            │ Scholarship & Eligibility │
    │      (SSO / Identity)    │            │        Service            │
    └────────────┬─────────────┘            └────────────┬─────────────┘
                 │                                       │
        ┌────────▼────────┐                    ┌─────────▼─────────┐
        │     MongoDB      │                    │  MS SQL Server    │
        └─────────────────┘                    └───────────────────┘
                 │                                       │
    ┌────────────▼─────────────┐            ┌────────────▼─────────────┐
    │  Funding Organizations   │            │    Custom Notebook       │
    │         Service          │            │         Service          │
    └────────────┬─────────────┘            └────────────┬─────────────┘
                 │                                       │
        ┌────────▼────────┐                    ┌─────────▼─────────┐
        │  Relational DB   │                    │     MongoDB       │
        └─────────────────┘                    └───────────────────┘

                 ┌───────────────────────────────────────────────┐
                 │   Comments & Community Service                │
                 │            ┌───────────────────────┐          │
                 │            │   AI Moderation API   │          │
                 │            │   (content safety)    │          │
                 │            └───────────────────────┘          │
                 └────────────────────────┬──────────────────────┘
                                          │
                                 ┌────────▼─────────┐
                                 │     MongoDB       │
                                 └───────────────────┘

  All services communicate asynchronously through RabbitMQ (event bus).
```

---

## 🧩 Services Breakdown

### Auth Service
- **SSO & Identity Management** — registration, login, refresh-token rotation, email/password reset, and role-based profiles.
- **Asymmetric JWT (RS256)** — signs tokens with a private key; exposes the public key for downstream verification.
- **Database:** MongoDB.

### Scholarship & Eligibility Service
- **Core domain** — lists scholarships and evaluates applicant eligibility criteria.
- **Database:** MS SQL Server (relational, transactional integrity).

### Funding Organizations Service
- **Partner profiles** — manages organizations that sponsor scholarships.
- **Database:** Relational DB (MS SQL Server).

### Custom Notebook Service
- **User tracking & notes** — personalized scholarship shortlists, notes, and application progress.
- **Database:** MongoDB (flexible, high-throughput).

### Comments & Community Service
- **Q&A & discussions** — community interaction around scholarships.
- **AI Moderation Integration** — automatic content screening before publishing.
- **Database:** MongoDB.

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10 / C# |
| Architecture | DDD, CQRS, Clean Architecture |
| Frontend | React.js |
| Databases | MS SQL Server, MongoDB |
| Message Broker | RabbitMQ |
| Testing | xUnit, Moq, FluentAssertions, Selenium / Playwright |
| DevOps | Azure, Docker, GitHub Actions CI/CD |

---

## 🚀 Deployment & Testing Strategy

### Resource Optimization

- **Azure Free Tier** — hosts core services with zero monthly cost.
- **Render / Railway** — free-tier container hosting for supporting services.
- **MongoDB Atlas** — free-tier managed NoSQL clusters.
- **Hybrid placement** — workloads are split across providers based on cost, latency, and reliability needs.

### Automated CI/CD

- **GitHub Actions** pipelines run on every push:
  - **Unit Testing** — fast feedback for domain and application layers (xUnit + Moq + FluentAssertions).
  - **End-to-End (E2E) Testing** — Playwright / Selenium against the running frontend + backend stack.
  - **Docker Builds** — containerized artifacts ready for deployment.

---

## ✍️ Author

Built by **Ahmed Zain** as a backend architecture showcase project — demonstrating distributed systems, event-driven design, and modern security practices in a real-world domain.