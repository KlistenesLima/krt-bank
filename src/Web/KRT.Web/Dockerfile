# ========== Stage 1: Build ==========
FROM node:20-alpine AS build
WORKDIR /app

# Cache de dependencias
COPY src/Web/KRT.Web/package*.json ./
RUN npm ci --silent

# Copiar source e buildar
COPY src/Web/KRT.Web/ ./
RUN npx ng build --configuration=production

# ========== Stage 2: Serve com Nginx ==========
FROM nginx:alpine
COPY --from=build /app/dist/krt-bank-web/browser /usr/share/nginx/html
COPY src/Web/KRT.Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
