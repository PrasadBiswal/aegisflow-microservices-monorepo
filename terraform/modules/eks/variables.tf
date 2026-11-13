variable "cluster_name" {
  type        = string
  description = "EKS cluster name."
}

variable "kubernetes_version" {
  type        = string
  default     = null
  description = "Kubernetes version. null = the current EKS default version."
}

variable "cluster_role_name" {
  type        = string
  default     = null
  description = "IAM role name for the control plane. null = <cluster_name>-cluster-role. Some sandboxes only allow PassRole for fixed names."
}

variable "node_role_name" {
  type        = string
  default     = null
  description = "IAM role name for the worker nodes. null = <cluster_name>-node-role."
}

variable "node_mode" {
  type        = string
  default     = "managed"
  description = "managed = EKS managed node group; self_managed = EC2 Auto Scaling group joined via an access entry."

  validation {
    condition     = contains(["managed", "self_managed"], var.node_mode)
    error_message = "node_mode must be \"managed\" or \"self_managed\"."
  }
}

variable "node_instance_profile_name" {
  type        = string
  default     = null
  description = "Instance profile name for self-managed nodes. null = <cluster_name>-node-profile."
}

variable "subnet_ids" {
  type        = list(string)
  description = "Private subnet IDs for the control-plane interfaces and worker nodes."
}

variable "public_access_cidrs" {
  type        = list(string)
  default     = ["0.0.0.0/0"]
  description = "Who can reach the public API endpoint. Restrict to your IP / office range in real accounts."
}

variable "cluster_log_types" {
  type        = list(string)
  default     = ["api", "audit", "authenticator"]
  description = "Control-plane logs sent to CloudWatch."
}

variable "node_instance_types" {
  type    = list(string)
  default = ["t3.medium"]
}

variable "node_desired_size" {
  type    = number
  default = 3
}

variable "node_min_size" {
  type    = number
  default = 2
}

variable "node_max_size" {
  type    = number
  default = 4
}
