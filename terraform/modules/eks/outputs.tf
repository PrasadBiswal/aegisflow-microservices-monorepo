output "cluster_endpoint" {
  value       = aws_eks_cluster.cluster.endpoint
  description = "Endpoint for EKS control plane"
}

output "cluster_name" {
  value       = aws_eks_cluster.cluster.name
  description = "The name of the EKS cluster"
}

output "cluster_certificate_authority_data" {
  value       = aws_eks_cluster.cluster.certificate_authority[0].data
  description = "Base64 encoded certificate authority data for the cluster"
}

output "cluster_oidc_issuer_url" {
  value       = aws_eks_cluster.cluster.identity[0].oidc[0].issuer
  description = "The OIDC Issuer URL for the EKS cluster (used for IRSA)"
}
