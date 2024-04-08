$ApiManagementPolicyPath='policy.xml'
[System.Xml.XmlDocument]$policy = Get-Content 'policy.xml'
$InBoundXml = '<base/><set-query-parameter name="code" exists-action="override"><value>' + $FUNCTION_KEY + '</value></set-query-parameter>'
$policy.policies.inbound.InnerXml = $InBoundXml
$policy.Save($ApiManagementPolicyPath)