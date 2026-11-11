resource "aws_db_subnet_group" "this" {
  name       = "${var.identifier}-subnets"
  subnet_ids = var.subnet_ids
}

resource "aws_security_group" "this" {
  name        = "${var.identifier}-sg"
  description = "SQL Server access from the EKS cluster only"
  vpc_id      = var.vpc_id
}

resource "aws_vpc_security_group_ingress_rule" "from_allowed" {
  for_each                     = toset(var.allowed_security_group_ids)
  security_group_id            = aws_security_group.this.id
  referenced_security_group_id = each.value
  ip_protocol                  = "tcp"
  from_port                    = 1433
  to_port                      = 1433
}

resource "aws_db_instance" "this" {
  identifier                  = var.identifier
  engine                      = var.engine
  engine_version              = var.engine_version
  license_model               = "license-included"
  instance_class              = var.instance_class
  allocated_storage           = var.allocated_storage
  max_allocated_storage       = var.max_allocated_storage
  storage_type                = "gp3"
  storage_encrypted           = var.storage_encrypted
  username                    = var.username
  manage_master_user_password = true # password is generated and stored in AWS Secrets Manager
  db_subnet_group_name        = aws_db_subnet_group.this.name
  vpc_security_group_ids      = [aws_security_group.this.id]
  publicly_accessible         = false
  multi_az                    = var.multi_az
  backup_retention_period     = var.backup_retention_period
  deletion_protection         = var.deletion_protection
  skip_final_snapshot         = var.skip_final_snapshot
}
