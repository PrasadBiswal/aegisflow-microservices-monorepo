# Account-wide IAM: lets GitHub Actions (dev and main branches only) log in to AWS without stored keys.
module "github_oidc" {
  source          = "../../../../modules/oidc"
  github_org      = var.github_org
  repository_name = var.repository_name
}

output "github_actions_role_arn" {
  description = "Set as GitHub repository variable AWS_ROLE_ARN (Settings > Secrets and variables > Actions > Variables)."
  value       = module.github_oidc.role_arn
}
