param([string]$OutputDirectory = (Join-Path $PSScriptRoot '.local'))
$ErrorActionPreference = 'Stop'
$inventory = Get-Content (Join-Path $PSScriptRoot '.local/qa-inventory.json') -Raw | ConvertFrom-Json
if ($inventory.domain -ne 'mikompri010qa20261030.onmicrosoft.com') { throw 'Only the isolated 010 QA tenant is permitted.' }
$token = az account get-access-token --tenant $inventory.tenantId --resource https://graph.microsoft.com/ --query accessToken -o tsv --only-show-errors 2>$null
if ($LASTEXITCODE -ne 0) { throw 'QA Graph token unavailable; no fixtures created.' }
$headers = @{ Authorization = "Bearer $token" }
$fixtureIds = [Collections.Generic.List[string]]::new()
$observations = [Collections.Generic.List[object]]::new()
function Graph([string]$Method, [string]$Path, $Body = $null) {
    $parameters = @{ Method=$Method; Uri="https://graph.microsoft.com/v1.0/$Path"; Headers=$headers }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json'
        $parameters.Body = $Body | ConvertTo-Json -Depth 12 -Compress
    }
    Invoke-RestMethod @parameters
}
function Observe([string]$Name, [scriptblock]$Action) {
    try { & $Action; $observations.Add(@{ Name=$Name; Result='ACCEPTED' }) }
    catch {
        $code = 'unparsed'; $message = ''
        try { $detail=$_.ErrorDetails.Message|ConvertFrom-Json; $code=$detail.error.code; $message=$detail.error.message } catch { }
        $message = $message -replace '[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}','[id-redacted]' `
            -replace '[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}','[email-redacted]'
        $observations.Add(@{ Name=$Name; Result='REJECTED'; Status=[int]$_.Exception.Response.StatusCode; Code=$code; Message=$message })
    }
}
function Read-Identities([string]$Id) { (Graph GET "users/$($Id)?`$select=id,identities").identities }
function New-FederatedIdentity { @{ signInType='federated';issuer='google.com';issuerAssignedId=('qa010-'+[guid]::NewGuid().ToString('N')) } }
try {
    $organization = Graph GET 'organization?$select=tenantType'
    if (@($organization.value).Count -ne 1 -or $organization.value[0].tenantType -ne 'CIAM') { throw 'Not an isolated CIAM organization.' }
    $federated = New-FederatedIdentity
    $social = Graph POST users @{ displayName='QA010 synthetic federated probe'; accountEnabled=$true; identities=@($federated) }
    $fixtureIds.Add($social.id)
    $socialIdentities = @(Read-Identities $social.id)
    $observations.Add(@{ Name='create_and_read_federated_fixture'; Result='ACCEPTED'; Types=@($socialIdentities|ForEach-Object {$_.signInType}) })
    $localIdentity = @{ signInType='emailAddress'; issuer=$inventory.domain; issuerAssignedId=('qa010-'+[guid]::NewGuid().ToString('N')+'@example.invalid') }
    Observe 'federated_only_add_email_identity' { Graph PATCH "users/$($social.id)" @{identities=($socialIdentities+@($localIdentity))} | Out-Null }
    $password = 'Qa010!'+[Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
    $local = Graph POST users @{ displayName='QA010 synthetic local probe'; accountEnabled=$true; identities=@($localIdentity); passwordProfile=@{password=$password;forceChangePasswordNextSignIn=$false};passwordPolicies='DisablePasswordExpiration' }
    $password = $null
    $fixtureIds.Add($local.id)
    $initialLocal = @(Read-Identities $local.id)
    $observations.Add(@{ Name='create_and_read_local_fixture'; Result='ACCEPTED'; Types=@($initialLocal|ForEach-Object {$_.signInType}); IsOtpSignInEvidence=$false })
    $newSocial = New-FederatedIdentity
    Observe 'local_add_unoccupied_federated_identity' { Graph PATCH "users/$($local.id)" @{identities=($initialLocal+@($newSocial))} | Out-Null }
    $afterAdd = @(Read-Identities $local.id)
    $observations.Add(@{ Name='read_after_add'; Result='OBSERVED'; LocalPresent=(@($afterAdd|Where-Object {$_.signInType -eq 'emailAddress'}).Count -eq 1); FederatedPresent=(@($afterAdd|Where-Object {$_.signInType -eq 'federated'}).Count -eq 1); SameObjectId=$true })
    Observe 'unlink_federated_keep_local' { Graph PATCH "users/$($local.id)" @{identities=$initialLocal} | Out-Null }
    Observe 'add_federated_identity_already_owned_by_other_object' { Graph PATCH "users/$($local.id)" @{identities=($initialLocal+@($federated))} | Out-Null }
    $upnOnly = @($initialLocal|Where-Object {$_.signInType -eq 'userPrincipalName'})
    Observe 'remove_last_explicit_method_leave_upn' { Graph PATCH "users/$($local.id)" @{identities=$upnOnly} | Out-Null }
    $afterLast = @(Read-Identities $local.id)
    $observations.Add(@{ Name='read_after_last_method_attempt'; Result='OBSERVED'; Types=@($afterLast|ForEach-Object {$_.signInType}); IsLoginRecoveryEvidence=$false })
    Graph PATCH "users/$($local.id)" @{identities=$initialLocal} | Out-Null

    # Actual OTP user observed in QA is federated/issuer=mail, NOT emailAddress/password.
    $otpIdentity = @{ signInType='federated';issuer='mail';issuerAssignedId=('qa010-'+[guid]::NewGuid().ToString('N')+'@example.invalid') }
    Observe 'google_shaped_federated_add_mail_otp_shaped_identity' { Graph PATCH "users/$($social.id)" @{identities=($socialIdentities+@($otpIdentity))} | Out-Null }
    $googleMailRead=@(Read-Identities $social.id)
    $observations.Add(@{Name='read_google_plus_mail';Result='OBSERVED';GooglePresent=@($googleMailRead|Where-Object {$_.issuer -eq 'google.com'}).Count -eq 1;MailPresent=@($googleMailRead|Where-Object {$_.issuer -eq 'mail'}).Count -eq 1;SameObjectId=$true;ProvesProviderLogin=$false})
    Graph PATCH "users/$($social.id)" @{identities=$socialIdentities} | Out-Null
    $mailOnly = @{ signInType='federated';issuer='mail';issuerAssignedId=('qa010-'+[guid]::NewGuid().ToString('N')+'@example.invalid') }
    $otp = Graph POST users @{ displayName='QA010 synthetic mail OTP shape probe';accountEnabled=$true;identities=@($mailOnly) }
    $fixtureIds.Add($otp.id)
    $otpInitial=@(Read-Identities $otp.id)
    $otpGoogle=New-FederatedIdentity
    Observe 'mail_otp_shaped_add_unoccupied_google_shaped_identity' { Graph PATCH "users/$($otp.id)" @{identities=($otpInitial+@($otpGoogle))} | Out-Null }
    $otpRead=@(Read-Identities $otp.id)
    $observations.Add(@{Name='read_mail_plus_google';Result='OBSERVED';MailPresent=@($otpRead|Where-Object {$_.issuer -eq 'mail'}).Count -eq 1;GooglePresent=@($otpRead|Where-Object {$_.issuer -eq 'google.com'}).Count -eq 1;SameObjectId=$true;ProvesProviderLogin=$false})
    Observe 'unlink_google_keep_mail_otp_shaped' { Graph PATCH "users/$($otp.id)" @{identities=$otpInitial} | Out-Null }
    Observe 'remove_last_mail_method_leave_upn' { Graph PATCH "users/$($otp.id)" @{identities=@($otpInitial|Where-Object {$_.signInType -eq 'userPrincipalName'})} | Out-Null }
    $otpAfterLast=@(Read-Identities $otp.id)
    $observations.Add(@{Name='read_after_last_mail_method';Result='OBSERVED';Types=@($otpAfterLast|ForEach-Object {$_.signInType});ProvesUsableSurvivingAccess=$false})

    # Both requests deliberately replace the same collection from the same snapshot.
    $left = New-FederatedIdentity; $right = New-FederatedIdentity
    $http = [Net.Http.HttpClient]::new()
    $http.DefaultRequestHeaders.Authorization = [Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer',$token)
    $leftContent = [Net.Http.StringContent]::new((@{identities=($initialLocal+@($left))}|ConvertTo-Json -Depth 10 -Compress),[Text.Encoding]::UTF8,'application/json')
    $rightContent = [Net.Http.StringContent]::new((@{identities=($initialLocal+@($right))}|ConvertTo-Json -Depth 10 -Compress),[Text.Encoding]::UTF8,'application/json')
    try {
        $leftTask = $http.PatchAsync("https://graph.microsoft.com/v1.0/users/$($local.id)",$leftContent)
        $rightTask = $http.PatchAsync("https://graph.microsoft.com/v1.0/users/$($local.id)",$rightContent)
        $leftResponse = $leftTask.GetAwaiter().GetResult(); $rightResponse = $rightTask.GetAwaiter().GetResult()
        $concurrentRead = @(Read-Identities $local.id)
        $observations.Add(@{Name='concurrent_collection_replacements';Result='OBSERVED';LeftStatus=[int]$leftResponse.StatusCode;RightStatus=[int]$rightResponse.StatusCode;FederatedIdentitiesRemaining=@($concurrentRead|Where-Object {$_.signInType -eq 'federated'}).Count;IsAtomicLinkingEvidence=$false})
        $leftResponse.Dispose();$rightResponse.Dispose()
        Graph PATCH "users/$($local.id)" @{identities=$initialLocal} | Out-Null
        $timeoutIdentity = New-FederatedIdentity
        $timeoutContent = [Net.Http.StringContent]::new((@{identities=($initialLocal+@($timeoutIdentity))}|ConvertTo-Json -Depth 10 -Compress),[Text.Encoding]::UTF8,'application/json')
        $cancellation = [Threading.CancellationTokenSource]::new([TimeSpan]::FromMilliseconds(1))
        try {
            $response=$http.PatchAsync("https://graph.microsoft.com/v1.0/users/$($local.id)",$timeoutContent,$cancellation.Token).GetAwaiter().GetResult()
            $timeoutResult='RESPONSE_OBSERVED';$response.Dispose()
        } catch { $timeoutResult='CLIENT_CANCELLED_OUTCOME_UNKNOWN' }
        finally { $timeoutContent.Dispose();$cancellation.Dispose() }
        # A cancellation doesn't prove Graph rolled back. Re-read only, no blind retry.
        $timeoutRead=@(Read-Identities $local.id)
        $observations.Add(@{Name='client_timeout_then_reread';Result=$timeoutResult;CandidatePresent=@($timeoutRead|Where-Object {$_.issuerAssignedId -eq $timeoutIdentity.issuerAssignedId}).Count -eq 1;ReadSucceeded=$true;ProvesExactlyOnce=$false})
    }
    finally {$leftContent.Dispose();$rightContent.Dispose();$http.Dispose()}
}
finally {
    $cleanup=@()
    foreach ($id in $fixtureIds) {
        Graph DELETE "users/$id" | Out-Null
        # Hard-delete only our newly created fixture from deletedItems, not any other object.
        Graph DELETE "directory/deletedItems/$id" | Out-Null
        $cleanup+=@{SoftDeleted=$true;PermanentlyDeleted=$true}
    }
    $token=$null;$headers=$null;$password=$null
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    @{Kind='REAL_CIAM_GRAPH_SYNTHETIC_FIXTURES_NOT_PROVIDER_SIGN_IN';Observations=@($observations.ToArray());FixtureCleanup=$cleanup}|ConvertTo-Json -Depth 12|Set-Content -LiteralPath (Join-Path $OutputDirectory 'graph-aggregation-controls.json') -Encoding utf8NoBOM
}
Write-Output 'Graph probes complete; sanitized observations saved and owned fixtures removed.'
