# 1. Container Registries (ECR) for Microservices
module "ecr_repositories" {
  source      = "../../../../modules/ecr"
  environment = "dev"
  repository_names = [
    "users-api", "catalog-api", "inventory-api", "cart-api", "orders-api",
    "payments-api", "shipping-api", "notifications-api", "reviews-api", "reporting-api"
  ]
}

# 2. Kubernetes Cluster (EKS)
module "eks_cluster" {
  source              = "../../../../modules/eks"
  cluster_name        = var.cluster_name
  kubernetes_version  = var.kubernetes_version
  environment         = "dev"
  vpc_id              = data.terraform_remote_state.networking.outputs.vpc_id
  subnet_ids          = data.terraform_remote_state.networking.outputs.private_subnet_ids
  node_instance_types = ["t3.medium"]
  min_size            = 2
  max_size            = 4
}

output "eks_cluster_endpoint" {
  value       = module.eks_cluster.cluster_endpoint
  description = "EKS Cluster Endpoint"
}

output "ecr_repository_urls" {
  value       = module.ecr_repositories.repository_urls
  description = "Map of ECR repository URLs for the 10 microservices"
}
