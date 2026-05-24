import bcrypt from 'bcryptjs';
import mysql from 'mysql2/promise';
import { execSync } from 'child_process';
import { prisma } from '../lib/prisma.js';

function parseDatabaseUrl() {
  const dbUrl = process.env.DATABASE_URL;
  if (!dbUrl) {
    throw new Error('DATABASE_URL is not configured');
  }

  const url = new URL(dbUrl);
  return {
    host: url.hostname,
    port: Number(url.port || 3306),
    user: decodeURIComponent(url.username || 'root'),
    password: decodeURIComponent(url.password || ''),
    database: url.pathname.replace(/^\//, '')
  };
}

async function ensureDatabaseExists() {
  const db = parseDatabaseUrl();
  const connection = await mysql.createConnection({
    host: db.host,
    port: db.port,
    user: db.user,
    password: db.password,
    multipleStatements: false
  });

  try {
    await connection.query(`CREATE DATABASE IF NOT EXISTS \`${db.database}\``);
  } finally {
    await connection.end();
  }
}

function syncPrismaSchema() {
  execSync('npx prisma db push --skip-generate', {
    stdio: 'inherit',
    cwd: process.cwd()
  });
}

function applyPrismaMigrations() {
  execSync('npx prisma migrate deploy', {
    stdio: 'inherit',
    cwd: process.cwd()
  });
}

async function listTables() {
  const db = parseDatabaseUrl();
  const connection = await mysql.createConnection({
    host: db.host,
    port: db.port,
    user: db.user,
    password: db.password,
    database: db.database,
    multipleStatements: false
  });

  try {
    const [rows] = await connection.query(
      `
      SELECT TABLE_NAME
      FROM information_schema.TABLES
      WHERE TABLE_SCHEMA = ? AND TABLE_TYPE = 'BASE TABLE'
      `,
      [db.database]
    );
    return rows.map((row) => String(row.TABLE_NAME || '').toLowerCase());
  } finally {
    await connection.end();
  }
}

async function ensurePrismaSchemaSafe() {
  const tables = await listTables();
  const hasNoTables = tables.length === 0;
  const hasMigrationTable = tables.includes('_prisma_migrations');
  const hasPrismaCoreTables = tables.includes('shop') && tables.includes('user');

  if (hasNoTables) {
    console.log('System check: empty database detected, initializing Prisma schema.');
    syncPrismaSchema();
    return true;
  }

  if (hasMigrationTable) {
    console.log('System check: Prisma migrations detected, applying pending migrations.');
    applyPrismaMigrations();
    return true;
  }

  if (hasPrismaCoreTables) {
    console.log('System check: Prisma schema tables already present, skipping schema sync.');
    return true;
  }

  console.warn(
    'System check: existing non-Prisma tables detected; skipping Prisma schema sync to avoid data loss.'
  );
  return false;
}

async function ensureSeedAdmin() {
  const users = await prisma.user.count();
  if (users > 0) {
    return;
  }

  const shopName = process.env.DEFAULT_SHOP_NAME || 'Medical Store';
  const username = process.env.DEFAULT_ADMIN_USERNAME || 'admin';
  const password = process.env.DEFAULT_ADMIN_PASSWORD || 'password';
  const passwordHash = await bcrypt.hash(password, 10);

  await prisma.shop.create({
    data: {
      name: shopName,
      users: {
        create: {
          username,
          passwordHash,
          role: 'ADMIN',
          displayName: 'System Admin'
        }
      }
    }
  });
}

export async function runSystemCheck() {
  await ensureDatabaseExists();
  const prismaReady = await ensurePrismaSchemaSafe();
  if (prismaReady) {
    await ensureSeedAdmin();
  }
}
