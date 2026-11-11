# 1. Create the GitHub OIDC Identity Provider in AWS IAM (if not already globally registered)
resource "aws_iam_openid_connect_provider" "github" {
  url             = "https://token.actions.githubusercontent.com"
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = ["6938fd4d98bab03faadb97b34396831e3780aea1"] # Standard GitHub Actions OIDC thumbprint
}

# 2. Define the IAM Assume Role Trust Policy restricted to your specific repository
data "aws_iam_policy_document" "github_oidc_trust" {
  statement {
    actions = ["sts:AssumeRoleWithWebIdentity"]
    effect  = "Allow"

    principals {
      type        = "Federated"
      identifiers = [aws_iam_openid_connect_provider.github.arn]
    }

    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:aud"
      values   = ["sts.amazonaws.com"]
    }

    condition {
      test     = "StringLike"
      variable = "token.actions.githubusercontent.com:sub"
      values   = ["repo:/:*"]
    }
  }
}

# 3. Create the IAM Role that GitHub Actions will assume
resource "aws_iam_role" "github_actions_role" {
  name               = "aegisflow-github-actions-role-"
  assume_role_policy = data.aws_iam_policy_document.github_oidc_trust.json

  tags = {
    Environment = var.environment
    ManagedBy   = "Terraform"
    Project     = "Aegisflow"
  }
}

# 4. Attach AdministratorAccess (or a scoped enterprise policy) to the role for infrastructure provisioning
resource "aws_iam_role_policy_attachment" "admin_attachment" {
  role       = aws_iam_role.github_actions_role.name
  policy_arn = "arn:aws:iam::aws:policy/AdministratorAccess"
}
