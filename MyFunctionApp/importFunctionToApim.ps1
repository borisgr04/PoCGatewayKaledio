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
    $ApiManagementPolicyPath,
    [Parameter(Mandatory = $true)]
    [String]
    $ApimApiDisplayName,
    [Parameter(Mandatory = $true)]
    [String]
    $ApimApiPath
)

# ------------------------------------------------------
# Variables
# ------------------------------------------------------

$RESOURCE_GROUP_NAME = $ResourceGroupName
$APIM_NAME = $ApiManagementName
$FUNCTION_APP_NAME = $FunctionAppName
$API_NAME = "helloworld-api-sw"

$DEFAULT_HOST_NAME = az functionapp show `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $FUNCTION_APP_NAME `
    --query "defaultHostName" `
    --output tsv

$FUNCTION_APP_SWAGGER_URI = "https://${DEFAULT_HOST_NAME}/api/swagger.json"

$FUNCTION_APP_BASE_URI = "https://${DEFAULT_HOST_NAME}/api"   

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

#Write-Host '-------------InBoundXml'
#$InBoundXml
#Write-Host '-------------InBoundXml'

#[string]$PolicyAsString = Get-Content $ApiManagementPolicyPath

#Write-Host '-------------PolicyAsString'
#[string]$PolicyAsString
#Write-Host '-------------PolicyAsString'


.\UpdatePolicyApim.ps1 -resourceGroup $RESOURCE_GROUP_NAME -swaggerUrl $FUNCTION_APP_SWAGGER_URI -apimName $ApiManagementName -apimApiDisplayName $ApimApiDisplayName -apimApiPath $ApimApiPath -serviceUrl $FUNCTION_APP_BASE_URI -policyFilePath $ApiManagementPolicyPath



