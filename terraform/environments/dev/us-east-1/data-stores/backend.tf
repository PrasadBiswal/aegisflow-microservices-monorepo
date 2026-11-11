terraform {
  # terraform init -backend-config="bucket=aegisflow-tfstate-<account-id>"
  backend "s3" {
    key          = "dev/us-east-1/data-stores/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
    encrypt      = true
  }
}
