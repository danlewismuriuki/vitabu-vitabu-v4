# S0 — Health check

```bash
curl -s http://localhost:5080/health
```

Expected:

```json
{"status":"ok","service":"vitabu-api","utc":"..."}
```

Infra (from repo root):

```bash
docker compose -f infra/docker-compose.yml up -d
```

- Postgres: `localhost:5433` (vitabu/vitabu)
- MinIO API: `localhost:9000` (console `:9001`)
- Mailpit UI: `http://localhost:8025`
- API: `http://localhost:5080`
