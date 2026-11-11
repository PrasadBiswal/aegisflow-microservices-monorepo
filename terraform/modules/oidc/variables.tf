variable "github_org" {
  type        = string
  description = "GitHub user or organization (case-sensitive), e.g. PrasadBiswal."
}

variable "repository_name" {
  type    = string
  default = "aegisflow-microservices-monorepo"
}

variable "role_name" {
  type    = string
  default = "aegisflow-github-actions"
}

variable "allowed_branches" {
  type        = list(string)
  default     = ["dev", "main"]
  description = "Branches whose workflows may assume the role (feature branches and PRs cannot)."
}

variable "policy_arns" {
  type        = list(string)
  default     = ["arn:aws:iam::aws:policy/AdministratorAccess"]
  description = "Permissions for the CI role. Broad for the sandbox (Terraform creates IAM roles); scope down in real accounts."
}

variable "create_oidc_provider" {
  type        = bool
  default     = true
  description = "Set false if the account already has the GitHub OIDC provider."
}
