output "address" {
  description = "Host name only (no port) - use as DB_SERVER."
  value       = aws_db_instance.this.address
}

output "master_user_secret_arn" {
  description = "Secrets Manager secret holding the generated admin password."
  value       = aws_db_instance.this.master_user_secret[0].secret_arn
}
