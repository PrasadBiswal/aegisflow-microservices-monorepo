variable "identifier" {
  type        = string
  description = "RDS instance name, e.g. aegisflow-dev-sqlserver."
}

variable "vpc_id" {
  type = string
}

variable "subnet_ids" {
  type        = list(string)
  description = "Private subnet IDs."
}

variable "allowed_security_group_ids" {
  type        = list(string)
  description = "Security groups allowed to connect on port 1433 (e.g. the EKS cluster security group)."
}

variable "engine" {
  type        = string
  default     = "sqlserver-ex"
  description = "sqlserver-ex (Express, dev) or sqlserver-se (Standard; needed for Multi-AZ in prod)."
}

variable "engine_version" {
  type        = string
  default     = "15.00"
  description = "15.00 = SQL Server 2019, same as the local Docker Compose image."
}

variable "instance_class" {
  type    = string
  default = "db.t3.small"
}

variable "allocated_storage" {
  type    = number
  default = 20
}

variable "max_allocated_storage" {
  type    = number
  default = 100
}

variable "storage_encrypted" {
  type    = bool
  default = true
}

variable "username" {
  type    = string
  default = "adminuser"
}

variable "multi_az" {
  type    = bool
  default = false
}

variable "backup_retention_period" {
  type    = number
  default = 1
}

variable "deletion_protection" {
  type    = bool
  default = false
}

variable "skip_final_snapshot" {
  type    = bool
  default = true
}
