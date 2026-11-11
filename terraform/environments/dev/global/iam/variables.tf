variable "github_org" {
  type        = string
  description = "Your GitHub username or organization name"
}

variable "repository_name" {
  type        = string
  description = "The name of your GitHub repository"
  default     = "aegisflow-microservices-monorepo"
}
