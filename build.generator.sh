docker build -t generator:latest -f Generator/Dockerfile .
minikube image load generator:latest