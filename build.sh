#!/bin/bash

# Check if an argument is provided
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <image-name>"
    exit 1
fi

IMAGE_NAME="$1"
VERSION_FILE="versions.txt"

# Check if the version file exists
if [ ! -f "$VERSION_FILE" ]; then
    echo "Version file not found. Please create $VERSION_FILE."
    exit 1
fi

# Read the current version for the specified image
LINE=$(grep "$IMAGE_NAME" "$VERSION_FILE")

if [ -z "$LINE" ]; then
    echo "Image '$IMAGE_NAME' not found in $VERSION_FILE."
    exit 1
fi

# Extract the Dockerfile location and current version
IFS=':' read -r dockerfile_location image version <<< "$LINE"

# Increment the version (simple patch version increment)
IFS='.' read -r major minor patch <<< "$version"
patch=$((patch + 1))
NEW_VERSION="$major.$minor.$patch"

# Update the version in the file
sed -i "s|^$dockerfile_location:$image:.*|$dockerfile_location:$image:$NEW_VERSION|" "$VERSION_FILE"

# Build the Docker image with the new version
docker build -t "$image:$NEW_VERSION" -f "$dockerfile_location" .

# Load the image into Minikube
minikube image load "$image:$NEW_VERSION"

# Output the new version
echo "Built and loaded image: $image:$NEW_VERSION"