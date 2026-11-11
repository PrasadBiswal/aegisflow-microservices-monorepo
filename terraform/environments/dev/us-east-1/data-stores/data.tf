data "terraform_remote_state" "networking" {
  backend = "s3"
  config = {
    bucket = "aegisflow-terraform-state-dev"
    key    = "dev/us-east-1/networking/terraform.tfstate"
    region = "us-east-1"
  }
}
