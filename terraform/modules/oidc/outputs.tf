output "role_arn" {
  description = "Set this as the GitHub repository variable AWS_ROLE_ARN."
  value       = aws_iam_role.github_actions.arn
}
