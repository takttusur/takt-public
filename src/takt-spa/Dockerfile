FROM node:20-alpine as builder
LABEL org.opencontainers.image.source="https://github.com/takttusur/takt-public"
EXPOSE 80
ENV PATH /app/node_modules/.bin:$PATH
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --silent
COPY . .
ENV NODE_ENV production
RUN npm run build

FROM nginx:stable-alpine
WORKDIR /usr/share/nginx/html
RUN rm -rf ./*
COPY --from=builder /app/dist .
CMD ["nginx", "-g", "daemon off;"]