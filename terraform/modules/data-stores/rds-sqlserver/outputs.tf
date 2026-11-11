output "endpoint" {
  value       = aws_db_instance.sqlserver.endpoint
  description = "The connection endpoint for the SQL Server instance"
}

output "resource_id" {
  value       = aws_db_instance.sqlserver.resource_id
  description = "The RDS resource ID"
}
