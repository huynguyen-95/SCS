# Multi-stage Docker build for Angular application
# Stage 1: Build the application
FROM node:20-alpine AS build

# Set working directory
WORKDIR /app

# Copy package files first for better layer caching
COPY package.json yarn.lock ./

# Install dependencies
RUN yarn install --frozen-lockfile

# Copy source code
COPY . .

# Build the application for development
RUN yarn build

# Stage 2: Serve the application with nginx
FROM nginx:alpine AS production

# Set default port (can be overridden by environment variable)
ENV NGINX_PORT=4200

# Copy built application from build stage
COPY --from=build /app/dist/SCS.Client/browser /usr/share/nginx/html

# Copy nginx configuration template
COPY nginx.conf.template /etc/nginx/templates/nginx.conf.template

# Expose the configurable port
EXPOSE $NGINX_PORT

# Start nginx (envsubst will process the template automatically)
CMD ["nginx", "-g", "daemon off;"]
