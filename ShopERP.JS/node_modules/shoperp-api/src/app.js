import cors from 'cors';
import express from 'express';
import morgan from 'morgan';
import { apiRouter } from './routes/index.js';

export const app = express();

app.use(cors());
app.use(express.json({ limit: '2mb' }));
app.use(morgan('dev'));

app.get('/health', (_req, res) => {
  res.json({ ok: true, service: 'shoperp-api' });
});

app.use('/api', apiRouter);

app.use((err, _req, res, _next) => {
  console.error(err);
  res.status(500).json({ message: 'Internal server error' });
});
