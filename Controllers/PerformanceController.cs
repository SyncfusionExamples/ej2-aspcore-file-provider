using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.FileManager.Base;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EJ2APIServices.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class PerformanceController : Controller
    {

        private readonly string _basePath;
        private readonly string _perfRootRelative = "wwwroot\\Files\\Perf";

        public PerformanceController(IWebHostEnvironment env)
        {
            _basePath = env.ContentRootPath;
        }

        private string GetPerfRoot(string root)
        {
            var safeRoot = string.IsNullOrWhiteSpace(root) ? "" : root.Trim();
            if (safeRoot.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("Invalid root name.");

            return Path.Combine(_basePath, _perfRootRelative, safeRoot);
        }

        [HttpGet("Generate")]
        public IActionResult Generate([FromQuery] int count)
        {
            if (count != 500 && count != 1000 && count != 2000 && count != 5000)
                return BadRequest(new { success = false, message = "Only 500, 1000, 2000, 5000 are supported." });

            var perfRootBase = Path.Combine(_basePath, _perfRootRelative);
            if (!Directory.Exists(perfRootBase))
            {
                Directory.CreateDirectory(perfRootBase);
            }

            var rootName = $"Perf-{count}";
            var rootPath = GetPerfRoot(rootName);

            if (Directory.Exists(rootPath))
            {
                DeleteSafe(rootPath);
            }
            Directory.CreateDirectory(rootPath);

            CreateBulkFolders(rootPath, count);

            return Ok(new { success = true, root = rootName });
        }

        private static void CreateBulkFolders(string basePath, int count)
        {
            const int batchSize = 500;
            int created = 0;

            while (created < count)
            {
                int upto = Math.Min(created + batchSize, count);
                var thisBatch = Enumerable.Range(created + 1, upto - created).Select(i => Path.Combine(basePath, $"Folder-{i}")).ToList();

                Parallel.ForEach(thisBatch, dir =>
                {
                    Directory.CreateDirectory(dir);
                });

                created = upto;
            }
        }

        private static void DeleteSafe(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                System.IO.File.SetAttributes(file, FileAttributes.Normal);
            }
            Directory.Delete(path, true);
        }

        private PhysicalFileProvider ResolveProvider()
        {
            var root = HttpContext.Request.Query["root"].ToString();

            var perfBase = Path.Combine(_basePath, _perfRootRelative);
            if (!Directory.Exists(perfBase))
                Directory.CreateDirectory(perfBase);

            if (string.IsNullOrWhiteSpace(root))
            {
                var provider = new PhysicalFileProvider();
                provider.RootFolder(perfBase);
                return provider;
            }

            string perfRoot = GetPerfRoot(root);
            if (!Directory.Exists(perfRoot))
            {
                var provider = new PhysicalFileProvider();
                provider.RootFolder(perfBase);
                return provider;
            }

            var ok = new PhysicalFileProvider();
            ok.RootFolder(perfRoot);
            return ok;
        }

        [HttpPost("FileOperations")]
        public object FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            var operation = ResolveProvider();
            if (args.Action == "delete" || args.Action == "rename")
            {
                if ((args.TargetPath == null) && (args.Path == ""))
                {
                    FileManagerResponse response = new FileManagerResponse();
                    response.Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." };
                    return operation.ToCamelCase(response);
                }
            }

            switch (args.Action)
            {
                case "read":
                    return operation.ToCamelCase(operation.GetFiles(args.Path, args.ShowHiddenItems));
                case "delete":
                    return operation.ToCamelCase(operation.Delete(args.Path, args.Names));
                case "copy":
                    return operation.ToCamelCase(operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "move":
                    return operation.ToCamelCase(operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "details":
                    return operation.ToCamelCase(operation.Details(args.Path, args.Names, args.Data));
                case "create":
                    return operation.ToCamelCase(operation.Create(args.Path, args.Name));
                case "search":
                    return operation.ToCamelCase(operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
                case "rename":
                    return operation.ToCamelCase(operation.Rename(args.Path, args.Name, args.NewName, false, args.ShowFileExtension, args.Data));
            }
            return null;
        }

        [HttpPost("Upload")]
        [DisableRequestSizeLimit]
        public IActionResult Upload(string path, long size, IList<IFormFile> uploadFiles, string action)
        {
            var operation = ResolveProvider();
            try
            {
                FileManagerResponse uploadResponse = operation.Upload(path, uploadFiles, action, size, null);
                if (uploadResponse.Error != null)
                {
                    Response.Clear();
                    Response.ContentType = "application/json; charset=utf-8";
                    Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
                }
            }
            catch (Exception)
            {
                ErrorDetails er = new ErrorDetails
                {
                    Message = "Access denied for Directory-traversal",
                    Code = "417"
                };
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(er.Code);
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = er.Message;
                return Content("");
            }
            return Content("");
        }

        [HttpPost("Download")]
        public IActionResult Download(string downloadInput)
        {
            var operation = ResolveProvider();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            FileManagerDirectoryContent args = JsonSerializer.Deserialize<FileManagerDirectoryContent>(downloadInput, options);
            return operation.Download(args.Path, args.Names, args.Data);
        }

        [HttpGet("GetImage")]
        public IActionResult GetImage(FileManagerDirectoryContent args)
        {
            var operation = ResolveProvider();
            return operation.GetImage(args.Path, args.Id, false, null, null);
        }
    }
}