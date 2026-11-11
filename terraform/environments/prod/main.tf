terraform {
  required_version = ">= 1.6.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
  default_tags {
    tags {
      Environment = "prod"
      Project     = "aegisflow"
      ManagedBy   = "terraform"
    }
  }
}

# 1. Call OIDC Module for GitHub Actions Keyless Auth
module "oidc" {
  source = "../../modules/oidc"
}

# 2. Call ECR Module to provision the 10 Microservice Repositories for Dev
module "ecr" {
  source = "../../modules/ecr"
}

# 3. Fetch Default VPC for Dev Sandbox
data "aws_vpc" "default" {
  default = true
}

data "aws_subnets" "default" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}

# 4. Call EKS Module with cost-optimized Dev node sizing
module "eks" {
  source     = "../../modules/eks"
  vpc_id     = data.aws_vpc.default.id
  subnet_ids = data.aws_subnets.default.ids
}

output "ecr_repository_urls" {
  value = module.ecr.repository_urls
}

output "eks_cluster_name" {
  value = module.eks.cluster_name
}
