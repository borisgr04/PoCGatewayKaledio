param(
[Parameter(Mandatory=$true)]
[String]$resourceGroup,
[Parameter(Mandatory=$true)]
[String]$swaggerUrl,
[Parameter(Mandatory=$true)]
[String]$apimName,
[Parameter(Mandatory=$true)]
[String]$apimApiDisplayName,
[Parameter(Mandatory=$true)]
[String]$apimApiPath,
[Parameter(Mandatory=$true)]
[String]$serviceUrl,
[Parameter(Mandatory=$true)]
[String]$policyFilePath
)

Write-Output "Getting context"
$azcontext = New-AzApiManagementContext -ResourceGroupName $resourceGroup -ServiceName $apimName

Write-Output "Getting API details"
$existing = Get-AzApiManagementApi -Context $azcontext -Name $apimApiDisplayName -ErrorAction SilentlyContinue

Write-Output "Show ApiId 01"
$existing.ApiId

Write-Output "Importing API"
$existing = Import-AzApiManagementApi -Context $azcontext -SpecificationFormat OpenApi -SpecificationUrl $swaggerUrl -ApiId $existing.ApiId -Path $apimApiPath 

Write-Output "Show ApiId 02"
$existing.ApiId

Write-Output "Setting API"
Set-AzApiManagementApi -Context $azcontext -ApiId $existing.ApiId -Protocols @("https") -Name $apimApiDisplayName -ServiceUrl $serviceUrl

Write-Output "Setting policy for API"
Set-AzApiManagementPolicy -Context $azcontext -PolicyFilePath $policyFilePath -ApiId $existing.ApiId