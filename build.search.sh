docker build -t search:latest -f Search/Dockerfile .
minikube image load search:latest