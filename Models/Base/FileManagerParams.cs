using System;
using System.Collections.Generic;
using System.Linq;
#if EJ2_DNX
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Syncfusion.EJ2.FileManager.Base
{
    public class FileManagerParams
    {
        public string Name { get; set; }

        public string[] Names { get; set; }

        public string Path { get; set; }

        public string TargetPath { get; set; }

        public string NewName { get; set; }

        public object Date { get; set; }
#if EJ2_DNX
        public IEnumerable<System.Web.HttpPostedFileBase> FileUpload { get; set; }
#else
        public IEnumerable<IFormFile> FileUpload { get; set; }

#endif
        public string[] ReplacedItemNames { get; set; }
    }
}