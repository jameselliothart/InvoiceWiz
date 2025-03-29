FROM nginx:alpine
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
COPY ./index.html /usr/share/nginx/html/
COPY ./index.js /usr/share/nginx/html/
COPY ./style.css /usr/share/nginx/html/

# Expose port 80 to serve the app
EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]