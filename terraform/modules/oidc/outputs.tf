output "role_arn" {
  value       = aws_iam_role.github_actions_role.arn
  description = "The ARN of the IAM Role assumed by GitHub Actions workflows"
}

output "provider_arn" {
  value       = aws_iam_openid_connect_provider.github.arn
  description = "The ARN of the GitHub OIDC Provider"
}
