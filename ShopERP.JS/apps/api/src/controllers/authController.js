import { z } from 'zod';
import { createShopAndAdmin, createUser, login } from '../services/authService.js';

const loginSchema = z.object({
  username: z.string().min(1),
  password: z.string().min(1)
});

const createShopSchema = z.object({
  shopName: z.string().min(2),
  username: z.string().min(3),
  password: z.string().min(6),
  displayName: z.string().optional()
});

const createUserSchema = z.object({
  shopId: z.string().min(1),
  username: z.string().min(3),
  password: z.string().min(6),
  role: z.enum(['ADMIN', 'MANAGER', 'STAFF']).default('STAFF'),
  displayName: z.string().optional()
});

export async function loginAction(req, res) {
  const parsed = loginSchema.safeParse(req.body);
  if (!parsed.success) return res.status(400).json(parsed.error.flatten());

  const result = await login(parsed.data);
  if (!result) return res.status(401).json({ message: 'Invalid credentials' });

  return res.json({
    token: result.token,
    user: {
      id: result.user.id,
      username: result.user.username,
      role: result.user.role,
      shopId: result.user.shopId,
      displayName: result.user.displayName
    }
  });
}

export async function createShopAction(req, res) {
  const parsed = createShopSchema.safeParse(req.body);
  if (!parsed.success) return res.status(400).json(parsed.error.flatten());

  try {
    const shop = await createShopAndAdmin(parsed.data);
    return res.status(201).json({ shopId: shop.id, adminId: shop.users[0].id });
  } catch (error) {
    if (error?.code === 'P2002') {
      return res.status(409).json({ message: 'Username already exists' });
    }

    return res.status(500).json({ message: 'Unable to create account' });
  }
}

export async function createUserAction(req, res) {
  const parsed = createUserSchema.safeParse(req.body);
  if (!parsed.success) return res.status(400).json(parsed.error.flatten());

  try {
    const user = await createUser(parsed.data);
    return res.status(201).json({ id: user.id, username: user.username, role: user.role });
  } catch (error) {
    if (error?.code === 'P2002') {
      return res.status(409).json({ message: 'Username already exists' });
    }

    return res.status(500).json({ message: 'Unable to create user' });
  }
}
