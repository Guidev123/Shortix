# Azure Project Bootstrap Guide

Step-by-step guide to provision infrastructure, configure GitHub Actions CI/CD and prepare the Azure environment. This document serves as a reusable bootstrap template for any new project.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Azure Login](#azure-login)
3. [Create Resource Group](#create-resource-group)
4. [Deploy Infrastructure (Bicep)](#deploy-infrastructure-bicep)
5. [Service Principal for GitHub Actions](#service-principal-for-github-actions)
6. [Federated Identity (OIDC) — Recommended](#federated-identity-oidc--recommended)
7. [GitHub Repository Secrets](#github-repository-secrets)
8. [Validating the Setup](#validating-the-setup)
9. [Reuse Checklist for New Projects](#reuse-checklist-for-new-projects)

---

## Prerequisites

Make sure the following tools are installed and up to date before starting:

| Tool | Purpose | Install |
|---|---|---|
| [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) | Provision and manage Azure resources | `winget install Microsoft.AzureCLI` |
| [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) | Compile and validate Bicep templates | `az bicep install` |
| [Git](https://git-scm.com/) | Source control | — |

Verify your setup:

```bash
az version
az bicep version
```

---

## Azure Login

```bash
az login
```

If you work with multiple tenants or subscriptions, explicitly set the target:

```bash
# List available subscriptions
az account list --output table

# Set the target subscription
az account set --subscription "<subscription-name-or-id>"

# Confirm active subscription
az account show --output table
```

---

## Create Resource Group

Replace `<project>` and `<env>` with your project name and environment (`dev`, `stg`, `prd`).

```bash
az group create \
  --name <project>-<env>-rg \
  --location brazilsouth
```

> **Convention:** `{project}-{env}-rg` — e.g. `shortix-dev-rg`, `shortix-prd-rg`

---

## Deploy Infrastructure (Bicep)

### 1. Validate the template (what-if)

Always run `what-if` before any deployment to preview changes without applying them:

```bash
az deployment group what-if \
  --resource-group <project>-<env>-rg \
  --template-file infra/main.bicep \
  --parameters env=<env> customDomainName=<your-domain>
```

Review the output carefully. Look for any unexpected deletions or modifications.

### 2. Deploy

```bash
az deployment group create \
  --resource-group <project>-<env>-rg \
  --template-file infra/main.bicep \
  --parameters env=<env> customDomainName=<your-domain>
```

> **Note:** The `pgSqlPassword` parameter is marked `@secure()` in the template. The CLI will prompt for it interactively if not passed via `--parameters`. To pass it explicitly:
>
> ```bash
> --parameters pgSqlPassword="<your-secure-password>"
> ```
>
> For production, prefer reading the value from a Key Vault or a `.bicepparam` file with `getSecret()`.

### 3. Verify the deployment

```bash
az deployment group show \
  --resource-group <project>-<env>-rg \
  --name main \
  --query "properties.provisioningState"
```

---

## Service Principal for GitHub Actions

GitHub Actions needs an identity with permission to deploy to Azure. There are two approaches — OIDC (federated) is strongly preferred for production.

### Option A — Federated Identity / OIDC (Recommended)

No long-lived secrets. GitHub Actions authenticates using a short-lived token issued by GitHub's OIDC provider.

#### 1. Create the App Registration

```bash
az ad app create --display-name "sp-<project>-github-actions"
```

Save the `appId` from the output.

#### 2. Create the Service Principal

```bash
az ad sp create --id <appId>
```

#### 3. Assign the role

Use the built-in `Contributor` role for simplicity, or a custom role scoped down to only what CI/CD needs (e.g. `infra_deploy`):

```bash
# Using built-in Contributor
az role assignment create \
  --assignee <appId> \
  --role Contributor \
  --scope /subscriptions/<subscriptionId>/resourceGroups/<project>-<env>-rg

# Confirm assignment
az role assignment list --assignee <appId> --output table
```

#### 4. Configure Federated Identity Credential

Add one credential per GitHub environment (or branch) that needs to deploy.

```bash
az ad app federated-credential create \
  --id <appId> \
  --parameters '{
    "name": "github-actions-<env>",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<org>/<repo>:environment:<env>",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

Common `subject` patterns:

| Trigger | Subject |
|---|---|
| GitHub Environment | `repo:<org>/<repo>:environment:<env>` |
| Branch | `repo:<org>/<repo>:ref:refs/heads/main` |
| Pull Request | `repo:<org>/<repo>:pull_request` |

> Full reference: [Microsoft Docs — Configure a federated identity credential](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp#configure-a-federated-identity-credential-on-an-app)

---

### Option B — Client Secret (Not Recommended for Production)

Use only for local testing or environments where OIDC is not supported.

```bash
az ad sp create-for-rbac \
  --name "sp-<project>-github-actions" \
  --role Contributor \
  --scopes /subscriptions/<subscriptionId>/resourceGroups/<project>-<env>-rg \
  --sdk-auth
```

To rotate or retrieve credentials:

```bash
az ad sp credential reset --id <appId> --sdk-auth
```

Output (store all values as GitHub secrets — never commit to source control):

```json
{
  "clientId": "<appId>",
  "clientSecret": "<secret>",
  "subscriptionId": "<subscriptionId>",
  "tenantId": "<tenantId>"
}
```

---

## GitHub Repository Secrets

Configure the following secrets in your GitHub repository under **Settings → Secrets and variables → Actions**:

### For OIDC (Option A)

| Secret | Value |
|---|---|
| `AZURE_CLIENT_ID` | App Registration `appId` |
| `AZURE_TENANT_ID` | Your Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target subscription ID |

### For Client Secret (Option B)

| Secret | Value |
|---|---|
| `AZURE_CREDENTIALS` | Full JSON output from `az ad sp create-for-rbac --sdk-auth` |

### Project-specific secrets

These are specific to this project's Bicep parameters and runtime configuration:

| Secret | Value |
|---|---|
| `PGSQL_PASSWORD` | PostgreSQL administrator password (used as `pgSqlPassword` in Bicep) |
| `CUSTOM_DOMAIN_NAME` | Custom domain name (used as `customDomainName` in Bicep) |

---

## Validating the Setup

After deployment, confirm the key resources are healthy:

```bash
# List all resources in the resource group
az resource list --resource-group <project>-<env>-rg --output table

# Check App Service status
az webapp show \
  --resource-group <project>-<env>-rg \
  --name <appName> \
  --query "state"

# Check Front Door endpoint
az afd endpoint show \
  --resource-group <project>-<env>-rg \
  --profile-name <frontDoorProfileName> \
  --endpoint-name <endpointName>

# Check Key Vault network rules
az keyvault show \
  --resource-group <project>-<env>-rg \
  --name <keyVaultName> \
  --query "properties.networkAcls"
```

---

## Reuse Checklist for New Projects

Use this checklist whenever bootstrapping a new Azure project with the same patterns:

- [ ] Create a new Resource Group following the `{project}-{env}-rg` convention
- [ ] Copy and adapt `infra/main.bicep` and the module structure
- [ ] Update subnet address prefixes if VNets will be peered or share a hub
- [ ] Run `az deployment group what-if` before every first deploy in a new environment
- [ ] Create an App Registration per project (do not reuse across projects)
- [ ] Configure Federated Identity Credentials for each GitHub environment (`dev`, `prd`)
- [ ] Add `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` as GitHub secrets
- [ ] Add all `@secure()` Bicep parameters as GitHub secrets
- [ ] Confirm Key Vault RBAC role assignments are scoped correctly after first deploy
- [ ] Validate App Services are only reachable through Front Door (check IP restrictions)
- [ ] Confirm all private endpoints resolve correctly inside the VNet