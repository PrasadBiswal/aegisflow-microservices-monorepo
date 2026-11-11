resource "aws_db_subnet_group" "rds" {
  name       = "${var.environment}-sqlserver-subnet-group"
  subnet_ids = var.subnet_ids

  tags = {
    Environment = var.environment
    Name        = "sqlserver-subnet-group"
  }
}

resource "aws_security_group" "rds" {
  name        = "${var.environment}-rds-sqlserver-sg"
  description = "Security group for RDS SQL Server"
  vpc_id      = var.vpc_id

  ingress {
    from_port   = 1433
    to_port     = 1433
    protocol    = "tcp"
    cidr_blocks = var.allowed_cidr_blocks
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Environment = var.environment
  }
}

resource "aws_db_instance" "sqlserver" {
  identifier             = "${var.environment}-aegisflow-sqlserver"
  engine                 = "sqlserver-ex"
  engine_version         = "15.00"
  instance_class         = var.instance_class
  allocated_storage      = var.allocated_storage
  max_allocated_storage  = 100
  storage_type           = "gp2"
  db_name                = var.db_name
  username               = var.admin_username
  password               = var.admin_password
  db_subnet_group_name   = aws_db_subnet_group.rds.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot    = true
  publicly_accessible    = false

  tags = {
    Environment = var.environment
    Project     = "Aegisflow"
  }
}
