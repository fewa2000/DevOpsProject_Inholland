# AssignmentDevOpsProject_fwald

An Azure Functions application that fetches real-time weather data from the Buienradar API, retrieves images from the Unsplash API, overlays weather information onto the images, and uploads the processed results to Azure Blob Storage.

## Architecture

```
┌─────────────────────┐     ┌──────────────────────┐     ┌─────────────────────────┐
│  HTTP Trigger /      │     │  Queue: start-job     │     │  Queue: image-processing │
│  ProcessAndUpload    │     │  QueueTriggerStarting │────▶│  QueueTriggerImage       │
│  ImageFunction       │     │  Job                  │     │  Processing              │
└────────┬────────────┘     └──────────┬───────────┘     └────────┬────────────────┘
         │                             │                          │
         ▼                             ▼                          ▼
   ┌───────────┐              ┌───────────────┐          ┌───────────────┐
   │ Buienradar│              │ Buienradar API│          │ Download Image│
   │ + Unsplash│              │ + Unsplash API│          │ + Overlay Text│
   │ + Process │              │ (fetch URLs)  │          │ + Upload Blob │
   │ + Upload  │              └───────────────┘          └───────────────┘
   └───────────┘
```

### Functions

| Function | Trigger | Description |
|----------|---------|-------------|
| `ProcessAndUploadImageFunction` | HTTP (GET/POST) | Fetches weather data and a random image, overlays the weather text, and uploads to Blob Storage. |
| `ProcessStartJobQueue` | Queue (`start-job-queue`) | Fetches weather data and multiple image URLs, then enqueues individual image processing jobs. |
| `ProcessImageQueue` | Queue (`image-processing-queue`) | Downloads an image, overlays weather text, and uploads the processed image to Blob Storage. |

### Services

| Service | Purpose |
|---------|---------|
| `BuienraderAPI` | Retrieves real-time weather station measurements from the Buienradar JSON feed. |
| `UnsplashAPI` | Fetches random images and image URLs from the Unsplash API. |
| `ImageHelper` | Overlays text onto images using SkiaSharp. |
| `BlobStorage` | Uploads processed images to Azure Blob Storage. |

## Tech Stack

- **.NET 6** / C#
- **Azure Functions v4** (In-Process hosting model)
- **Azure Blob Storage** for image storage
- **Azure Queue Storage** for message-based processing
- **SkiaSharp** for image manipulation
- **Bicep** for infrastructure as code

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- An [Unsplash Developer](https://unsplash.com/developers) API key
- An Azure subscription

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd DevsOpsProject_Inholland
```

### 2. Configure local settings

Create a `local.settings.json` file in the `AssignmentDevOpsProject_fwald/` directory:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "UnsplashApiKey": "<your-unsplash-api-key>",
    "BlobContainer": "processed-images"
  }
}
```

### 3. Run locally

```bash
cd AssignmentDevOpsProject_fwald
func start
```

The HTTP trigger will be available at `http://localhost:7172/api/ProcessAndUploadImageFunction`.

## Infrastructure Deployment

The `azurefunctions.bicep` template provisions all required Azure resources:

- Storage Account (Blob + Queue services)
- Application Insights
- App Service Plan (Consumption tier)
- Function App with System-Assigned Managed Identity

Deploy using the included PowerShell script:

```powershell
./deployment.ps1
```

Or deploy manually via Azure CLI:

```bash
az group create -l westeurope -n <resource-group-name>
az deployment group create \
  --resource-group <resource-group-name> \
  --template-file azurefunctions.bicep \
  --parameters prefix=<prefix> serviceTag=Assignment environment=D regionTag=AZWE
```

## CI/CD

### Azure DevOps

The `devopsproject.yml` pipeline triggers on pushes to `master` and runs a two-stage build and deploy process.

### GitHub Actions

The `.github/workflows/main_assignmentdevopsproject.yml` workflow triggers on pushes to `main` and deploys to the Azure Function App using a publish profile stored in GitHub Secrets.

## Project Structure

```
DevsOpsProject_Inholland/
├── .github/workflows/                  # GitHub Actions workflow
├── AssignmentDevOpsProject_fwald/
│   ├── Properties/                     # Launch settings
│   ├── Services/
│   │   ├── BlobStorage.cs              # Azure Blob Storage client
│   │   ├── BuienraderAPI.cs            # Buienradar weather data client
│   │   ├── ImageHelper.cs              # SkiaSharp image processing
│   │   └── UnsplashAPI.cs              # Unsplash image API client
│   ├── ProcessAndUploadImageFunction.cs
│   ├── QueueTriggerFunctionImageProcessing.cs
│   ├── QueueTriggerFunctionStartingJob.cs
│   ├── Startup.cs                      # DI configuration
│   ├── host.json                       # Azure Functions host config
│   └── .gitignore
├── azurefunctions.bicep                # Infrastructure as Code
├── deployment.ps1                      # Azure deployment script
├── devopsproject.yml                   # Azure DevOps pipeline
└── AssignmentDevOpsProject_fwald.sln
```

## Environment Variables

| Variable | Description |
|----------|-------------|
| `AzureWebJobsStorage` | Azure Storage connection string (used for blobs and queues) |
| `UnsplashApiKey` | Unsplash API client ID |
| `BlobContainer` | Name of the blob container for processed images (default: `processed-images`) |
| `FUNCTIONS_WORKER_RUNTIME` | Must be set to `dotnet` |
| `FUNCTIONS_EXTENSION_VERSION` | Must be set to `~4` |
