import { prisma } from '../lib/prisma.js';
import fs from 'fs/promises';
import path from 'path';
import mysql from 'mysql2/promise';

function buildMysqlConnection(dbUrl) {
  const url = new URL(dbUrl);
  return {
    host: url.hostname,
    port: Number(url.port || '3306'),
    user: decodeURIComponent(url.username || 'root'),
    password: decodeURIComponent(url.password || ''),
    database: url.pathname.replace(/^\//, '')
  };
}

function escapeSqlValue(value) {
  if (value === null || value === undefined) return 'NULL';
  if (value instanceof Date) return `'${value.toISOString().slice(0, 19).replace('T', ' ')}'`;
  if (Buffer.isBuffer(value)) return `0x${value.toString('hex')}`;
  if (typeof value === 'number' || typeof value === 'bigint') return String(value);
  if (typeof value === 'boolean') return value ? '1' : '0';

  const normalized = String(value)
    .replace(/\\/g, '\\\\')
    .replace(/\r/g, '\\r')
    .replace(/\n/g, '\\n')
    .replace(/\t/g, '\\t')
    .replace(/'/g, "''");

  return `'${normalized}'`;
}

function buildInsertStatement(tableName, columns, row) {
  const columnSql = columns.map((column) => `\`${column}\``).join(', ');
  const valuesSql = columns.map((column) => escapeSqlValue(row[column])).join(', ');
  return `INSERT INTO \`${tableName}\` (${columnSql}) VALUES (${valuesSql});`;
}

async function dumpDatabase(connectionOptions, outputFile) {
  const connection = await mysql.createConnection({
    host: connectionOptions.host,
    port: connectionOptions.port,
    user: connectionOptions.user,
    password: connectionOptions.password,
    database: connectionOptions.database,
    multipleStatements: true
  });

  try {
    const sqlParts = [];
    const [tables] = await connection.query('SHOW FULL TABLES WHERE Table_type = "BASE TABLE"');
    const tableNames = tables
      .map((row) => Object.values(row)[0])
      .filter(Boolean)
      .sort((left, right) => left.localeCompare(right));

    sqlParts.push('-- ShopERP database backup');
    sqlParts.push(`-- Generated at ${new Date().toISOString()}`);
    sqlParts.push('SET FOREIGN_KEY_CHECKS=0;');
    sqlParts.push('SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";');
    sqlParts.push('');

    for (const tableName of tableNames) {
      const [createRows] = await connection.query(`SHOW CREATE TABLE \`${tableName}\``);
      const createStatement = createRows?.[0]?.['Create Table'];

      sqlParts.push(`DROP TABLE IF EXISTS \`${tableName}\`;`);
      if (createStatement) {
        sqlParts.push(`${createStatement};`);
      }
      sqlParts.push('');

      const [rows] = await connection.query(`SELECT * FROM \`${tableName}\``);
      if (rows.length > 0) {
        const columns = Object.keys(rows[0]);
        for (const row of rows) {
          sqlParts.push(buildInsertStatement(tableName, columns, row));
        }
        sqlParts.push('');
      }
    }

    sqlParts.push('SET FOREIGN_KEY_CHECKS=1;');
    await fs.writeFile(outputFile, sqlParts.join('\n'), 'utf8');
  } finally {
    await connection.end();
  }
}

export async function runBackup(req, res) {
  const dbUrl = process.env.DATABASE_URL;
  if (!dbUrl) {
    return res.status(500).json({ message: 'DATABASE_URL is not configured' });
  }

  const connection = buildMysqlConnection(dbUrl);
  const dbName = connection.database;

  const stamp = new Date().toISOString().replace(/[:.]/g, '-');
  const targetDir = req.body.destination || path.resolve(process.cwd(), 'backups');
  const outputFile = path.join(targetDir, `${dbName}-${stamp}.sql`);

  await fs.mkdir(targetDir, { recursive: true });

  try {
    await dumpDatabase(connection, outputFile);

    const row = await prisma.backupLog.create({
      data: {
        shopId: req.user.shopId,
        status: 'SUCCESS',
        destination: outputFile,
        message: 'Backup completed',
        createdById: req.user.sub
      }
    });

    return res.status(201).json(row);
  } catch (error) {
    let row = null;
    try {
      row = await prisma.backupLog.create({
        data: {
          shopId: req.user.shopId,
          status: 'FAILED',
          destination: outputFile,
          message: error.message,
          createdById: req.user.sub
        }
      });
    } catch (logError) {
      console.error('Failed to write backup log', logError);
    }

    return res.status(500).json({
      message: 'Backup failed',
      details:
        row?.message ||
        error.message ||
        'Backup could not complete. Check MySQL credentials and that the backup target folder is writable.'
    });
  }
}

export async function backupHistory(req, res) {
  const rows = await prisma.backupLog.findMany({
    where: { shopId: req.user.shopId },
    orderBy: { createdAt: 'desc' },
    take: 100
  });

  res.json(rows);
}
