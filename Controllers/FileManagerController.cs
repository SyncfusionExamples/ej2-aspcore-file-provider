using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Syncfusion.EJ2.FileManager.Base;

namespace EJ2APIServices.Controllers
{

    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class FileManagerController : Controller
    {
        private readonly PhysicalFileProvider operation;
        private readonly IWebHostEnvironment hostingEnv;
        private const string Root = "wwwroot\\Files";
        
        public FileManagerController(IWebHostEnvironment hostingEnvironment)
        {
            hostingEnv = hostingEnvironment;
            var basePath = hostingEnvironment.ContentRootPath;
            operation = new PhysicalFileProvider();
            operation.RootFolder(basePath + "\\" + Root);
        }
        
        [Route("FileOperations")]
        public object FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            if (args.Action is "delete" or "rename")
            {
                if ((args.TargetPath == null) && (args.Path == ""))
                {
                    FileManagerResponse response = new()
                    {
                        Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." }
                    };
                    return operation.ToCamelCase(response);
                }
            }

            return args.Action switch
            {
                "read" => operation.ToCamelCase(operation.GetFiles(args.Path, args.ShowHiddenItems)),
                "delete" => operation.ToCamelCase(operation.Delete(args.Path, args.Names)),
                "copy" => operation.ToCamelCase(operation.Copy(args.Path, args.TargetPath, args.Names,
                    args.RenameFiles, args.TargetData)),
                "move" => operation.ToCamelCase(operation.Move(args.Path, args.TargetPath, args.Names,
                    args.RenameFiles, args.TargetData)),
                "details" => operation.ToCamelCase(operation.Details(args.Path, args.Names, args.Data)),
                "create" => operation.ToCamelCase(operation.Create(args.Path, args.Name)),
                "search" => operation.ToCamelCase(operation.Search(args.Path, args.SearchString,
                    args.ShowHiddenItems, args.CaseSensitive)),
                "rename" => operation.ToCamelCase(operation.Rename(args.Path, args.Name, args.NewName)),
                _ => null
            };
        }
        
        
        // uploads the file(s) into a specified path
        [Route("Upload")]
        public IActionResult Upload(string path, IList<IFormFile> uploadFiles, string action)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "/";
            }

            var uploadResponse = operation.Upload(path, uploadFiles, action, null);
            if (uploadResponse.Error == null) return Content("");
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            return Content("");
        }


        // downloads the selected file(s) and folder(s)
        [Route("Download")]
        public IActionResult Download(string downloadInput)
        {
            var args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            return operation.Download(args.Path, args.Names, args.Data);
        }

        // gets the image(s) from the given path
        [Route("GetImage")]
        public IActionResult GetImage(FileManagerDirectoryContent args)
        {
            return this.operation.GetImage(args.Path, args.Id, false, null, null);
        }      
    }

}
