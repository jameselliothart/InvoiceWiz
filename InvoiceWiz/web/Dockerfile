FROM nginx:alpine
COPY . /usr/share/nginx/html

# Expose port 80 to serve the app
EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]