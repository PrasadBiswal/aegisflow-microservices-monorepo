terraform {
  backend "s3" {
    bucket       = "aegisflow-terraform-state-dev"
    key          = "dev/us-east-1/applications/terraform.tfstate"
    region       = "us-east-1"
    use_lockfile = true
    encrypt      = true
  }
}
