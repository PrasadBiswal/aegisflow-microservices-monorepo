variable "github_org" {
  type        = string
  default     = "PrasadBiswal"
  description = "GitHub user or organization that owns the repository (case-sensitive)."
}

variable "repository_name" {
  type    = string
  default = "aegisflow-microservices-monorepo"
}
