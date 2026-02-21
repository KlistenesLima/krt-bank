# ========== Stage 1: Build ==========
FROM node:20-alpine AS build
WORKDIR /app

# Cache de dependencias (package*.json primeiro para cache de layers)
COPY package.json package-lock.json ./
RUN npm cache clean --force && npm ci

# Copiar source e buildar
COPY . .
ENV NODE_OPTIONS=--max_old_space_size=4096
RUN npx ng build --configuration=production

# ========== Stage 2: Serve com Nginx ==========
FROM nginx:alpine
COPY --from=build /app/dist/krt-bank-web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
