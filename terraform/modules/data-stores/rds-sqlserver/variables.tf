variable "environment" {
  type        = string
  description = "Deployment environment (e.g., dev, prod)"
}
variable "username" {
  type        = string
  description = "Master username for SQL Server"
  default     = "adminuser"
}

variable "password" {
  type        = string
  description = "Master password for SQL Server"
  sensitive   = true
}

variable "vpc_id" {
  type        = string
  description = "VPC ID where RDS will be deployed"
}

variable "subnet_ids" {
  type        = list(string)
  description = "List of database subnet IDs"
}

variable "allowed_cidr_blocks" {
  type        = list(string)
  description = "CIDR blocks allowed to connect to SQL Server"
}

variable "instance_class" {
  type        = string
  default     = "db.t3.small"
  description = "RDS instance size"
}

variable "allocated_storage" {
  type        = number
  default     = 20
  description = "Allocated storage in GB"
}

variable "db_name" {
  type        = string
  default     = "AegisflowDb"
  description = "Initial database name"
}

