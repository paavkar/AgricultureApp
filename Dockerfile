# Use the official Nginx image as the base image
FROM nginx:stable-alpine

# Remove the default Nginx config file
RUN rm -f /etc/nginx/conf.d/default.conf

# Copy the custom Nginx configuration into the container
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY /ssl/cert.pem /etc/nginx/ssl/cert.pem
COPY /ssl/key.pem /etc/nginx/ssl/key.pem

# Expose ports 80, 443, 8080, 8081
EXPOSE 80 443 8080 8081

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]