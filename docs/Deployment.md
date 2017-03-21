# Deployment 
Deploying and configure the Partner Center Bot requires numerous configurations. This document will guide you through each of the
configurations. If you need help please
log an issue using the [issue tracker](https://github.com/Microsoft/Partner-Center-Bot/issues).

## Prerequisites 
The following are _optional_ prerequisites for this project 

| Prerequisite          | Purpose                                                                                                       |
|-----------------------|---------------------------------------------------------------------------------------------------------------|
|  Azure Subscription   | A subscription is only required if you want to host the sample in Azure or utilize the various Azure services.|

The following are _required_ prerequisites for this project

| Prerequisite                           | Purpose                                                                                      |
|----------------------------------------|----------------------------------------------------------------------------------------------|
|  Azure AD global admin privileges      | Required to create the required Azure AD application utilized to obtain access tokens.       |
|  Partner Center admin agent privileges | Required to perform various Partner Center operations through the Partner Center API.        |

## Azure Key Vault
Azure Key Vault is utilized by this project to protect application secrets and 
various connection strings. It is not required that this service by deployed, 
however, it is highly recommend that you utilize this service to protect this 
sensitive information.

If you would like to utilize Azure Key Vault then please follows the steps outlined
in the [Azure Key Vault](KeyVault.md) documentation.

## Partner Center Azure AD Application
App only authentication is utilized when performing various operations using the 
Partner Center API. In order to obtain the necessary access token to perform 
these operations an applicaiton needs to be registered in the Partner Center 
portal. Perform the following to create, if necessary, and register the required 
application

1. Login into the [Partner Center](https://partnercenter.microsoft.com) portal using credentials that have _AdminAgents_ and _Global Admin_ privileges
2. Click _Dashboard_ -> _Account Settings_ -> _App Management_ 
3. Click on _Register existing_ app if you want to use an existing Azure AD application, or click _Add new web app_ to create a new one

    ![Partner Center App](Images/appmgmt01.png)

4. Document the _App ID_ and _Account ID_ values. Also, if necessary create a key and document that value. 

    ![Partner Center App](Images/appmgmt02.png)

Now that the application has been registered in Partner Center you can update the 
the related configurations for the project. Perform the following to configure the
require settings 

1. Set the _PartnerCenterApplicationId_ setting in the _web.config_ file to the _App ID_ value documented in step 4 above. 
2. If you have deployed Azure Key Vault then add a new secret with name of _PartnerCenterApplicationSecret_. Configure the value to the key value that was obtained in step 4 above. If you have decided not to utilize Azure Key Vault then create a new application setting in the _web.config_ file with the name of _PartnerCenterApplicationSecret_ and set the value to key vaule obtained in step 4 above.
3. Set the _PartnerCenterApplicationTenantId_ setting in the _web.config_ file to the _Account ID_ value documented in step 4 above.

## Creating the Bot Azure AD Application
The bot requires an Azure AD application that grants privileges to Azure AD and the Microsoft Graph. Perform the following tasks to create and configure the application 

1. Login into the [Azure Management portal](https://portal.azure.com) using credentials that have _Global Admin_ privileges
2. Open the _Azure Active Directory_ user experince and then click _App registration_

	![Azure AD application creation](Images/aad01.png)

3. Click _+ Add_ to start the new application wizard
4. Specify an appropriate name for the bot, select _Web app / API_ for the application, an appropriate value for the sign-on URL, and then click _Create_
5. Click _Required permissions_ found on the settings blade for the the application and then click _+ Add_ 
6. Add the _Microsoft Graph_ API and grant it the _Read directory data_ application permission
7. Add the _Partner Center API_  and grant it the _Access Partner Center PPE_ delegated permission

	![Azure AD application permissions](Images/aad02.png)

8. Click _Grant Permissions_, found on the _Required Permissions_ blade, to consent to the application for the reseller tenant 

    ![Azure AD application consent](Images/aad03.png)

9. Enable pre-consent for this application by completing the steps outlined in the [Pre-consent](Preconsent.md) documentation.

## Register With the Bot Framework
Registering the bot with the framework is how the connector service knows how to interact with the bot's web service. Perform the 
following to register the bot and update the required configurations

1. Go to the Microsoft Bot Framework portal at https://dev.botframework.com and sign in.
2. Click the "Register a Bot" button and fill out the form. Be sure to document the application identifier and password that you generate as part of this registration. 
3. Set the _BotId_ and _MicrosoftAppId_ settings, found in the _web.config_ accordingly

    ```xml
    <!-- Specify the identifer for the bot here -->
    <add key="BotId" value="" />
    <!-- Specify the Microsoft application identifier for the bot here -->
    <add key="MicrosoftAppId" value="" />
    ```

4. If you have elected to utilize _Azure Key Vault_ then create a new secret with the name _MicrosoftAppPassword_ and set the value to the password generate in step 2. Otherwise, you will need to create a new setting in the _web.config_ that is similar to the following 

    ```xml
    <!-- Specify the Microsoft application password for the bot here -->
    <add key="MicrosoftAppPasssword" value="" />
    ```
    ```