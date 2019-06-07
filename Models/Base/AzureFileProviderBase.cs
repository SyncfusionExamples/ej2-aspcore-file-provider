using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.EJ2.FileManager.Base;

#if EJ2_DNX
using System.Web.Mvc;
using System.IO.Packaging;
using System.Web;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
#endif

namespace Syncfusion.EJ2.FileManager.Base
{
    public  interface AzureFileProviderBase : FileProviderBase
    {        
            void RegisterAzure(string accountName, string accountKey, string blobName);
        }
    
}
