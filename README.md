HockeyApp for Windows
=========

HockeyApp for Windows is the official uploader for windows. The installation comes with two applications (HockeyApp.exe and Hoch.exe) and 3 flavours:

## Configuration Mode

If you start HockeyApp.exe without parameters, you get the configuration mode. In the configuration mode you have to manage your HockeyApp-accounts. One account can be marked as default. Having one account marked as default you do not have to state an account when starting the uploader in the different modes.
After selecting an account the configurator loads all available app-information from the platform. For each app you can manage default values, which are used when updating the platform with a new app-version.
Furthermore the uploader has to link a package with the correlated app-configuration in HockeyApp. This is done in several ways:

### Windows (*.msi)
The MSI-File is parsed for a manifest information. The uploader searches the msi-database information for the values of the keys "ProductVersion" and "ProductName". "ProductVersion" ist used for the version information of the new app-version, the "ProductName" value for the bundle_id.

### Windows Phone (*.xap)
The manifest files "WMAppManifest.xml" and "AppManifest.xaml" are parsed for the keys "Version" and "EntryPointType". The "Version"-value is the information for the new app-version, the "EntryPointType" is used for matching the apps at the platform (bundle_id).

### Android (*.apk)
In th case of android apps, the android manifest tool "aapt.exe" is used to read manifest information. The values of the keys "versionName" is used for the version information of the new app-version and "name" for the bundle_id, which is matched with the bundle_ids of the apps of the platform.

### Custom (*.zip)
There are no default information in a custom app, which can be used. To ease the handling with custom package you can use a RegExp-expression for matching the filename with the app in HockeyApp. Not the whole name of the filename is matched - only the pathinformation and the filename without extension. Only *.zip-Files are allowed.

## Upload-Mode (Dialog Mode)
This mode can be started, when starting the HockeyApp.exe with parameters. The following parameters are supported:

    HockeyApp <PackageFile> [/version] [/configuration] [/app_id] [/bundle_id] [/status] [/notify] [/mandatory] [/notes] [/verbose] [/help]
    
    <PackageFile>: Fully qualified package filename to upload
    [/version]: Version information for new version (overwrites manifest information, if available)
    [/configuration]: (optional) Account name (of omitted, the default configuration is used)
    [/app_id]: (optional) HockeyApp Id (if used, only the app with id is searched)
    [/bundle_id]: (optional) Bundle identifier (overwrited manifest information, if available)
    [/status]: (optional) Status [created, downloadable] (overwrites the default values)
    [/notify]: (optional) Notify [none, eMail] (overwrites the default values)
    [/mandatory]: (optional) Mandatory [true, false] (overwrites the default values)
    [/notes]: (optional) Notes
    [/help]: Prints the usage

Before uploading, a dialog with all necessary information is opened. If more than one app matches the informations, a selection of the matching apps is opened.

## Upload-Mode (command line mode)
For integrating the upload in a built server you have to use the command line tool. The command line tool HOCH.exe uses the same parameters as HockeyApp.exe - except one:

    [/verbose]: (optional) Verbose [true,false] (default is true)

If using HOCH.exe the uploader has to identify one app exactly - if no or more than one app is found, the uploader throws an exception.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Contributor License

You must sign a [Contributor License Agreement](https://cla.microsoft.com/) before submitting your pull request. To complete the Contributor License Agreement (CLA), you will need to submit a request via the [form](https://cla.microsoft.com/) and then electronically sign the CLA when you receive the email containing the link to the document. You need to sign the CLA only once to cover submission to any Microsoft OSS project. 
