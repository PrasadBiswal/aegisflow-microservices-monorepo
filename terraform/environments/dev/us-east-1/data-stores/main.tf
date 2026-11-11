# Off by default: in the sandbox SQL Server runs inside EKS. Turn on with -var="enable_rds=true".
module "sqlserver" {
  count                      = var.enable_rds ? 1 : 0
  source                     = "../../../../modules/data-stores/rds-sqlserver"
  identifier                 = "aegisflow-dev-sqlserver"
  vpc_id                     = data.terraform_remote_state.networking[0].outputs.vpc_id
  subnet_ids                 = data.terraform_remote_state.networking[0].outputs.private_subnet_ids
  allowed_security_group_ids = [data.terraform_remote_state.applications[0].outputs.cluster_security_group_id]
}

output "rds_address" {
  description = "Use as DB_SERVER when enable_rds = true."
  value       = var.enable_rds ? module.sqlserver[0].address : null
}

output "rds_master_user_secret_arn" {
  value = var.enable_rds ? module.sqlserver[0].master_user_secret_arn : null
}
