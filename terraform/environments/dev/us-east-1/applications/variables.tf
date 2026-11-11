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

variable "services" {
  type        = list(string)
  description = "One ECR repository per service; names must match what CI pushes."
  default = [
    "users-api", "catalog-api", "inventory-api", "cart-api", "orders-api",
    "payments-api", "shipping-api", "notifications-api", "reviews-api", "reporting-api",
  ]
}
