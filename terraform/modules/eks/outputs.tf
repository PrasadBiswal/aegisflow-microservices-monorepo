output "cluster_name" {
  value = aws_eks_cluster.main.name
}

output "cluster_endpoint" {
  value = aws_eks_cluster.main.endpoint
}

output "cluster_certificate_authority_data" {
  value = aws_eks_cluster.main.certificate_authority[0].data
}

output "cluster_security_group_id" {
  description = "Security group of the cluster and its nodes (used to allow database access from pods)."
  value       = aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
}

output "oidc_issuer_url" {
  description = "For IAM roles for service accounts (IRSA)."
  value       = aws_eks_cluster.main.identity[0].oidc[0].issuer
}
