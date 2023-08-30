# ASP.NET Core service for the file manager component

This repository contains the ASP.NET Core file system providers for the Essential JS 2 File Manager component.

To know more about file system provider for File Manager, please refer our documentation [here](https://ej2.syncfusion.com/aspnetcore/documentation/file-manager/file-system-provider).

## Key Features

A file system provider is an API for access to the physical file system in the FileManager control. It also provides the methods for performing various file actions like creating a new folder, renaming files and deleting files.

ASP.NET Core file system provider serves the physical file system for the file manager component.

The following actions can be performed with ASP.NET Core file system Provider.

| **Actions** | **Description** |
| --- | --- |
| Read      | Read the files from the local file storage. |
| Details   | Gets a file's metadata which consists of Type, Size, Location and Modified date. |
| Download  | Download the selected file or folder. |
| Upload    | Upload's the file. Accepts uploaded media with the following characteristics: <ul><li>Maximum file size:  30MB</li><li>Accepted Media MIME types: `*/*`</li></ul> |
| Create    | Create a new folder. |
| Delete    | Delete a folder or file. |
| Copy      | Copies the contents of the file from the target location . |
| Move      | Paste the copied files to the desired location. |
| Rename    | Rename a folder or file. |
| Search    | Search a file or folder. |

## Prerequisites

* Visual Studio 2022

## How to run the project

* Checkout this project to a location in your disk.
* Open the solution file using Visual Studio 2022.
* Restore the NuGet packages by rebuilding the solution.
* Run the project.

## File Manager AjaxSettings

To access the basic actions such as Read, Delete, Copy, Move, Rename, Search, and Get Details of File Manager using Azure service, just map the following code snippet in the Ajaxsettings property of File Manager.

Here, the `hostUrl` will be your locally hosted port number.

```
  var hostUrl = http://localhost:62870/;
  ajaxSettings: {
        url: hostUrl + 'api/FileManager/FileOperations'
  }
```

## File download AjaxSettings

To perform download operation, initialize the `downloadUrl` property in ajaxSettings of the File Manager component.

```
  var hostUrl = http://localhost:62870/;
  ajaxSettings: {
        url: hostUrl + 'api/FileManager/FileOperations',
        downloadUrl: hostUrl +'api/FileManager/Download'
  }
```

## File upload AjaxSettings

To perform upload operation, initialize the `uploadUrl` property in ajaxSettings of the File Manager component.

```
  var hostUrl = http://localhost:62870/;
  ajaxSettings: {
        url: hostUrl + 'api/FileManager/FileOperations',
        uploadUrl: hostUrl +'api/FileManager/Upload'
  }
```

## File image preview AjaxSettings

To perform image preview support in the File Manager component, initialize the `getImageUrl` property in ajaxSettings of the File Manager component.

```
  var hostUrl = http://localhost:62870/;
  ajaxSettings: {
        url: hostUrl + 'api/FileManager/FileOperations',
         getImageUrl: hostUrl +'api/FileManager/GetImage'
  }
```

The FileManager will be rendered as the following.

![File Manager](https://ej2.syncfusion.com/products/images/file-manager/readme.gif)

## Support

Product support is available for through following mediums.

* Creating incident in Syncfusion [Direct-trac](https://www.syncfusion.com/support/directtrac/incidents?utm_source=npm&utm_campaign=filemanager) support system or [Community forum](https://www.syncfusion.com/forums/essential-js2?utm_source=npm&utm_campaign=filemanager).
* New [GitHub issue](https://github.com/syncfusion/ej2-javascript-ui-controls/issues/new).
* Ask your query in [Stack Overflow](https://stackoverflow.com/?utm_source=npm&utm_campaign=filemanager) with tag `syncfusion` and `ej2`.

## License

Check the license detail [here](https://github.com/syncfusion/ej2-javascript-ui-controls/blob/master/license).

## Changelog

Check the changelog [here](https://github.com/syncfusion/ej2-javascript-ui-controls/blob/master/controls/filemanager/CHANGELOG.md)

© Copyright 2023 Syncfusion, Inc. All Rights Reserved. The Syncfusion Essential Studio license and copyright applies to this distribution.