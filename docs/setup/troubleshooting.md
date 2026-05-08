# Troubleshooting

- If Docker ports are busy, stop the conflicting service or edit `infrastructure/docker/docker-compose.dev.yml`.
- If frontend workspaces fail to resolve local packages, run `npm install` from `frontend/`.
- Do not add real production secrets to the repo.
