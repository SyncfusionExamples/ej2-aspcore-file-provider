using FileManager;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;


namespace EJ2FileManagerServices.Controllers
{

    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class FileManagerController : Controller
    {
        public FileManagerProviderBase operation;
        public string basePath;        
        public FileManagerController(IHostingEnvironment hostingEnvironment)
        {
            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new FileManagerProviderBase(this.basePath + "\\wwwroot\\Files" );
        }
        [Route("FileOperations")]
        public object FileOperations([FromBody] FEParams args)
        {
            if (args.action == "Remove" || args.action == "Rename")
            {
                if ((args.targetPath == null) && (args.path == ""))
                {
                    FileManagerResponse response = new FileManagerResponse();
                    ErrorProperty er = new ErrorProperty
                    {
                        Code = "403",
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
            FileManagerResponse uploadResponse;
            uploadResponse = operation.Upload(path, uploadFiles, action, null);
            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }
            return Content("");
        }

        [Route("Download")]
        public IActionResult Download(string downloadInput)
        {
            FEParams args = JsonConvert.DeserializeObject<FEParams>(downloadInput);      
            return operation.Download(args.path, args.itemNames);
        }


        [Route("GetImage")]
        public IActionResult GetImage(FEParams args)
        {
            return this.operation.GetImage(args.path, true);
        }       
    }
    public class FEParams
    {
        public string action { get; set; }

        public string path { get; set; }

        public string targetPath { get; set; }

        public bool showHiddenItems { get; set; }

        public string[] itemNames { get; set; }

        public string name { get; set; }

        public bool caseSensitive { get; set; }
        public string[] CommonFiles { get; set; }

        public string searchString { get; set; }

        public string itemNewName { get; set; }

        public IList<IFormFile> UploadFiles { get; set; }

        public object[] data { get; set; }
    }

}
