output "repository_url" {
  value       = aws_ecr_repository.repo.repository_url
  description = "The URL of the created ECR repository"
}

output "repository_arn" {
  value       = aws_ecr_repository.repo.arn
  description = "The ARN of the created ECR repository"
}
