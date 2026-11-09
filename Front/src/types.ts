import { z } from 'zod'

export const roleSchema = z.object({ id: z.number(), name: z.string() })

export const userSchema = z.object({
  id: z.number(),
  username: z.string(),
  roles: z.array(z.string()),
  createdAtUtc: z.string(),
})

export const sessionSchema = z.object({
  accessToken: z.string(),
  expiresAtUtc: z.string(),
  user: userSchema,
})

export type Role = z.infer<typeof roleSchema>
export type User = z.infer<typeof userSchema>
export type Session = z.infer<typeof sessionSchema>
export type ProblemDetails = { title?: string; detail?: string; status?: number; traceId?: string }

export const superAdminRole = 'SuperAdmin'
