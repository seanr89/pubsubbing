
.DEFAULT_GOAL := help


help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m   %s\n", $$1, $$2}'

docker-build: ## Build the docker image
	@echo "Building Docker image"
	cd externalapi && docker build -t externalapi:latest .

# run-local-rabbit: ## Run RabbitMQ locally
# 	@echo "Running RabbitMQ locally"
# 	docker run -d --hostname my-rabbitmq-server --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

run-masstransit: ## Run the MassTransit consumer
	@echo "Running MassTransit consumer"
	docker run -p 15672:15672 -p 5672:5672 masstransit/rabbitmq

run-masstransit-arm: ## Run the MassTransit consumer on ARM64
	docker run --platform linux/arm64 -p 15672:15672 -p 5672:5672 masstransit/rabbitmq

