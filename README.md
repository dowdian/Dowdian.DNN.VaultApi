# DNN Azure Key Vault Web API
## Introduction {#introduction}
>__Please Note__: 
>This solution is broken for the moment because of a _lot_ of breaking fixes all at once. I'm working on fixing it. I'll remove this message when it's ready to go.
>
>These instructions are accurate as of May 30, 2024. If you're reading this in the future (which I presume you are), you may need to adjust the instructions to match the current state of the Azure portal. This is what made creating this project so damn hard. _All_ the instructions, tutorials and sample projects I could find were convoluted and/or out of date. This goal of this project is to create a simple project that people (i.e: me) can learn from.

This document is (or will be) boken up into six sections:
1. [Certificate Creation](#certificates)
    You'll need to this no matter how you want to use this solution.
1. [Azure Key Vault Setup](#azuresetup)
    Also required no matter which path you choose, this is for setting up the Azure Key Vault and the App Registration in Azure Entra (formerly Active Directory).
1. [Installing the API locally](#localinstall)
    This is for using the API in a development environment on your local machine or on a provisioned server.
1. [Installing the API in Azure](#azureinstall)
    This is for using the API in an Azure App Service instance.
1. [Setting up the development environment](#development)
    This is for stepping through or modifying the code, or (if you're feeling _very_ generous) contributing to the project. 🙏
1. [Using the API](#use)
    Once you have the API set up wherever it is, this is how you can use it in your own projects to manage secrets either locally in a web.config or in the Azure Key Vault.

This project is at the proof of concept stage. The end goal is to easily manage secrets stored in DNN in the most secure ways possible. There may well be some obvious gaps. Please point them out with kindness.

This solution:
 - Uses a self-signed 4096 bit certificate to authenticate to Azure Key Vault. 
 - Uses a seperate self-signed 4096 bit certificate to encrypt sections of the web.config files (two web.config files to be exact).
 - Includes basic CRUD methods for managing the Secrets implemented as Web API methods available to authenticated processes within a DNN instance. 
 - Does not modify the schema of the DNN database.
 - Can be used either in a development environemnt (localhost), in a self-hosted server situation, or in an Azure App Service instance. These instructions focus on a localhost setup (for now).

## Certificate Creation {#certificates}
You'll need to create two self-signed certificates. One to authenticate to the Key Vault and the second to encrypt and decrypt sections of the web.config. 
>In the context of certificates, there are generally two types of keys: signature keys and exchange keys.
> - Signature keys are used to create digital signatures for authenticity and integrity. They are used to verify that data hasn't been tampered with and that it originates from the specified source.
> - Exchange keys, on the other hand, are used for encrypting and decrypting the symmetric key in secure communications. In a typical secure communication, the symmetric key is used to encrypt the actual message data, and then the symmetric key itself is encrypted with the recipient's public exchange key. The recipient can then use their private exchange key to decrypt the symmetric key, and then use that to decrypt the actual message.

Let's start things off with the signature key certificate.
1. Create a new folder to store your certificates and their related files.
1. Open a PowerShell window as an administrator and navigate to your new folder. (Keep it open when you're done, you'll need it again later.)
1. Run the following command to create a self-signed signature key certificate in your current directory. This will cause Windows to ask you to enter a password three times across two dialogs. Leaving it blank is not an option:
    ```powershell
    makecert –r -n "cn=DnnVaultApiSignature" DnnVaultApiSignature.cer -sv DnnVaultApiSignature.pvk -b 05/30/2024 -e 06/30/2024 -len 4096
    ```
    > - `-r` creates a self-signed certificate.
    > - `-n` specifies the subject name of the certificate.
    > - `-b` specifies the start date of the certificate.
    > - `-e` specifies the end date of the certificate.
    > - `-len` specifies the length of the key.
    > - `DnnVaultApiSignature.cer -sv DnnVaultApiSignature.pvk` specifies the output file names for the certificate and the private key file.


    > __Note__: When I copied and pasted this command into the command prompt, it didn't work. I had to type it out manually. I'm not sure why. 🤷‍♂️
2. Run the following command to get the thumbprint of the certificate:
    ```powershell
    Get-PfxCertificate -FilePath .\DnnVaultApiSignature.cer | fl Thumbprint | Out-File -FilePath .\DnnVaultApiSignatureThumbprint.txt
    ```
1. Run the following command to extract the combined certificate and private key into a new Personal Information Exchange (PFX) file:
    ```powershell
    pvk2pfx -pvk DnnVaultApiSignature.pvk -spc DnnVaultApiSignature.cer -pfx DnnVaultApiSignature.pfx -pi <password>
    ```
    > Replace `<password>` with the password you chose when creating the certificate.

Now you should have four files in your new certificates folder. Next up, let's create the exchange key certificate. It's pretty much a line-for-line repeat of the signature key process with just a few small differences.
1. Run the following command to create a self-signed exchange key certificate in your current directory. I would use the same password as last time, unless you're feeling extra spicy:
    ```powershell
    makecert -r -n "cn=DnnVaultApiExchange" -sky exchange DnnVaultApiExchange.cer -sv DnnVaultApiExchange.pvk -b 05/18/2024 -e 06/18/2024 -len 4096
    ```
    > - `-sky exchange` specifies the subject's key type. Here, `exchange` means it's an exchange key.

2. Run the following command to get the thumbprint of the certificate:
    ```powershell
    Get-PfxCertificate -FilePath .\DnnVaultApiExchange.cer | fl Thumbprint | Out-File -FilePath .\DnnVaultApiExchangeThumbprint.txt
    ```
3. Run the following command to create the PFX file:
    ```powershell
    pvk2pfx -pvk DnnVaultApiExchange.pvk -spc DnnVaultApiExchange.cer -pfx DnnVaultApiExchange.pfx -pi <password>
    ```
    > Replace `<password>` with the password you chose when creating this certificate.

Now you should have _eight_ files in your certificates folder and this should be all you need to [install the API locally](#localinstall), [install in Azure](#azureinstall), or to [set up the development environment](#development). I bet you feel more secure already! 🎉

## Azure Key Vault Setup
1. If you don't already have an Azure account, you can create one for free [here](https://azure.microsoft.com/en-us/free/). 
1. You will also need to create an App Registration in Azure Active Directory to uniquely identify your DNN instance. This will be used to authenticate to the Key Vault. You can follow the instructions [here](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app).
    1. Be certain to add a redirect URI ([details](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app#add-a-redirect-uri)). 
    
        ![Redirect URI](./ReadMeImages/redirect-uri.png)
    2. Upload the public key of the **Signature** Certificate to the App registration in Azure Active Directory ([details](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app#add-a-certificate)).

        ![App Registration Certificate](./ReadMeImages/app-registration-certificate.png)
   > Make note of the App Registration's `Application (client) ID` and the `Directory (tenant) ID` as you will need these later.
2. If you don't already have a Azure Key Vault associated with you Azure account, you can create one by following the instructions [here](https://learn.microsoft.com/en-us/azure/key-vault/secrets/quick-create-cli#create-a-key-vault).
   > make note of the URI of the Key Vault as you will need this later.
3. Give the App registration the necessary permissions to access the Key Vault. You can follow the instructions [here](https://learn.microsoft.com/en-us/azure/key-vault/secrets/quick-create-cli#assign-a-role-to-the-app-registration).


## Installing the API locally {#localinstall}
I'll write this section up after I add the automatic build and install scripts. For now, you can follow the instructions in the [Development Environment Setup](#development) section to get the API up and running on your local machine.

## Installing the API in Azure {#azureinstall}
Here again, I'll write this section up after I add the automatic build and install scripts. For now, you can follow the instructions in the [Development Environment Setup](#development) section to get the API up and running on your local machine.

## Development Environment Setup {#development}
This solution was built using the [Upendo DNN Generator](https://github.com/UpendoVentures/generator-upendodnn#readme). If you're already familiar you'll have a bit of a head start, but if you're not don't worry. We'll walk through the set up from the beginning.

One more note before we get started, In order for the IIS process (or an Azure App Service for that matter) to have access to the certificates, the certificates _have_ to be associated with a user. You can create your own local user if you like with just exactly the right premissions for your solution, _or_ just use `NETWORK SERVICE`, which has a very nice mix of **enough permissions to do what we need to do** and **easy-peasy**. In this I choose the easy way.

### File system Setup
To begin, let's get DNN and the project directory structure set up.
1. Clone this repository to a directory close to the root of your local machine (to avoid any potential issues with long paths).
1. Create a new folder in the root of the solution called `Website`.
1. Give the `NETWORK SERVICE` account full permissions on the `Website` directory to allow IIS to read and write to the directory.

    ![Folder permissions](./ReadMeImages/dnn-step-01.png)
1. Unzip a [fresh copy of DNN](https://github.com/dnnsoftware/Dnn.Platform/releases) into the new `Website` directory.

### Database setup
1. Create (or overwrite!) a database in your local SQL Server instance and allow IIS to attach to it using the `NETWORK SERVICE` account using the following SQL script:
    ```sql
	USE [master]
	GO

	DECLARE @DatabaseName AS sysname = 'YourDatabaseName'
	DECLARE @PathToFiles AS sysname = 'C:\Path\For\Your\Database\Files'
	DECLARE @Sql as nvarchar(max)

	-- If I'm running this script and the database already exists, it's 
	-- because I screwed up my development environment and I need to start over.
	-- If I delete something I shouldn't have, well, that's on me.
	IF EXISTS (select * from sys.databases where name = @DatabaseName)
	BEGIN
		SET @Sql = 'ALTER DATABASE ' + @DatabaseName + ' SET  SINGLE_USER WITH ROLLBACK IMMEDIATE'
		EXEC (@Sql)

		EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = @DatabaseName

		SET @Sql = 'DROP DATABASE ' + @DatabaseName
		EXEC (@Sql)
	END

	-- Now, with a truely clean slate, create a new database.
	SET @Sql = 'CREATE DATABASE ' + @DatabaseName + ' CONTAINMENT = NONE 
		ON  PRIMARY ( NAME = N''' + @DatabaseName + ''', FILENAME = N''' + @PathToFiles + '\' + @DatabaseName + '.mdf'', SIZE = 8192KB , FILEGROWTH = 65536KB ) 
		LOG ON ( NAME = N''' + @DatabaseName + '_log'', FILENAME = N''' + @PathToFiles + '\' + @DatabaseName + '_log.ldf'', SIZE = 8192KB , FILEGROWTH = 65536KB ) 
		WITH LEDGER = OFF'
	EXEC (@Sql)

	-- Lastly, let's give full permissions to NETWORK SERVICE to access our new database.
	SET @Sql = 'USE ' + @DatabaseName + ' 
		CREATE USER [NT AUTHORITY\NETWORK SERVICE]
		ALTER AUTHORIZATION ON SCHEMA::[db_owner] TO [NT AUTHORITY\NETWORK SERVICE]
		ALTER ROLE [db_owner] ADD MEMBER [NT AUTHORITY\NETWORK SERVICE]'
	EXEC (@Sql)
	GO
    ```
	> Replace `YourDatabaseName` and `Path\For\Your\Database\Files` with appropriate values.

    This should result in a lovely new database, seen here in SSMS, all set up for IIS to use it, which is good, because IIS is next.

    ![Database permissions](./ReadMeImages/database-permissions.png)

### IIS Setup
3. Open IIS Manager and add an Application under the Default Web Site with the following settings:
	- Alias: `DnnVaultApi`
	- Physical Path: `C:\PathToYourProjectDirectory\Website`
        > Replace `PathToYourProjectDirectory` with the appropriate value.
	- Application Pool: `DefaultAppPool`
	    > Note: If you already have applications running under your DefaultAppPool, you may want to create a new AppPool for this application.
4. Modify the DefaultAppPool (or the AppPool of your choice) to use the `NETWORK SERVICE` account as its identity.

### Certificates Setup
1. Run the following commands in an Administrator PowerShell console to install the certificates on your local machine:
    ```powershell
    certutil -f -p <password> -importpfx .\DnnVaultApiExchange.pfx
    certutil -f -p <password> -importpfx .\DnnVaultApiSignature.pfx
    ```
    > Replace `<password>` with the password(s) you chose when creating the certificates.

1. Lastly, you'll need to give the `NETWORK SERVICE` account full permissions on the certificates so that our IIS process will have access to them.
1. Open the Certificates MMC snap-in by running `certlm.msc` in the Run dialog.
1. Navigate to `Personal > Certificates`.
1. Right-click on the `DnnVaultApiSignature` certificate and select `All Tasks > Manage Private Keys`.
1. Add the `NETWORK SERVICE` account with `Full Control` permissions.
1. Right-click on the `DnnVaultApiExchange` certificate and select `All Tasks > Manage Private Keys`.
1. Add the `NETWORK SERVICE` account with `Full Control` permissions.
   
    ![Exchange Key Certificate permissions](./ReadMeImages/exchange-cert-permissions.png) ![Signature Key Certificate permissions](./ReadMeImages/signature-cert-permissions.png)

Seriously, I can't believe you're still with me!!! Hang on! We're almost there.

### Running the new environment
1. Using your favorite browser, navigate to `http://localhost/DnnVaultApi` to complete the DNN installation process.
	- Give the host user a password.
    - Database Setup: `Custom`
    - Database Type: `SQL Server/SQL Server Express Database`
    - Server Name: `(local)`
	- Database Name: `YourDatabaseName`
	- Security: `Integrated`
    - All other values can be left as their default values.
    This should result in a perfectly adequate DNN instance.
   
    ![Perfectly Adequate DNN](./ReadMeImages/perfectly-adequate-dnn.png)

2. Now, with DNN installed, we can move over to building and installing the DnnVaultApi. Open the solution in Visual Studio and build it in **Release mode**. This will create the installer files in the `.\Website\Install\Modules` directory.
   ![How to build a release package](./ReadMeImages/how-to-build-release.png)

3. Install the `DnnVaultApi` module in the DNN instance.
    
    ![Module Install Step 1](./ReadMeImages/dnn-step-13.1.png)

    ![Module Install Step 2](./ReadMeImages/dnn-step-13.2.png)

    ![Module Install Step 3](./ReadMeImages/dnn-step-13.3.png)

You made it!!! We're all done! Now you should be all set to use the API to Create, Read, Update, and Delete secrets both locally in a web.config file in your Azure Key Vault. 🎉

If you ready to jump right into the code, rebuild the application in debug mode and attach the debugger to the IIS process. You can set breakpoints in the code to see how it works.

## Using the API {#use}
1. First, you'll need to make one more change to your web.config file. Add the following app settings to the `<appSettings>` section:
	```xml
	  <add key="TenantId" value="[Directory (tenant) ID]" />
	  <add key="ClientApplicationId" value="[Your Application (client) ID]" />
	  <add key="Thumbprint" value="[Your Certificate Thumbprint]" />
	  <add key="KeyVaultUri" value="[Your Vault URI]" />
	```
	> Replace `[Directory (tenant) ID]`, `[Your Application (client) ID]`, `[Your Certificate Thumbprint]`, and `[Your Vault URI]` with the values you collected earlier.
    This step will change in the near future as I intend to encrypt one or more sections of the web.config to hide these values properly.
1. Using your favorite browser, log into your DNN instance.
1. Open Dev Tools and navigate to the Console tab.
1. Run the following JavaScript code to test the API by creating a new secret named "DnnVaultApiTestValue" with a value of "SuperSecretValueHandleWithCare":
    ```javascript
    const url = '/DnnVaultApi/API/DnnVaultApi/DnnVaultApi/CreateSecret?secretName=DnnVaultApiTestValue&secretValue=SuperSecretValueHandleWithCare';
    const options = {
        method: 'GET',
        headers: {
            beforeSend: $.ServicesFramework(0)
        }
    };
    new Promise((resolve, reject) => {
        fetch(url, options)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    console.log('Damn. Something went wrong on the server.');
                    console.log(response);
                    reject(response);
                }
            })
            .then(json => {
                console.log(json);
                resolve(json);
            })
            .catch(error => {
                cosole.log('Damn. Something went wrong on the client.');
                console.log(error);
                reject(error);
            });
    });
    ```
      This should return a message indicating that the secret was created successfully.
	```
	A new secret with the name DnnVaultApiTestValue has been successfully created in the Vault.
    ```
1. Run the following JavaScript code to test the API by retrieving the value of the secret you just created (notice that the only thing changed is the URL):
    ```javascript
    const url = '/DnnVaultApi/API/DnnVaultApi/DnnVaultApi/GetSecret?secretName=DnnVaultApiTestValue';
    const options = {
        method: 'GET',
        headers: {
            beforeSend: $.ServicesFramework(0)
        }
    };
    new Promise((resolve, reject) => {
        fetch(url, options)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    console.log('Damn. Something went wrong on the server.');
                    console.log(response);
                    reject(response);
                }
            })
            .then(json => {
                console.log(json);
                resolve(json);
            })
            .catch(error => {
                cosole.log('Damn. Something went wrong on the client.');
                console.log(error);
                reject(error);
            });
    });
    ```

You should see the following output in the console:
```
SuperSecretValueHandleWithCare
```

### Next Steps:
1. Build the UI in the Persona Bar for entering the `[Directory (tenant) ID]`, `[Your Application (client) ID]`, `[Your Certificate Thumbprint]`, and `[Your Vault URI]` values to be saved in the encrypted section of the web.config.
2. Build support for [Bitwarden Secrets Manager](https://bitwarden.com/help/secrets-manager-overview/)

### Conclusion
I have been told that the best way to ask for help online is to make a statement with absolute certainty and wait for the good folks of the world to tell you how wrong you are. With this in mind, I can say with the utmost conviction that the above instructions will work perfectly for you. If they don't, please let me know so that I can correct them. 🙏

### Acknowledgements
I would like to thank the creators of the following resources for their help in creating this project:
1. https://kamranicus.com/azure-key-vault-config-encryption-azure/
1. https://github.com/HoeflingSoftware/Dnn.KeyMaster
1. https://intelequia.com/en/blog/post/storing-azure-app-service-secrets-on-azure-key-vault
1. https://stackoverflow.com/questions/52044838/how-to-use-certificate-from-azure-keyvault-with-httpclient-without-extracting-it
1. https://www.c-sharpcorner.com/article/accessing-azure-key-vaults-using-certification/
1. https://stackoverflow.com/questions/67646500/azure-api-authenticating-apis-with-a-client-certificate-oauth-2-0
1. Upendo Ventures for the [Upendo DNN Generator](https://github.com/UpendoVentures/generator-upendodnn)
2. The DNN Community for the [DNN Platform](https://github.com/dnnsoftware/Dnn.Platform)
