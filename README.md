# 🧠 Habit Tracker (Microservices-based)

A scalable and modular **Habit Tracker** built with **.NET Core**, using a **microservices architecture** with **YARP Gateway**, **PostgreSQL**, **Kafka**, **Redis**, and **Docker Compose**.

---

## 📁 Project Structure

```plaintext
habit-tracker/
├── gateway/               # API Gateway (YARP + JWT validation)
├── user-service/          # User auth and profile management
├── habit-service/         # Habit creation, update, delete
├── tracking-service/      # Daily habit tracking, streaks
├── notification-service/  # Sends notifications based on events
├── migration-service/     # Runs DB migrations from .sql files
├── docker-compose.yml     # Orchestrates all services
├── .env                   # Environment config (not committed)
└── README.md              # This file
```

---

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/habit-tracker.git
cd habit-tracker
```

### 2. Create `.env` File

```ini
# .env
DB_HOST=postgres
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=habitdb

GOOGLE_CLIENT_ID=your_google_client_id
GOOGLE_CLIENT_SECRET=your_google_client_secret
JWT_SECRET=your_jwt_secret_key
```

> 🛡️ **Important:** Do not commit `.env` – it's already in `.gitignore`.

### 3. Run the App

```bash
docker compose up --build
```

This will spin up:

- PostgreSQL  
- Kafka + Zookeeper  
- Redis  
- All microservices (e.g., `gateway`, `user-service`, etc.)  
- Migration service to apply SQL schema

---

✅ You're ready to start building habits!
