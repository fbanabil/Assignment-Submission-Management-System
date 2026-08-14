# Multi-stage Dockerfile for Next.js Frontend Application

# Stage 1: Base image
FROM node:20-alpine AS base

# Stage 2: Install dependencies
FROM base AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Install dependencies based on package-lock.json
COPY package.json package-lock.json* ./
RUN npm ci

# Stage 3: Build the application
FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Pass build-time environment variables for Next.js client-side code
ARG NEXT_PUBLIC_API_URL=http://localhost:8080/api
ARG NEXT_PUBLIC_API_BASE_URL=http://localhost:8080/api

ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL
ENV NEXT_TELEMETRY_DISABLED=1

RUN npm run build

# Stage 4: Production runner
FROM base AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Create a non-root user and group
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy static public assets
COPY --from=builder /app/public ./public

# Create and set permissions for .next cache folder
RUN mkdir .next
RUN chown nextjs:nodejs .next

# Copy built application and required production node_modules
COPY --from=builder --chown=nextjs:nodejs /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./package.json

# Run as non-root user
USER nextjs

EXPOSE 3000


CMD ["npm", "start"]
