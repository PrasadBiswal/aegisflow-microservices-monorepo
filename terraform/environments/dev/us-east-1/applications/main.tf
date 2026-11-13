module "ecr" {
  source           = "../../../../modules/ecr"
  repository_names = var.services
}

# 3 x t3.medium: 10 services + in-cluster SQL Server (~2 GB) + ArgoCD fit with room to spare.
# KodeKloud sandbox limit: max 3 nodes per node group, so max_size must stay 3.
module "eks" {
  source             = "../../../../modules/eks"
  cluster_name       = var.cluster_name
  kubernetes_version = var.kubernetes_version
  cluster_role_name  = var.cluster_role_name
  node_role_name     = var.node_role_name
  node_mode          = var.node_mode

  node_instance_profile_name = var.node_instance_profile_name
  subnet_ids                 = data.terraform_remote_state.networking.outputs.private_subnet_ids
  node_instance_types        = ["t3.medium"]
  node_desired_size          = 3
  node_min_size              = 2
  node_max_size              = 3
}

output "cluster_name" {
  description = "aws eks update-kubeconfig --name <this> --region us-east-1"
  value       = module.eks.cluster_name
}

output "cluster_endpoint" {
  value = module.eks.cluster_endpoint
}

output "cluster_security_group_id" {
  value = module.eks.cluster_security_group_id
}

output "ecr_repository_urls" {
  value = module.ecr.repository_urls
}
