# Azure Key Vault
Azure Key Vault provides a way to encrypt keys and small secrets like passwords using 
keys stored in hardware security modules (HSMs). Partner Center Bot utilizes this service
in order to protect application secrets and connection strings. Perorm the following in order
to create and configure the required resources.

## Create an instance of Azure Key Vault
Perform the following to create new instance of Azure Key Vault

1. Open PowerShell and install the [Azure PowerShell cmdlets](https://docs.microsoft.com/en-us/powershell/azureps-cmdlets-docs/)
if you necessary
2. Modify the following PowerShell cmdlets accordingly and then invoke them

    ```powershell
    Login-AzureRmAccount

    $resourceGroupName = "ResourceGroup"
    $vaultName = "VaultName"

    New-AzureRmKeyVault -VaultName $vaultName -ResourceGroupName $resourceGroupName -Location 'South Central US'
    ```
If you need additional information please check out [New-AzureRmKeyVault](https://docs.microsoft.com/en-us/powershell/resourcemanager/azurerm.keyvault/v2.2.0/new-azurermkeyvault).
Set the _VaultBaseAddress_ setting, found in the _web.config_, to a string similar to https://VAULT-NAME-HERE.vault.azure.net

```xml 
<!-- Specify the Azure Key Vault base address here -->
<add key="VaultBaseAddress" value="" />
```

## Create Certificate
A certificate is utilized to obtain the required access token in order to interact with the vault. 
Perform the following to create the certificate

1. Modify the makecert command accordingly and then invoke it 

    ```
    makecert -sv mykey.pvk -n "cn=PartnerCenterBot" bot.cer -b 03/01/2017 -e 03/01/2020 -r
    ```

2. Modify the pvk2pfx command accordingly and then invoke it 

    ```
    pvk2pfx -pvk mykey.pvk -spc bot.cer -pfx bot.pfx -po test123
    ```

## Create the Azure Active Directory Applcation
An Azure Active Directory (AAD) application is utilized to obtain the token used to interact with 
the vault. Perform the following to create and configure the AAD application

1. Open PowerShell and install the [Azure PowerShell cmdlets](https://docs.microsoft.com/en-us/powershell/azureps-cmdlets-docs/)
if you necessary
2. Update the following cmdlets and then invoke them

    ```powershell
    Login-AzureRmAccount

    ## Update these variable before invoking the rest of the cmdlets
    $certFile = "C:\cert\bot.cer"
    $identifierUri = "https://{0}/{1}" -f "tenant.onmicrosoft.com", [System.Guid]::NewGuid()
    $resourceGroupName = "ResourceGroupName"
    $vaultName = "VaultName"

    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cert.Import($certFile)
    $value = [System.Convert]::ToBase64String($cert.GetRawCertData())

    $app = New-AzureRmADApplication -DisplayName "Bot Vault App" -HomePage "https://localhost" -IdentifierUris "https://botapp" -CertValue $value -EndDate $cert.NotAfter -StartDate $cert.NotBefore
    $spn = New-AzureRmADServicePrincipal -ApplicationId $app.ApplicationId

    Set-AzureRmKeyVaultAccessPolicy -VaultName $vaultName -ObjectId $spn.Id -PermissionsToSecrets get -ResourceGroupName $resourceGroupName

    # Get the certificate thumbprint value for the VaultApplicationCertThumbprint setting
    $cert.Thumbprint
    ```

Set the _VaultApplicationCertThumbprint_ setting, found in the _web.config_, to the value return for the _$cert.Thumbprint_ variable 

```xml
<!-- Specify the thumbprint of the certificate used access Azure Key Vault here -->
<add key="VaultApplicationCertThumbprint" value="" />
```

If you need additional information please check out [Use Azure Key Vault from a Web Application](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-use-from-web-application).

