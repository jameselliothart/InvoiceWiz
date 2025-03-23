docker build -t apigateway:latest -f APIGateway/Dockerfile .
minikube image load apigateway:latest