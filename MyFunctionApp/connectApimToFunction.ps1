Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $ApiManagementName,
    [Parameter(Mandatory = $true)]
    [String]
    $FunctionAppName,
    [Parameter(Mandatory = $true)]
    [String]
    $ApiManagementPolicyPath
)

# ------------------------------------------------------
# Variables
# ------------------------------------------------------

$RESOURCE_GROUP_NAME = $ResourceGroupName
$APIM_NAME = $ApiManagementName
$FUNCTION_APP_NAME = $FunctionAppName
$API_NAME = "helloworld-api"

# ------------------------------------------------------
# Get Function App Base URI
# ------------------------------------------------------

$DEFAULT_HOST_NAME = az functionapp show `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $FUNCTION_APP_NAME `
    --query "defaultHostName" `
    --output tsv

$FUNCTION_APP_BASE_URI = "https://${DEFAULT_HOST_NAME}/api"

# ------------------------------------------------------
# Add Hello World API to API Management Instance
# ------------------------------------------------------

az apim api create `
    --resource-group $RESOURCE_GROUP_NAME `
    --service-name $APIM_NAME `
    --api-id $API_NAME `
    --display-name "Hello World API" `
    --path "helloworld" `
    --service-url $FUNCTION_APP_BASE_URI `
    --protocols "https" `
    --description "This is a simple Hello World API" `

# ------------------------------------------------------
# Add /Ping Operation to Hello World API
# ------------------------------------------------------

$APIM_OPERATION_ID = az apim api operation create `
    --display-name "Get Hello World" `
    --method "GET" `
    --resource-group $RESOURCE_GROUP_NAME `
    --service-name $APIM_NAME `
    --api-id $API_NAME `
    --url-template "/helloworld" `
    --query "name" `
    --output tsv `

# ------------------------------------------------------
# Get Function key for use in API Management Policy
# ------------------------------------------------------

$FUNCTION_KEY = az functionapp keys list `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $FUNCTION_APP_NAME `
    --query "functionKeys.default" `
    --output tsv

# ------------------------------------------------------
# Build and Deploy Query Param Policy for Function Key
# ------------------------------------------------------

[System.Xml.XmlDocument]$policy = Get-Content $ApiManagementPolicyPath
$InBoundXml = '<base/><set-query-parameter name="code" exists-action="override"><value>' + $FUNCTION_KEY + '</value></set-query-parameter>'
$policy.policies.inbound.InnerXml = $InBoundXml
$policy.Save($ApiManagementPolicyPath)

Write-Host '-------------InBoundXml'
$InBoundXml
Write-Host '-------------InBoundXml'

[string]$PolicyAsString = Get-Content $ApiManagementPolicyPath

Write-Host '-------------PolicyAsString'
[string]$PolicyAsString
Write-Host '-------------PolicyAsString'

[PSCustomObject]$POLICY_RESOURCE = @{
    type = "Microsoft.ApiManagement/service/apis/operations/policies"
    apiVersion = "2022-08-01"
    name = "${APIM_NAME}/${API_NAME}/${APIM_OPERATION_ID}/policy"
    properties = @{
        format = "xml"
        value = $PolicyAsString
    }
}

$TEMPLATE = @{
   '$schema' = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#"
    contentVersion = "1.0"
    resources = @($POLICY_RESOURCE)
}

#touch ./azure/apimPolicyDeploy.json
New-Item -Path 'apimPolicyDeploy.json' -ItemType File
$TEMPLATE | ConvertTo-Json -Depth 10 | Out-File apimPolicyDeploy.json

az deployment group create `
    --name "apim-op-${APIM_OPERATION_ID}-policy-deploy" `
    --resource-group $RESOURCE_GROUP_NAME `
    --template-file apimPolicyDeploy.json

Remove-Item -Path 'apimPolicyDeploy.json' 