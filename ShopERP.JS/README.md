# ShopERP JavaScript Migration

This workspace contains a JavaScript migration of ShopERP:

- `apps/api`: Node.js + Express + Prisma API
- `apps/desktop`: Electron + React desktop app

## Quick Start

1. Install dependencies:
   - `npm install`
2. Configure API environment:
   - copy `apps/api/.env.example` to `apps/api/.env`
   - set real MySQL credentials in `DB_USER` and `DB_PASSWORD`
3. Create database and client:
   - create MySQL database `shoperp`
   - `npm run db:generate`
   - `npm run db:migrate`
4. Run API:
   - `npm run dev:api`
5. Run Desktop:
   - `npm run dev:desktop`

## Startup Behavior

- Desktop auto-starts API process when Electron opens
- API performs startup system checks:
  - create DB if missing
  - sync Prisma schema (`prisma db push`)
  - seed default admin if no users exist
- Login opens once health checks are complete

## Current Migration Coverage

- Auth, products, suppliers, sales, purchases, stock basics
- Endpoint structure for remaining modules is scaffolded and ready for incremental parity work
- Electron shell with login and module navigation


npm run dev:desktop
