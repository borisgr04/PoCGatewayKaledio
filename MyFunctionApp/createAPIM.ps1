Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $ApiManagementName,
    [Parameter(Mandatory = $true)]
    [String]
    $Location,
    [Parameter(Mandatory = $true)]
    [String]
    $PublisherEmail,
    [Parameter(Mandatory = $true)]
    [String]
    $PublisherName
)

# ------------------------------------------------------
# Variables
# ------------------------------------------------------
$RESOURCE_GROUP_NAME = $ResourceGroupName
$APIM_NAME = $ApiManagementName
$APIM_SKU = "Consumption"
$APIM_LOCATION = $Location
$APIM_PUBLISHER_EMAIL = $PublisherEmail
$APIM_PUBLISHER_NAME = $PublisherName

# ------------------------------------------------------
# Provision Resource Group
# ------------------------------------------------------
az group create `
    --name $RESOURCE_GROUP_NAME `
    --location $LOCATION

# ------------------------------------------------------
# Provision API Management Instance
# ------------------------------------------------------
az apim create `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $APIM_NAME `
    --location $APIM_LOCATION `
    --sku-name $APIM_SKU `
    --enable-managed-identity true `
    --publisher-email $APIM_PUBLISHER_EMAIL `
    --publisher-name $APIM_PUBLISHER_NAME