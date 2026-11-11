variable "github_org" {
  type        = string
  description = "Your GitHub organization or username (e.g., your-github-handle)"
}

variable "repository_name" {
  type        = string
  description = "The name of your GitHub repository (e.g., aegisflow-microservices-monorepo)"
  default     = "aegisflow-microservices-monorepo"
}

variable "environment" {
  type        = string
  description = "Target deployment environment (dev, staging, prod)"
}
