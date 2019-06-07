using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FileManager
{
    interface IFileManagerProviderBase
    {
        //public string ContentRootPath;
        FileManagerResponse GetFiles(string path, bool showHiddenItems, params object[] data);

        FileManagerResponse CreateFolder(string path, string name, params object[] data);

        FileManagerResponse GetDetails(string path, string[] names, params object[] data);

        FileManagerResponse Remove(string path, string[] names, params object[] data);

        FileManagerResponse Rename(string path, string name, string newName, bool replace = false, params object[] data);

        FileManagerResponse CopyTo(string path, string targetPath, string[] names, string[] replacedItemNames, params object[] data);

        FileManagerResponse MoveTo(string path, string targetPath, string[] names, string[] replacedItemNames, params object[] data);

        FileManagerResponse Search(string path, string searchString, bool showHiddenItems, bool caseSensitive, params object[] data);

        FileStreamResult Download(string path, string[] names, params object[] data);

        FileManagerResponse Upload(string path, IList<IFormFile> uploadFiles, string action, string[] replacedItemNames, params object[] data);

        FileStreamResult GetImage(string path, bool allowCompress, params object[] data);
    }

    public class FileManagerProviderBase : IFileManagerProviderBase
    {
        public string contentRootPath;
        public string[] allowedExtention = new string[] { "*" };

        public FileManagerProviderBase(string ContentRootPath, string[] AlowedExtention = null)
        {
            this.contentRootPath = ContentRootPath;

            this.allowedExtention = AlowedExtention == null ? this.allowedExtention : allowedExtention;
        }

        public virtual FileManagerResponse GetFiles(string path, bool showHiddenItems, params object[] data)
        {
            FileManagerResponse readResponse = new FileManagerResponse();
            try
            {
                if (path == null)
                {
                    path = string.Empty;
                }
                String fullPath = (contentRootPath + path);
                var directory = new DirectoryInfo(fullPath);
                var extensions = this.allowedExtention;
                FileManagerDirectoryContent cwd = new FileManagerDirectoryContent();
                cwd.Name = directory.Name;
                cwd.Size = 0;
                cwd.IsFile = false;
                cwd.DateModified = directory.LastWriteTime;
                cwd.DateCreated = directory.CreationTime;
                cwd.HasChild = directory.GetDirectories().Length > 0 ? true : false;
                cwd.Type = directory.Extension;
                cwd.FilterPath = '\\' + GetRelativePath(this.contentRootPath, directory.FullName);
                readResponse.Files = ReadDirectories(directory, extensions, showHiddenItems, data);
                readResponse.CWD = cwd;
                readResponse.Files = readResponse.Files.Concat(ReadFiles(directory, extensions, showHiddenItems, data));
                return readResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                readResponse.Error = er;

                return readResponse;
            }
        }

        public virtual IEnumerable<FileManagerDirectoryContent> ReadFiles(DirectoryInfo directory, string[] extensions, bool showHiddenItems, params object[] data)
        {
            try
            {
                FileManagerResponse readFiles = new FileManagerResponse();
                if (!showHiddenItems)
                {
                    var files = extensions.SelectMany(directory.GetFiles).Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                            .Select(file => new FileManagerDirectoryContent
                            {
                                Name = file.Name,
                                IsFile = true,
                                Size = file.Length,
                                DateModified = file.LastWriteTime,
                                DateCreated = file.CreationTime,
                                HasChild = false,
                                Type = file.Extension,
                                FilterPath = '\\' + GetRelativePath(this.contentRootPath, directory.FullName)
                            });
                    readFiles.Files = (IEnumerable<FileManagerDirectoryContent>)files;
                }
                else
                {
                    var files = extensions.SelectMany(directory.GetFiles)
                            .Select(file => new FileManagerDirectoryContent
                            {
                                Name = file.Name,
                                IsFile = true,
                                Size = file.Length,
                                DateModified = file.LastWriteTime,
                                DateCreated = file.CreationTime,
                                HasChild = false,
                                Type = file.Extension,
                                FilterPath = '\\' + GetRelativePath(this.contentRootPath, directory.FullName)
                            });
                    readFiles.Files = (IEnumerable<FileManagerDirectoryContent>)files;
                }
                return readFiles.Files;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string GetRelativePath(string rootPath, string fullPath)
        {
            if (!String.IsNullOrEmpty(rootPath) && !String.IsNullOrEmpty(fullPath))
            {
                var rootDirectory = new DirectoryInfo(rootPath);
                if (fullPath.Contains(rootDirectory.FullName + "\\"))
                {
                    return fullPath.Substring(rootPath.Length + 1);
                }
            }
            return String.Empty;
        }


        public virtual IEnumerable<FileManagerDirectoryContent> ReadDirectories(DirectoryInfo directory, string[] extensions, bool showHiddenItems, params object[] data)
        {
            FileManagerResponse readDirectory = new FileManagerResponse();
            try
            {
                if (!showHiddenItems)
                {
                    var directories = directory.GetDirectories().Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                            .Select(subDirectory => new FileManagerDirectoryContent
                            {
                                Name = subDirectory.Name,
                                Size = 0,
                                IsFile = false,
                                DateModified = subDirectory.LastWriteTime,
                                DateCreated = subDirectory.CreationTime,
                                HasChild = subDirectory.GetDirectories().Length > 0 ? true : false,
                                Type = subDirectory.Extension,
                                FilterPath = '\\' + GetRelativePath(this.contentRootPath, subDirectory.FullName)
                            });
                    readDirectory.Files = (IEnumerable<FileManagerDirectoryContent>)directories;
                }
                else
                {
                    var directories = directory.GetDirectories().Select(subDirectory => new FileManagerDirectoryContent
                    {
                        Name = subDirectory.Name,
                        Size = 0,
                        IsFile = false,
                        DateModified = subDirectory.LastWriteTime,
                        DateCreated = subDirectory.CreationTime,
                        HasChild = subDirectory.GetDirectories().Length > 0 ? true : false,
                        Type = subDirectory.Extension,
                        FilterPath = '\\' + GetRelativePath(this.contentRootPath, subDirectory.FullName)
                    });
                    readDirectory.Files = (IEnumerable<FileManagerDirectoryContent>)directories;
                }
                return readDirectory.Files;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public virtual FileManagerResponse CreateFolder(string path, string name, params object[] data)
        {
            FileManagerResponse createResponse = new FileManagerResponse();
            try
            {
                var newDirectoryPath = Path.Combine(contentRootPath + path, name);

                var directoryExist = Directory.Exists(newDirectoryPath);

                if (directoryExist)
                {
                    var exist = new DirectoryInfo(newDirectoryPath);
                    ErrorProperty er = new ErrorProperty();
                    er.Code = "400";
                    er.Message = "A file or folder with the name " + exist.Name.ToString() + " already exists.";
                    createResponse.Error = er;

                    return createResponse;
                }

                Directory.CreateDirectory(newDirectoryPath);
                var directory = new DirectoryInfo(newDirectoryPath);
                FileManagerDirectoryContent CreateData = new FileManagerDirectoryContent();
                CreateData.Name = directory.Name;
                CreateData.IsFile = false;
                CreateData.Size = 0;
                CreateData.DateModified = directory.LastWriteTime;
                CreateData.DateCreated = directory.CreationTime;
                CreateData.HasChild = directory.GetDirectories().Length > 0 ? true : false; ;
                CreateData.Type = directory.Extension;
                var newData = new FileManagerDirectoryContent[] { CreateData };
                createResponse.Files = newData;
                return createResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                createResponse.Error = er;

                return createResponse;
            }
        }
        ////need to be work on GetInfo
        public virtual FileManagerResponse GetDetails(string path, string[] names, params object[] data)
        {
            FileManagerResponse getDetailResponse = new FileManagerResponse();
            FileDetails detailFiles = new FileDetails();
            try
            {
                if (names.Length == 1)
                {
                    if (path == null) { path = string.Empty; };
                    var fullPath = "";
                    if (names[0] == null || names[0] == "")
                    {
                        fullPath = (contentRootPath + path);
                    }
                    else
                    {
                        fullPath = Path.Combine(contentRootPath + path, names[0]);
                    }
                    var directory = new DirectoryInfo(fullPath);
                    FileInfo info = new FileInfo(fullPath);
                    FileDetails fileDetails = new FileDetails();
                    var baseDirectory = new DirectoryInfo(this.contentRootPath);
                    fileDetails.Name = info.Name == "" ? directory.Name : info.Name;
                    fileDetails.IsFile = info.Attributes.ToString() == "Directory" ? false : true;
                    fileDetails.Size = info.Attributes.ToString() != "Directory" ? byteConversion(info.Length).ToString() : byteConversion(new DirectoryInfo(fullPath).EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => (file.Length))).ToString();
                    fileDetails.Created = info.CreationTime;
                    fileDetails.Location = '\\' + GetRelativePath(baseDirectory.Parent.FullName, info.FullName);
                    fileDetails.Modified = info.LastWriteTime;
                    detailFiles = fileDetails;
                }
                else
                {
                    FileDetails fileDetails = new FileDetails();
                    fileDetails.Size = "0";
                    for (int i = 0; i < names.Length; i++)
                    {
                        var fullPath = "";
                        if (names[i] == null)
                        {
                            fullPath = (contentRootPath + path);
                        }
                        else
                        {
                            fullPath = Path.Combine(contentRootPath + path, names[i]);
                        }
                        var baseDirectory = new DirectoryInfo(this.contentRootPath);
                        FileInfo info = new FileInfo(fullPath);
                        fileDetails.Name = string.Join(", ", names);
                        fileDetails.Size = (long.Parse(fileDetails.Size) + ((info.Extension != "") ? info.Length : new DirectoryInfo(fullPath).EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length))).ToString();
                        fileDetails.Location = '\\' + GetRelativePath(baseDirectory.Parent.FullName, info.Directory.FullName);
                    }
                    fileDetails.Size = byteConversion(long.Parse(fileDetails.Size)).ToString();
                    fileDetails.MultipleFiles = true;
                    detailFiles = fileDetails;
                }
                getDetailResponse.Details = detailFiles;
                return getDetailResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                getDetailResponse.Error = er;

                return getDetailResponse;
            }
        }

        public virtual FileManagerResponse Remove(string path, string[] names, params object[] data)
        {
            FileManagerResponse DeleteResponse = new FileManagerResponse();
            FileManagerDirectoryContent[] removedFiles = new FileManagerDirectoryContent[names.Length];
            try
            {
                for (int i = 0; i < names.Length; i++)
                {
                    var fullPath = Path.Combine((contentRootPath + path), names[i]);
                    var directory = new DirectoryInfo(fullPath);
                    if (!string.IsNullOrEmpty(names[i]))
                    {
                        FileAttributes attr = File.GetAttributes(fullPath);
                        removedFiles[i] = GetFileDetails(fullPath);
                        //detect whether its a directory or file
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            DeleteDirectory(fullPath);
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("name should not be null");
                    }
                }
                DeleteResponse.Files = removedFiles;
                return DeleteResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                DeleteResponse.Error = er;

                return DeleteResponse;
            }
        }

        public virtual FileManagerResponse Rename(string path, string name, string newName, bool replace = false, params object[] data)
        {
            FileManagerResponse renameResponse = new FileManagerResponse();
            try
            {
                var tempPath = (contentRootPath + path);
                var oldPath = Path.Combine(tempPath, name);
                var newPath = Path.Combine(tempPath, newName);
                FileAttributes attr = File.GetAttributes(oldPath);

                FileInfo info = new FileInfo(oldPath);
                var isFile = (info.Attributes.ToString() != "Directory") ? true : false;
                if (isFile)
                {
                    info.MoveTo(newPath);
                }
                else
                {
                    var directoryExist = Directory.Exists(newPath);
                    if (directoryExist)
                    {
                        var exist = new DirectoryInfo(newPath);
                        ErrorProperty er = new ErrorProperty();
                        er.Code = "400";
                        er.Message = "Cannot rename " + exist.Name.ToString() + " to " + newName + ": destination already exists.";
                        renameResponse.Error = er;

                        return renameResponse;
                    }
                    else
                    {
                        Directory.Move(oldPath, newPath);
                    }
                }
                var addedData = new[]{
                        GetFileDetails(newPath)
                    };
                renameResponse.Files = addedData;
                return renameResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                renameResponse.Error = er;

                return renameResponse;
            }
        }

        public virtual FileManagerResponse CopyTo(string path, string targetPath, string[] names, string[] replacedItemNames, params object[] data)
        {
            FileManagerResponse copyResponse = new FileManagerResponse();
            try
            {
                if (replacedItemNames.Length > 0)
                {
                    names = replacedItemNames;
                }
                FileManagerDirectoryContent[] copiedFiles = new FileManagerDirectoryContent[names.Length];
                var existFiles = new List<string>();
                var tempPath = Path.Combine(contentRootPath, path);
                for (int i = 0; i < names.Length; i++)
                {
                    FileAttributes fileAttributes = File.GetAttributes(Path.Combine(contentRootPath, path, names[i]));
                    if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory) //Fixed if the directory is compressed
                    {
                        var directoryName = names[i];
                        var oldPath = Path.Combine(contentRootPath, path, directoryName);
                        var newPath = Path.Combine(contentRootPath, targetPath, directoryName);
                        var exist = Directory.Exists(newPath);
                        if (exist && !(replacedItemNames.Length > 0))
                        {
                            if (newPath == oldPath)
                            {
                                int directoryCount = 0;
                                while (System.IO.Directory.Exists(newPath + (directoryCount > 0 ? "(" + directoryCount.ToString() + ")" : "")))
                                {
                                    directoryCount++;
                                }
                                newPath = newPath + (directoryCount > 0 ? "(" + directoryCount.ToString() + ")" : "");
                                DirectoryCopy(oldPath, newPath, replacedItemNames, "copy");
                                var directory = new DirectoryInfo(newPath);
                                copiedFiles[i] = GetFileDetails(newPath);
                            }
                            else
                            {
                                existFiles.Add(newPath);
                            }
                        }
                        else
                        {
                            DirectoryCopy(oldPath, newPath, replacedItemNames, "copy");
                            var directory = new DirectoryInfo(newPath);
                            copiedFiles[i] = GetFileDetails(newPath);
                        }
                    }
                    else
                    {
                        var fileName = names[i];
                        var newFilePath = Path.Combine(targetPath, fileName);
                        var oldPath = Path.Combine(contentRootPath, path, fileName);
                        var newPath = Path.Combine(contentRootPath, targetPath, fileName);
                        var fileExist = System.IO.File.Exists(newPath);

                        if (fileExist && !(replacedItemNames.Length > 0))
                        {
                            if (newPath == oldPath)
                            {
                                int name = newPath.LastIndexOf(".");
                                if (name >= 0)
                                    newPath = newPath.Substring(0, name);
                                int fileCount = 0;
                                while (System.IO.File.Exists(newPath + (fileCount > 0 ? "(" + fileCount.ToString() + ")" + Path.GetExtension(fileName) : Path.GetExtension(fileName))))
                                {
                                    fileCount++;
                                }
                                newPath = newPath + (fileCount > 0 ? "(" + fileCount.ToString() + ")" : "") + Path.GetExtension(fileName);
                                File.Copy(oldPath, newPath);
                                copiedFiles[i] = GetFileDetails(newPath);
                            }
                            else
                            {
                                existFiles.Add(newPath);
                            }
                        }
                        else
                        {
                            if (replacedItemNames.Length > 0)
                            {
                                File.Delete(newPath);
                            }
                            File.Copy(oldPath, newPath);
                            copiedFiles[i] = GetFileDetails(newPath);
                        }
                    }
                }
                copyResponse.Files = copiedFiles;
                if (existFiles.Count > 0)
                {
                    ErrorProperty er = new ErrorProperty();
                    er.FileExists = existFiles;

                    copyResponse.Error = er;
                }
                return copyResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                copyResponse.Error = er;

                return copyResponse;
            }
        }

        public virtual FileManagerResponse MoveTo(string path, string targetPath, string[] names, string[] replacedItemNames = null, params object[] data)
        {
            FileManagerResponse pasteResponse = new FileManagerResponse();
            try
            {
                if (replacedItemNames.Length > 0)
                {
                    names = replacedItemNames;
                }
                var existFiles = new List<string>();
                FileManagerDirectoryContent[] copiedFiles = new FileManagerDirectoryContent[names.Length];
                var tempPath = Path.Combine(contentRootPath, path);
                for (int i = 0; i < names.Length; i++)
                {
                    FileAttributes fileAttributes = File.GetAttributes(Path.Combine(contentRootPath, path, names[i]));
                    if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory) //Fixed if the directory is compressed
                    {
                        var directoryName = names[i];
                        var oldPath = Path.Combine(contentRootPath, path, directoryName);
                        var newPath = Path.Combine(contentRootPath, targetPath, directoryName);
                        var exist = File.Exists(newPath);
                        if (exist && (replacedItemNames.Length < 0))
                        {
                            existFiles.Add(newPath);
                        }
                        else
                        {
                            DirectoryCopy(oldPath, newPath, replacedItemNames, "paste");
                            var directory = new DirectoryInfo(newPath);
                            copiedFiles[i] = GetFileDetails(newPath);
                        }
                    }
                    else
                    {
                        var fileName = names[i];
                        var newFilePath = Path.Combine(targetPath, fileName);
                        var oldPath = Path.Combine(contentRootPath, path, fileName);
                        var newPath = Path.Combine(contentRootPath, targetPath, fileName);
                        var fileExist = File.Exists(newPath);

                        if (fileExist && !(replacedItemNames.Length > 0))
                        {
                            existFiles.Add(newPath);
                        }
                        else
                        {
                            if (replacedItemNames.Length > 0)
                            {
                                File.Delete(newPath);
                            }
                            File.Move(oldPath, newPath);
                            copiedFiles[i] = GetFileDetails(newPath);
                        }
                    }
                }
                pasteResponse.Files = copiedFiles;
                if (existFiles.Count > 0)
                {
                    ErrorProperty er = new ErrorProperty();
                    er.FileExists = existFiles;

                    pasteResponse.Error = er;
                }
                return pasteResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                pasteResponse.Error = er;
                return pasteResponse;
            }
        }

        public virtual FileManagerResponse Search(string path, string searchString, bool showHiddenItems = false, bool caseSensitive = false, params object[] data)
        {
            FileManagerResponse searchResponse = new FileManagerResponse();
            try
            {
                if (path == null) { path = string.Empty; };

                var searchWord = searchString;
                var searchPath = (this.contentRootPath + path);
                var directory = new DirectoryInfo((this.contentRootPath + path));
                FileManagerDirectoryContent cwd = new FileManagerDirectoryContent();
                cwd.Name = directory.Name;
                cwd.Size = 0;
                cwd.IsFile = false;
                cwd.DateModified = directory.LastWriteTime;
                cwd.DateCreated = directory.CreationTime;
                cwd.HasChild = directory.GetDirectories().Length > 0 ? true : false;
                cwd.Type = directory.Extension;
                cwd.FilterPath = '\\' + GetRelativePath(this.contentRootPath, directory.FullName);
                searchResponse.CWD = cwd;

                List<FileManagerDirectoryContent> foundedFiles = new List<FileManagerDirectoryContent>();
                var extensions = this.allowedExtention;
                var searchDirectory = new DirectoryInfo(searchPath);
                if (showHiddenItems)
                {
                    var filteredFileList = searchDirectory.GetFiles(searchString, SearchOption.AllDirectories).
                        Where(item => new Regex(WildcardToRegex(searchString), (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name));
                    var filteredDirectoryList = searchDirectory.GetDirectories(searchString, SearchOption.AllDirectories).
                        Where(item => new Regex(WildcardToRegex(searchString), (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name));
                    foreach (FileInfo file in filteredFileList)
                    {
                        foundedFiles.Add(GetFileDetails(Path.Combine(this.contentRootPath, file.DirectoryName, file.Name)));
                    }
                    foreach (DirectoryInfo dir in filteredDirectoryList)
                    {
                        foundedFiles.Add(GetFileDetails(Path.Combine(this.contentRootPath, dir.FullName)));
                    }
                }
                else
                {
                    var filteredFileList = searchDirectory.GetFiles(searchString, SearchOption.AllDirectories).
                        Where(item => new Regex(WildcardToRegex(searchString), (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name) && (item.Attributes & FileAttributes.Hidden) == 0);
                    var filteredDirectoryList = searchDirectory.GetDirectories(searchString, SearchOption.AllDirectories).
                        Where(item => new Regex(WildcardToRegex(searchString), (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name) && (item.Attributes & FileAttributes.Hidden) == 0);
                    foreach (FileInfo file in filteredFileList)
                    {
                        foundedFiles.Add(GetFileDetails(Path.Combine(this.contentRootPath, file.DirectoryName, file.Name)));
                    }
                    foreach (DirectoryInfo dir in filteredDirectoryList)
                    {
                        foundedFiles.Add(GetFileDetails(Path.Combine(this.contentRootPath, dir.FullName)));
                    }
                }
                searchResponse.Files = (IEnumerable<FileManagerDirectoryContent>)foundedFiles;
                return searchResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Code = "404";
                er.Message = e.Message.ToString();
                searchResponse.Error = er;

                return searchResponse;
            }
        }

        public String byteConversion(long fileSize)
        {
            try
            {
                string[] index = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
                if (fileSize == 0)
                {
                    return "0 " + index[0];
                }

                long bytes = Math.Abs(fileSize);
                int loc = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, loc), 1);
                return (Math.Sign(fileSize) * num).ToString() + " " + index[loc];
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public virtual string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }

        public virtual FileStreamResult GetImage(string path, bool allowCompress, params object[] data)
        {
            try
            {
                String fullPath = (contentRootPath + path);
                FileStream fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                FileStreamResult fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
                return fileStreamResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual FileManagerResponse Upload(string path, IList<IFormFile> uploadFiles, string action, string[] replacedItemNames, params object[] data)
        {
            FileManagerResponse uploadResponse = new FileManagerResponse();
            try
            {
                var existFiles = new List<string>();
                foreach (var file in uploadFiles)
                {
                    if (uploadFiles != null)
                    {
                        var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        filename = Path.Combine(this.contentRootPath + path, filename);
                        if (action == "Remove")
                        {
                            if (System.IO.File.Exists(filename))
                            {
                                System.IO.File.Delete(filename);
                            }
                        }
                        else
                        {
                            if (!System.IO.File.Exists(filename))
                            {
                                using (FileStream fs = System.IO.File.Create(filename))
                                {
                                    file.CopyTo(fs);
                                    fs.Flush();
                                }
                            }
                            else
                            {
                                existFiles.Add(filename);
                            }
                        }
                    }
                }
                if (existFiles.Count != 0)
                {
                    ErrorProperty er = new ErrorProperty();
                    er.Message = "File already exists.";
                    er.FileExists = existFiles;
                    uploadResponse.Error = er;
                }
                return uploadResponse;
            }
            catch (Exception e)
            {
                ErrorProperty er = new ErrorProperty();
                er.Message = e.Message.ToString();
                uploadResponse.Error = er;

                return uploadResponse;
            }
        }

        public virtual FileStreamResult Download(string path, string[] names, params object[] data)
        {
            try
            {
                String extension;
                int count = 0;
                for (var i = 0; i < names.Length; i++)
                {
                    extension = Path.GetExtension(names[i]);
                    if (extension != "")
                    {
                        count++;
                    }
                }
                if (count == names.Length)
                {
                    return DownloadFile(path, names);
                }
                else
                {
                    return DownloadFolder(path, names, count);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private FileStreamResult fileStreamResult;
        public virtual FileStreamResult DownloadFile(string path, string[] names = null)
        {
            try
            {
                path = Path.GetDirectoryName(path);
                var tempPath = Path.Combine(Path.GetTempPath(), "temp.zip");
                String fullPath;
                if (names == null || names.Length == 0)
                {
                    fullPath = (contentRootPath + path);
                    byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                    FileStream fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
                }
                else if (names.Length == 1)
                {
                    fullPath = Path.Combine(contentRootPath + path, names[0]);
                    FileStream fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
                    fileStreamResult.FileDownloadName = names[0];
                }
                else if (names.Length > 1)
                {
                    string fileName = Guid.NewGuid().ToString() + "temp.zip";
                    string newFileName = fileName.Substring(36);
                    tempPath = Path.Combine(Path.GetTempPath(), newFileName);
                    string currentDirectory;
                    ZipArchiveEntry zipEntry;
                    ZipArchive archive;
                    for (int i = 0; i < names.Count(); i++)
                    {
                        fullPath = Path.Combine((contentRootPath + path), names[i]);
                        if (!string.IsNullOrEmpty(fullPath))
                        {
                            try
                            {
                                using (archive = ZipFile.Open(tempPath, ZipArchiveMode.Update))
                                {
                                    currentDirectory = Path.Combine((contentRootPath + path), names[i]);
                                    zipEntry = archive.CreateEntryFromFile(Path.Combine(this.contentRootPath, currentDirectory), names[i], CompressionLevel.Fastest);
                                }
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException("name should not be null");
                        }
                    }
                    try
                    {
                        FileStream fileStreamInput = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
                        fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
                        fileStreamResult.FileDownloadName = "files.zip";
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return fileStreamResult;
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected FileStreamResult DownloadFolder(string path, string[] names, int count)
        {
            try
            {
                path = Path.GetDirectoryName(path);
                FileStreamResult fileStreamResult;
                // create a temp.Zip file intially 
                var tempPath = Path.Combine(Path.GetTempPath(), "temp.zip");
                String fullPath;
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                if (names.Length == 1)
                {
                    fullPath = Path.Combine(contentRootPath + path, names[0]);
                    ZipFile.CreateFromDirectory(fullPath, tempPath, CompressionLevel.Fastest, true);
                    FileStream fileStreamInput = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
                    fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
                    fileStreamResult.FileDownloadName = names[0] + ".zip";
                }
                else
                {
                    string extension;
                    string currentDirectory;
                    ZipArchiveEntry zipEntry;
                    ZipArchive archive;
                    if (count == 0)
                    {
                        string directory = this.contentRootPath + path;
                        using (archive = ZipFile.Open(tempPath, ZipArchiveMode.Update))
                        {
                            for (var i = 0; i < names.Length; i++)
                            {
                                currentDirectory = directory;
                                foreach (var filePath in Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories))
                                {
                                    zipEntry = archive.CreateEntryFromFile(filePath, names[i] + filePath.Substring(currentDirectory.Length), CompressionLevel.Fastest);
                                }
                            }
                        }
                    }
                    else
                    {
                        string lastSelected = names[names.Length - 1];
                        string selectedExtension = Path.GetExtension(lastSelected);
                        using (archive = ZipFile.Open(tempPath, ZipArchiveMode.Update))
                        {
                            for (var i = 0; i < names.Length; i++)
                            {
                                extension = Path.GetExtension(names[i]);
                                currentDirectory = Path.Combine((contentRootPath + path), names[i]);
                                if (extension == "")
                                {
                                    if (Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories).Length == 0)
                                    {
                                        zipEntry = archive.CreateEntry(names[i] + "/");
                                    }
                                    else
                                    {
                                        foreach (var filePath in Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories))
                                        {
                                            zipEntry = archive.CreateEntryFromFile(Path.Combine(this.contentRootPath, filePath), filePath.Substring(path.Length), CompressionLevel.Fastest);
                                        }
                                    }

                                }
                                else
                                {
                                    zipEntry = archive.CreateEntryFromFile(Path.Combine(this.contentRootPath, currentDirectory), names[i], CompressionLevel.Fastest);
                                }
                            }
                        }

                    }
                    FileStream fileStreamInput = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
                    fileStreamResult = new FileStreamResult(fileStreamInput, "application/force-download");
                    fileStreamResult.FileDownloadName = "folders.zip";
                }
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                return fileStreamResult;
            }
            catch (Exception)
            {
                return null;
            }
        }


        private void DirectoryCopy(string sourceDirName, string destDirName, string[] replacedItemNames, string action)
        {
            try
            {
                // Gets the subdirectories for the specified directory.
                var dir = new DirectoryInfo(sourceDirName);

                var dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, creates it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Gets the files in the directory and copy them to the new location.
                var files = dir.GetFiles();
                foreach (var file in files)
                {
                    var oldPath = Path.Combine(sourceDirName, file.Name);
                    var temppath = Path.Combine(destDirName, file.Name);
                    var fileExist = File.Exists(temppath);
                    if (!fileExist)
                    {
                        if (action != "paste")
                        {
                            file.CopyTo(temppath, true);
                        }
                        else
                        {
                            File.Move(oldPath, temppath);
                        }
                    }
                    else if (fileExist && replacedItemNames.Length > 0)
                    {
                        File.Delete(temppath);
                        if (action != "paste")
                        {
                            file.CopyTo(temppath, true);
                        }
                        else
                        {
                            File.Move(oldPath, temppath);
                        }
                    }
                }
                if (action == "paste")
                {
                    DeleteDirectory(sourceDirName);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public virtual void DeleteDirectory(string path)
        {
            try
            {
                string[] files = Directory.GetFiles(path);
                string[] dirs = Directory.GetDirectories(path);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }
                Directory.Delete(path, true);
            }
            catch (IOException e)
            {
                throw e;
            }
        }
        public virtual FileManagerDirectoryContent GetFileDetails(string path)
        {
            try
            {
                FileInfo info = new FileInfo(path);
                FileAttributes attr = File.GetAttributes(path);
                FileInfo detailPath = new FileInfo(info.FullName);
                var folderLength = 0;
                var isFile = ((attr & FileAttributes.Directory) == FileAttributes.Directory) ? false : true;
                if (!isFile)
                {
                    folderLength = detailPath.Directory.GetDirectories().Length;
                }
                return new FileManagerDirectoryContent
                {
                    Name = info.Name,
                    Size = isFile ? info.Length : 0,
                    IsFile = isFile,
                    DateModified = info.LastWriteTime,
                    DateCreated = info.CreationTime,
                    Type = info.Extension,
                    HasChild = isFile ? false : (info.Directory.GetDirectories().Length > 0 ? true : false),
                    FilterPath = '\\' + GetRelativePath(this.contentRootPath, info.FullName)
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public string ToCamelCase(FileManagerResponse userData)
        {
            return JsonConvert.SerializeObject(userData, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
        }
    }


    public class FileManagerResponse
    {
        public FileManagerDirectoryContent CWD { get; set; }
        public IEnumerable<FileManagerDirectoryContent> Files { get; set; }

        public ErrorProperty Error { get; set; }

        public FileDetails Details { get; set; }

    }
    public class ErrorProperty
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public IEnumerable<string> FileExists { get; set; }
    }
    public class ImageSize
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
    public class FileDetails
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public bool IsFile { get; set; }
        public string Size { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool MultipleFiles { get; set; }
    }
    public class FileManagerParams
    {
        public string Name { get; set; }

        public string[] Names { get; set; }

        public string Path { get; set; }

        public string TargetPath { get; set; }

        public string NewName { get; set; }

        public object Date { get; set; }

        public IEnumerable<IFormFile> FileUpload { get; set; }

        public string[] ReplacedItemNames { get; set; }
    }
    public class FileManagerDirectoryContent
    {

        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateCreated { get; set; }

        public bool HasChild { get; set; }

        public bool IsFile { get; set; }

        public string Type { get; set; }

        public string FilterPath { get; set; }
    }
}
