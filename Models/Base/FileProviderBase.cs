using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Syncfusion.EJ2.FileManager.Base
{
    public interface FileProviderBase
    {

        FileManagerResponse GetFiles(string path, bool showHiddenItems, params FileManagerDirectoryContent[] data);
        FileManagerResponse Create(string path, string name, params FileManagerDirectoryContent[] data);

        FileManagerResponse Details(string path, string[] names, params FileManagerDirectoryContent[] data);

        FileManagerResponse Delete(string path, string[] names, params FileManagerDirectoryContent[] data);

        FileManagerResponse Rename(string path, string name, string newName, bool replace = false, bool showFileExtension = true, params FileManagerDirectoryContent[] data);

        FileManagerResponse Copy(string path, string targetPath, string[] names, string[] renameFiles, FileManagerDirectoryContent targetData, params FileManagerDirectoryContent[] data);

        FileManagerResponse Move(string path, string targetPath, string[] names, string[] renameFiles, FileManagerDirectoryContent targetData, params FileManagerDirectoryContent[] data);

        FileManagerResponse Search(string path, string searchString, bool showHiddenItems, bool caseSensitive, params FileManagerDirectoryContent[] data);

        FileStreamResult Download(string path, string[] names, params FileManagerDirectoryContent[] data);
#if EJ2_DNX
            FileManagerResponse Upload(string path, IList<System.Web.HttpPostedFileBase> uploadFiles, string action, params FileManagerDirectoryContent[] data);
#else
        FileManagerResponse Upload(string path, IList<IFormFile> uploadFiles, string action, params FileManagerDirectoryContent[] data);
#endif

        FileStreamResult GetImage(string path, string id, bool allowCompress, ImageSize size, params FileManagerDirectoryContent[] data);
      
    }

}





