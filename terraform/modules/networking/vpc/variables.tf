variable "environment" {
  type        = string
  description = "Environment name (dev, staging, prod)."
}

variable "cluster_name" {
  type        = string
  description = "EKS cluster name. Subnets are tagged with it so load balancers can find them."
}

variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}

variable "az_count" {
  type        = number
  default     = 2
  description = "Number of availability zones (one public and one private subnet each)."
}

variable "single_nat_gateway" {
  type        = bool
  default     = true
  description = "true = one NAT gateway (cheaper, dev/sandbox); false = one per AZ (prod)."
}
