provider "aws" {
  region = "us-east-1"
}

locals {
  environment = "dev"
  services    = ["users-api", "catalog-api", "inventory-api", "cart-api", "orders-api", "payments-api", "shipping-api", "notifications-api", "reviews-api", "reporting-api"]
}

module "ecr" {
  source        = "../../../modules/ecr"
  service_names = local.services
}

output "ecr_urls" {
  value       = module.ecr.repository_urls
  description = "Production ECR endpoints for Aegisflow microservices"
}
