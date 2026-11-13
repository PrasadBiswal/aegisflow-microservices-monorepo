# Self-managed worker nodes: an EC2 Auto Scaling group running the EKS-optimized Amazon Linux 2023 AMI.
# Used when managed node groups are not allowed (e.g. the KodeKloud sandbox). Nodes are authorized
# through an EC2_LINUX access entry, so no aws-auth ConfigMap is needed.

locals {
  self_managed = var.node_mode == "self_managed"
}

# Latest EKS-optimized AL2023 AMI for the cluster's Kubernetes version.
data "aws_ssm_parameter" "node_ami" {
  count = local.self_managed ? 1 : 0
  name  = "/aws/service/eks/optimized-ami/${aws_eks_cluster.main.version}/amazon-linux-2023/x86_64/standard/recommended/image_id"
}

resource "aws_iam_instance_profile" "node" {
  count = local.self_managed ? 1 : 0
  name  = coalesce(var.node_instance_profile_name, "${var.cluster_name}-node-profile")
  role  = aws_iam_role.node.name
}

resource "aws_eks_access_entry" "node" {
  count         = local.self_managed ? 1 : 0
  cluster_name  = aws_eks_cluster.main.name
  principal_arn = aws_iam_role.node.arn
  type          = "EC2_LINUX"
}

resource "aws_launch_template" "node" {
  count         = local.self_managed ? 1 : 0
  name_prefix   = "${var.cluster_name}-node-"
  image_id      = data.aws_ssm_parameter.node_ami[0].value
  instance_type = var.node_instance_types[0]

  vpc_security_group_ids = [aws_eks_cluster.main.vpc_config[0].cluster_security_group_id]

  iam_instance_profile {
    arn = aws_iam_instance_profile.node[0].arn
  }

  # Standard CPU credits: T-series "unlimited" mode is not allowed in the sandbox (and costs extra).
  credit_specification {
    cpu_credits = "standard"
  }

  # IMDSv2 only; hop limit 2 so pods (VPC CNI) can reach instance metadata.
  metadata_options {
    http_endpoint               = "enabled"
    http_tokens                 = "required"
    http_put_response_hop_limit = 2
  }

  block_device_mappings {
    device_name = "/dev/xvda"

    ebs {
      volume_size           = 20
      volume_type           = "gp3"
      encrypted             = true
      delete_on_termination = true
    }
  }

  # AL2023 nodes join the cluster with nodeadm, configured by this NodeConfig.
  user_data = base64encode(<<-EOT
    MIME-Version: 1.0
    Content-Type: multipart/mixed; boundary="BOUNDARY"

    --BOUNDARY
    Content-Type: application/node.eks.aws

    ---
    apiVersion: node.eks.aws/v1alpha1
    kind: NodeConfig
    spec:
      cluster:
        name: ${aws_eks_cluster.main.name}
        apiServerEndpoint: ${aws_eks_cluster.main.endpoint}
        certificateAuthority: ${aws_eks_cluster.main.certificate_authority[0].data}
        cidr: ${aws_eks_cluster.main.kubernetes_network_config[0].service_ipv4_cidr}

    --BOUNDARY--
  EOT
  )

  tag_specifications {
    resource_type = "instance"
    tags          = { Name = "${var.cluster_name}-node" }
  }
}

resource "aws_autoscaling_group" "node" {
  count               = local.self_managed ? 1 : 0
  name                = "${var.cluster_name}-nodes"
  vpc_zone_identifier = var.subnet_ids
  min_size            = var.node_min_size
  max_size            = var.node_max_size
  desired_capacity    = var.node_desired_size

  launch_template {
    id      = aws_launch_template.node[0].id
    version = aws_launch_template.node[0].latest_version
  }

  tag {
    key                 = "kubernetes.io/cluster/${var.cluster_name}"
    value               = "owned"
    propagate_at_launch = true
  }

  depends_on = [aws_eks_access_entry.node, aws_iam_role_policy_attachment.node]
}
