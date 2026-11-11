variable "cluster_name" {
  type    = string
  default = "aegisflow-dev"
}

module "vpc" {
  source             = "../../../../modules/networking/vpc"
  environment        = "dev"
  cluster_name       = var.cluster_name
  az_count           = 2
  single_nat_gateway = true
}

output "vpc_id" {
  value = module.vpc.vpc_id
}

output "public_subnet_ids" {
  value = module.vpc.public_subnet_ids
}

output "private_subnet_ids" {
  value = module.vpc.private_subnet_ids
}
