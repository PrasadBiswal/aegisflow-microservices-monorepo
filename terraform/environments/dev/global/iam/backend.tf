terraform {
  # The bucket name contains the AWS account ID (new in every sandbox), so it is passed at init:
  #   terraform init -backend-config="bucket=aegisflow-tfstate-<account-id>"
  backend "s3" {
    key          = "dev/global/iam/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
    encrypt      = true
  }
}
