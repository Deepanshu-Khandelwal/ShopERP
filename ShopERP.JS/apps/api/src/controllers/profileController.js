import { prisma } from '../lib/prisma.js';

export async function getProfile(req, res) {
  const user = await prisma.user.findUnique({
    where: { id: req.user.sub },
    select: {
      id: true,
      username: true,
      role: true,
      displayName: true,
      lastLoginUtc: true,
      createdAt: true,
      updatedAt: true
    }
  });

  if (!user) return res.status(404).json({ message: 'User not found' });
  res.json(user);
}

export async function updateProfile(req, res) {
  const user = await prisma.user.update({
    where: { id: req.user.sub },
    data: {
      displayName: req.body.displayName,
      username: req.body.username
    },
    select: {
      id: true,
      username: true,
      role: true,
      displayName: true,
      lastLoginUtc: true,
      updatedAt: true
    }
  });

  res.json(user);
}
