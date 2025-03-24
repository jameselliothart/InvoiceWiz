docker build -t persister:latest -f Persister/Dockerfile .
minikube image load persister:latest