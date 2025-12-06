# Shortix

Highly scalable URL shortener

## IaC

How to provision all the infrastructure

## Log in into Azure

```bash
az login
```

## Create RG

```bash
az group create --name shortix-urlshortener-dev-rg --location eastus
```

## Deploy a template

```bash
az deployment group what-if --resource-group shortix-urlshortener-dev-rg --template-file infra/main.bicep

# Is everything ok?

az deployment group create --resource-group shortix-urlshortener-dev-rg --template-file infra/main.bicep
```

## Create User for GH Actions

```bash
az ad sp create-for-rbac --name "Github-Actions-SP" --role contributor --scopes /subscriptions/{subscriptionId} --sdk-auth
```

## Configure a federated identity credential on an app

https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp#configure-a-federated-identity-credential-on-an-app

## Get Azure Credentials

```bash
az ad sp credential reset --id {app-id} --sdk-auth
```

```json
{
  "clientId": "5e2ccd3f-fcd3-48ce-ac08-ee0dc97c157e",
  "clientSecret": "PASSWORD-HERE",
  "subscriptionId": "5e2ccd3f-fcd3-48ce-ac08-ee0dc97c157e",
  "tenantId": "5e2ccd3f-fcd3-48ce-ac08-ee0dc97c157e"
}
```