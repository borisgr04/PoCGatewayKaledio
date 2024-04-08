.\az_active_subscription_test.ps1

.\createFunction.ps1 -ResourceGroupName 'rg-function-2024' -StorageAccountName 'sa-function-2024' -FunctionAppName 'FnWebHook-2024' -Location 'westus'

##func azure functionapp publish FnWebHook-2024
.\PublishFnWebHook.ps1

.\createAPIM.ps1 -ResourceGroupName 'rg-function-2024' -ApiManagementName 'apim-function-2024' -PublisherEmail 'boris.gonzalez@softwareone.com' -Location 'westus'

#.\connectApimToFunction.ps1 -ResourceGroupName 'rg-function-2024' -ApiManagementName 'apim-function-2024' -FunctionAppName 'FnWebHook-2024' -ApiManagementPolicyPath 'C:\SCEcpTrue\PocAzFunctionApiM\MyFunctionApp\policy.xml'

.\importFunctionToApim.ps1 -ResourceGroupName 'rg-function-2024' -ApiManagementName 'apim-function-2024' -FunctionAppName 'FnWebHook-2024' -ApiManagementPolicyPath 'C:\SCEcpTrue\PocAzFunctionApiM\MyFunctionApp\policy.xml' -ApimApiPath 'WebHook' -ApimApiDisplayName 'Kaleido-WebHook'  



