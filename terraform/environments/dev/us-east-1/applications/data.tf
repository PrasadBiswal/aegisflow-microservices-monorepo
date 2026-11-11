# Reads the VPC and subnet IDs created by the networking stack.
data "terraform_remote_state" "networking" {
  backend = "s3"

  config = {
    bucket = var.state_bucket
    key    = "dev/us-east-1/networking/terraform.tfstate"
    region = "us-east-1"
  }
}
