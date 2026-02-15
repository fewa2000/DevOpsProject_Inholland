$prefix              = 'INHOLLAND-'+ (Get-Random -Minimum 1000 -Maximum 9999)
$resource_group_name = "$prefix-Assignment-D-AZWE-RG-1"
$template            = "./azurefunctions.bicep"

$parameters = @{
    prefix      = $prefix
    serviceTag  = "Assignment"
    environment = "D"
    regionTag   = "AZWE"
}

$parameters = $parameters.Keys.ForEach({"$_=$($parameters[$_])"}) -join ' '

Write-Host "Deploying resources in $resource_group_name"

# Create a new resource-group
az group create -l westeurope -n $resource_group_name

# Deploy resources inside resource-group
$cmd = "az deployment group create --mode Incremental --resource-group $resource_group_name --template-file $template --parameters $parameters"
Write-Host $cmd
Invoke-Expression  $cmd