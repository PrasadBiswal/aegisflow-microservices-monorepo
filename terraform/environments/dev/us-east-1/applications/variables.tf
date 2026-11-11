variable "cluster_name" {
  type        = string
  description = "Name of the EKS cluster"
  default     = "aegisflow-dev-cluster"
}

variable "kubernetes_version" {
  type        = string
  description = "Kubernetes version for EKS"
  default     = "1.31"
}
