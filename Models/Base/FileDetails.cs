using System;
using System.Collections.Generic;
using System.Linq;


namespace Syncfusion.EJ2.FileManager.Base
{
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
}