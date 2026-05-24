import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import { prisma } from '../lib/prisma.js';

function signToken(user) {
  return jwt.sign(
    {
      sub: user.id,
      username: user.username,
      role: user.role,
      shopId: user.shopId
    },
    process.env.JWT_SECRET,
    { expiresIn: '12h' }
  );
}

export async function createShopAndAdmin({ shopName, username, password, displayName }) {
  const passwordHash = await bcrypt.hash(password, 10);

  return prisma.shop.create({
    data: {
      name: shopName,
      users: {
        create: {
          username,
          passwordHash,
          role: 'ADMIN',
          displayName
        }
      }
    },
    include: { users: true }
  });
}

export async function createUser({ shopId, username, password, role, displayName }) {
  const passwordHash = await bcrypt.hash(password, 10);
  return prisma.user.create({
    data: {
      shopId,
      username,
      passwordHash,
      role,
      displayName
    }
  });
}

export async function login({ username, password }) {
  const user = await prisma.user.findUnique({ where: { username } });
  if (!user) return null;

  const ok = await bcrypt.compare(password, user.passwordHash);
  if (!ok) return null;

  await prisma.user.update({
    where: { id: user.id },
    data: { lastLoginUtc: new Date() }
  });

  const token = signToken(user);
  return { token, user };
}
