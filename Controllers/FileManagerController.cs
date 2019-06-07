using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Syncfusion.EJ2.FileManager.Base;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;

namespace EJ2APIServices.Controllers
{

    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class FileManagerController : Controller
    {
        public PhysicalFileProvider operation;
        public string basePath;
        string root = "wwwroot\\Files";
        public FileManagerController(IHostingEnvironment hostingEnvironment)
        {
            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new PhysicalFileProvider();
            this.operation.RootFolder(this.basePath + "\\" + this.root);
        }
        [Route("FileOperations")]
        public object FileOperations([FromBody] FMParams args)
        {
            if (args.action == "Remove" || args.action == "Rename")
            {
                if ((args.targetPath == null) && (args.path == ""))
                {
                    FileManagerResponse response = new FileManagerResponse();
                    ErrorDetails er = new ErrorDetails
                    {
                        Code = "401",
                        Message = "Restricted to modify the root folder."
                    };
                    response.Error = er;
                    return this.operation.ToCamelCase(response);
                }
            }
            switch (args.action)
            {
                case "Read":
                    return this.operation.ToCamelCase(this.operation.GetFiles(args.path, args.showHiddenItems));
                case "Remove":
                    return this.operation.ToCamelCase(this.operation.Remove(args.path, args.itemNames));
                case "CopyTo":
                    return this.operation.ToCamelCase(this.operation.CopyTo(args.path, args.targetPath, args.itemNames, args.renameItems));
                case "MoveTo":
                    return this.operation.ToCamelCase(this.operation.MoveTo(args.path, args.targetPath, args.itemNames, args.renameItems));
                case "GetDetails":
                    return this.operation.ToCamelCase(this.operation.GetDetails(args.path, args.itemNames));
                case "CreateFolder":
                    return this.operation.ToCamelCase(this.operation.CreateFolder(args.path, args.name));
                case "Search":
                    return this.operation.ToCamelCase(this.operation.Search(args.path, args.searchString, args.showHiddenItems, args.caseSensitive));
                case "Rename":
                    return this.operation.ToCamelCase(this.operation.Rename(args.path, args.name, args.itemNewName));
            }
            return null;
        }

        [Route("Upload")]
        public IActionResult Upload(string path, IList<IFormFile> uploadFiles, string action)
        {
            //FileManagerResponse uploadResponse;
            //uploadResponse = operation.Upload(path, uploadFiles, action, null);
            //if (uploadResponse.Error != null)
            //{
            //    Response.Clear();
            //    Response.ContentType = "application/json; charset=utf-8";
            //    Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
            //    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            //}
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.StatusCode = 403;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File Manager's upload functionality is restricted in the online demo. If you need to test upload functionality, please install Syncfusion Essential Studio on your machine and run the demo.";
            return Content("");
        }

        [Route("Download")]
        public IActionResult Download(string downloadInput)
        {
            FMParams args = JsonConvert.DeserializeObject<FMParams>(downloadInput);
            return operation.Download(args.path, args.itemNames);
        }


        [Route("GetImage")]
        public IActionResult GetImage(FMParams args)
        {
            return this.operation.GetImage(args.path,false,null, null);
        }       
    }
    public class FMParams
    {
        public string action { get; set; }

        public string path { get; set; }

        public string targetPath { get; set; }

        public bool showHiddenItems { get; set; }

        public string[] itemNames { get; set; }

        public string name { get; set; }

        public bool caseSensitive { get; set; }
        public string[] renameItems { get; set; }

        public string searchString { get; set; }

        public string itemNewName { get; set; }

        public IList<IFormFile> UploadFiles { get; set; }

        public FileManagerDirectoryContent[] data { get; set; }
    }

}
