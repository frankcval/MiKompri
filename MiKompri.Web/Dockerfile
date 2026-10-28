# syntax=docker/dockerfile:1.7

FROM node:24-alpine AS builder
WORKDIR /app

COPY MiKompri.Web/package*.json ./
RUN npm ci

COPY MiKompri.Web/ ./
RUN npm run build

FROM nginx:1.27-alpine AS runtime
WORKDIR /usr/share/nginx/html

RUN rm -rf ./*
COPY --from=builder /app/dist ./
COPY MiKompri.Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
