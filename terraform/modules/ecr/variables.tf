variable "repository_name" {
  type        = string
  description = "The name of the ECR repository for the microservice"
}

variable "image_tag_mutability" {
  type        = string
  description = "Image tag mutability setting (MUTABLE or IMMUTABLE)"
  default     = "MUTABLE"
}

variable "environment" {
  type        = string
  description = "Target deployment environment (dev, staging, prod)"
}
