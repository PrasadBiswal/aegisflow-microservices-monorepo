variable "db_password" {
  type        = string
  description = "Master password for SQL Server RDS instance"
  sensitive   = true
}
