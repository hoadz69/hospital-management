# Local Development

## Infrastructure

Start local infrastructure:

```powershell
.\infrastructure\scripts\dev-up.ps1
```

Stop local infrastructure:

```powershell
.\infrastructure\scripts\dev-down.ps1
```

## Frontend

Install frontend dependencies:

```powershell
cd frontend
npm install
```

Run apps:

```powershell
npm run dev:public
npm run dev:clinic
npm run dev:owner
```

Ports:

- Public Website: `5173`
- Clinic Admin: `5174`
- Owner Admin: `5175`
