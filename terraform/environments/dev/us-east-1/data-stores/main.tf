module "sqlserver_db" {
  source            = "../../../../modules/data-stores/rds-sqlserver"
  environment       = "dev"
  vpc_id            = data.terraform_remote_state.networking.outputs.vpc_id
  subnet_ids        = data.terraform_remote_state.networking.outputs.private_subnet_ids
  allocated_storage = 20
  instance_class    = "db.t3.small"
  username          = "adminuser"
  password          = var.db_password
}

output "rds_sqlserver_endpoint" {
  value       = module.sqlserver_db.endpoint
  description = "Connection endpoint for SQL Server"
  sensitive   = true
}
