# Changelog

## 17.4.40 (2019-12-24)

### File Manager

#### New Features

- Support has been provided to include a custom message in `AccessRule` class using the message property.

#### Breaking Changes

- Now, in access control, the `FolderRule` and `FileRule` classes are combined into a single `AccessRule` class, where you can specify both folder and file rules by using the `IsFile` property.
- Now, the `Edit` and `EditContents` in access control are renamed as `Write` and `WriteContents`.

## 17.2.34 (2019-07-11)

### ASP.NET Core File System Provider

#### New Features

- Replaced the base model class files with `Syncfusion.EJ2` assembly reference.

## 17.2.28-beta (2019-06-27)

### ASP.NET Core File System Provider

#### New Features

- Added filesystem provider support for ASP.NET Core.
