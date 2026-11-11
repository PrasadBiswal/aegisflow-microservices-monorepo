variable "cluster_name" {
  type        = string
  description = "EKS cluster name."
}

variable "kubernetes_version" {
  type        = string
  default     = null
  description = "Kubernetes version. null = the current EKS default version."
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
