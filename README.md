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
az group create --name shortix-urlshortener-dev-rg --location eastus2
```

## Deploy a template

```bash
az deployment group what-if --resource-group shortix-urlshortener-dev-rg --template-file infra/main.bicep

# Is everything ok?

az deployment group create --resource-group shortix-urlshortener-dev-rg --template-file infra/main.bicep
```