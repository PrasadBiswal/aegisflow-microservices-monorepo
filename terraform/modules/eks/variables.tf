variable "cluster_name" {
  type        = string
  description = "The name of the EKS cluster"
}

variable "kubernetes_version" {
  type        = string
  description = "Desired Kubernetes master version"
  default     = "1.28"
}

variable "vpc_id" {
  type        = string
  description = "The VPC ID where the EKS cluster will be deployed"
}

variable "subnet_ids" {
  type        = list(string)
  description = "List of subnet IDs for EKS worker nodes and control plane interfaces"
}

variable "node_instance_types" {
  type        = list(string)
  description = "EC2 instance types for the EKS managed node group"
  default     = ["t3.medium"]
}

variable "min_size" {
  type        = number
  description = "Minimum number of worker nodes in the auto-scaling group"
  default     = 1
}

variable "max_size" {
  type        = number
  description = "Maximum number of worker nodes in the auto-scaling group"
  default     = 3
}

variable "environment" {
  type        = string
  description = "Target deployment environment (dev, staging, prod)"
}
