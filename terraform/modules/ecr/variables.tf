variable "repository_names" {
  type        = list(string)
  description = "One ECR repository per service (must match the service names CI pushes)."
}

variable "image_tag_mutability" {
  type        = string
  default     = "IMMUTABLE"
  description = "IMMUTABLE: a tag (git SHA) can never be overwritten."
}

variable "force_delete" {
  type        = bool
  default     = true
  description = "Allow destroy even when images exist (fine for sandbox/dev; set false in prod)."
}

variable "max_image_count" {
  type    = number
  default = 30
}
