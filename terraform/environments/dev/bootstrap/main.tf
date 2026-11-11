# Run ONCE per AWS account (once per sandbox session), with local state, before any other stack:
#   terraform init && terraform apply
# Creates the S3 bucket that stores the state of all other stacks.

terraform {
  required_version = ">= 1.10.0"

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
    tags = {
      Project   = "Aegisflow"
      ManagedBy = "Terraform"
      Layer     = "Bootstrap"
    }
  }
}

data "aws_caller_identity" "current" {}

resource "aws_s3_bucket" "state" {
  bucket        = "aegisflow-tfstate-${data.aws_caller_identity.current.account_id}"
  force_destroy = true # sandbox: the account is wiped anyway
}

resource "aws_s3_bucket_versioning" "state" {
  bucket = aws_s3_bucket.state.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "state" {
  bucket = aws_s3_bucket.state.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "state" {
  bucket                  = aws_s3_bucket.state.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

output "state_bucket" {
  description = "Pass to every other stack: terraform init -backend-config=\"bucket=<this>\""
  value       = aws_s3_bucket.state.bucket
}
