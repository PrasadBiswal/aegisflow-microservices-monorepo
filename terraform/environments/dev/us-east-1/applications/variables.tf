variable "state_bucket" {
  type        = string
  description = "Terraform state bucket from the bootstrap stack (aegisflow-tfstate-<account-id>)."
}

variable "cluster_name" {
  type    = string
  default = "aegisflow-dev"
}

variable "kubernetes_version" {
  type        = string
  default     = null
  description = "null = the current EKS default version."
}

# KodeKloud sandbox only allows iam:PassRole for these role names. Set to null in a real account.
variable "cluster_role_name" {
  type    = string
  default = "eksClusterRole"
}

variable "node_role_name" {
  type    = string
  default = "EC2LabRole"
}

# KodeKloud sandbox: managed node groups are not allowed, so nodes are a self-managed Auto Scaling group,
# and only the instance profile name "ecsInstanceRole" may be created. Use "managed" / null in a real account.
variable "node_mode" {
  type    = string
  default = "self_managed"
}

variable "node_instance_profile_name" {
  type    = string
  default = "ecsInstanceRole"
}

variable "services" {
  type        = list(string)
  description = "One ECR repository per service; names must match what CI pushes."
  default = [
    "users-api", "catalog-api", "inventory-api", "cart-api", "orders-api",
    "payments-api", "shipping-api", "notifications-api", "reviews-api", "reporting-api",
  ]
}
