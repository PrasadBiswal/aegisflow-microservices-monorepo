variable "state_bucket" {
  type        = string
  description = "Terraform state bucket from the bootstrap stack (aegisflow-tfstate-<account-id>)."
}

variable "enable_rds" {
  type        = bool
  default     = false
  description = "false (sandbox): SQL Server runs as a pod in EKS. true (real account): create RDS SQL Server."
}
