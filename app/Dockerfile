# Use the official Node.js image as the base image
FROM node:latest AS builder

ARG API_HOST=https://api.bonch.xfox111.net

# Set the working directory inside the container
WORKDIR /app

# Copy the package.json and package-lock.json files to the working directory
COPY package*.json ./

# Install the app dependencies
RUN npm install

# Copy the app source code to the working directory
COPY . .

RUN echo "VITE_BACKEND_HOST=${API_HOST}" > .env.production

# Build the app
RUN npm run build

# Use the official Nginx image as the base image
FROM nginx:latest AS prod

# Copy the build output to replace the default Nginx contents
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose port 80
CMD ["nginx", "-g", "daemon off;"]
