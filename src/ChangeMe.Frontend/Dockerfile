FROM node:24-alpine AS build
WORKDIR /app

COPY src/ChangeMe.Frontend/package*.json ./
RUN npm ci

COPY src/ChangeMe.Frontend/ ./
RUN npm run build

FROM nginx:1.29-alpine AS final
WORKDIR /usr/share/nginx/html

COPY src/ChangeMe.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY src/ChangeMe.Frontend/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
COPY --from=build /app/dist/ChangeMe.Frontend/browser/ ./

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
