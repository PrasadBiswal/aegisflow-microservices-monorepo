data "terraform_remote_state" "networking" {
  count   = var.enable_rds ? 1 : 0
  backend = "s3"

  config = {
    bucket = var.state_bucket
    key    = "dev/us-east-1/networking/terraform.tfstate"
    region = "us-east-1"
  }
}

data "terraform_remote_state" "applications" {
  count   = var.enable_rds ? 1 : 0
  backend = "s3"

  config = {
    bucket = var.state_bucket
    key    = "dev/us-east-1/applications/terraform.tfstate"
    region = "us-east-1"
  }
}
