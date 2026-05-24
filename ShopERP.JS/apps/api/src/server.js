import dotenv from 'dotenv';
import { app } from './app.js';
import { runSystemCheck } from './bootstrap/systemCheck.js';

dotenv.config();

const port = Number(process.env.PORT || 5050);

async function start() {
  await runSystemCheck();
  app.listen(port, () => {
    console.log(`ShopERP JS API listening on http://localhost:${port}`);
  });
}

start().catch((error) => {
  console.error('Startup system check failed:', error);
  process.exit(1);
});
