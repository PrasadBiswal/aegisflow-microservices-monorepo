# Sandbox runbook (KodeKloud AWS, 2-hour session)

Every session is a new, empty AWS account. Follow these steps in order. Run commands from the repo root in PowerShell.

**Prerequisites (laptop):** AWS CLI, Terraform >= 1.10, kubectl, Helm, Git, Docker Desktop.

## 0:00 — Connect (5 min)
```powershell
aws configure                         # sandbox access key + secret, region us-east-1
aws sts get-caller-identity           # note the Account number
$acct = (aws sts get-caller-identity --query Account --output text)
$bucket = "aegisflow-tfstate-$acct"
```

## 0:05 — Bootstrap + IAM (5 min)
```powershell
terraform -chdir=terraform/environments/dev/bootstrap init
terraform -chdir=terraform/environments/dev/bootstrap apply -auto-approve

terraform -chdir=terraform/environments/dev/global/iam init -backend-config="bucket=$bucket"
terraform -chdir=terraform/environments/dev/global/iam apply -auto-approve
terraform -chdir=terraform/environments/dev/global/iam output -raw github_actions_role_arn
```
GitHub → **Settings → Secrets and variables → Actions → Variables** → set `AWS_ROLE_ARN` to that value.

## 0:10 — Network + EKS + ECR (~20 min)
```powershell
terraform -chdir=terraform/environments/dev/us-east-1/networking init -backend-config="bucket=$bucket"
terraform -chdir=terraform/environments/dev/us-east-1/networking apply -auto-approve

terraform -chdir=terraform/environments/dev/us-east-1/applications init -backend-config="bucket=$bucket"
terraform -chdir=terraform/environments/dev/us-east-1/applications apply -auto-approve -var="state_bucket=$bucket"
```
While EKS is creating: GitHub → **Actions → Release all images → Run workflow** (branch `dev`). Pushes all 10 images to ECR.

## 0:35 — Connect kubectl + install ArgoCD (10 min)
```powershell
aws eks update-kubeconfig --name aegisflow-dev --region us-east-1
kubectl get nodes                     # 3 nodes Ready

kubectl create namespace argocd
kubectl apply -n argocd --server-side --force-conflicts -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
kubectl -n argocd rollout status deploy/argocd-server
```

## 0:45 — Secrets (never stored in Git)
```powershell
# Database password (pick your own; SQL Server needs 8+ chars with upper, lower, digit, symbol)
kubectl create namespace aegisflow
kubectl -n aegisflow create secret generic aegisflow-db-secret --from-literal=password='<StrongPassword1!>'

# Private repo only: let ArgoCD read it (GitHub fine-grained token with Contents: Read-only)
kubectl -n argocd create secret generic repo-aegisflow `
  --from-literal=type=git `
  --from-literal=url=https://github.com/PrasadBiswal/aegisflow-microservices-monorepo.git `
  --from-literal=username=PrasadBiswal `
  --from-literal=password='<github-token>'
kubectl -n argocd label secret repo-aegisflow argocd.argoproj.io/secret-type=repository
```

## 0:50 — Deploy with GitOps
```powershell
git fetch origin
$tag = (git rev-parse origin/dev)     # must be the commit "Release all images" ran on

kubectl apply -f gitops/argocd/platform-app.yaml          # SQL Server
(Get-Content gitops/argocd/services-appset.yaml) `
  -replace '__ACCOUNT_ID__', $acct -replace '__IMAGE_TAG__', $tag |
  kubectl apply -f -                                       # 10 services

kubectl -n aegisflow get pods -w      # migrate Jobs complete, then 10 APIs Running
```

ArgoCD UI (optional):
```powershell
kubectl -n argocd port-forward svc/argocd-server 8443:443   # https://localhost:8443  (user: admin)
$p = kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}"
[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($p))
```

## 1:00 — Test (Postman)
Forward the services to the same local ports, then run the Postman collection with the **local** environment:
```powershell
kubectl -n aegisflow port-forward svc/users-api 8081:80
# (one terminal per service you want to test: catalog-api 8082, inventory-api 8083, ...)
```

## 1:30 — Tear down (15 min)
```powershell
kubectl delete -f gitops/argocd/platform-app.yaml
kubectl -n argocd delete applicationset aegisflow-services
terraform -chdir=terraform/environments/dev/us-east-1/applications destroy -auto-approve -var="state_bucket=$bucket"
terraform -chdir=terraform/environments/dev/us-east-1/networking destroy -auto-approve
```
GitHub → delete the `AWS_ROLE_ARN` variable (CI goes back to test/build only).

## If something fails
| Symptom | Check |
|---|---|
| Pods `ImagePullBackOff` | Did "Release all images" finish? Is `$tag` the same commit? |
| `*-migrate` Job failing | `kubectl -n aegisflow logs job/<service>-migrate` — usually SQL Server still starting (it retries) |
| Pods not Ready | `kubectl -n aegisflow describe pod <name>`; `/health/ready` fails when the DB is unreachable |
| ArgoCD "repository not found" | Private repo: create the `repo-aegisflow` secret (step 0:45) |
