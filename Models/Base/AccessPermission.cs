using System;
using System.Collections.Generic;
using System.Linq;

namespace Syncfusion.EJ2.FileManager.Base
{
    public class AccessPermission
    {
        public bool Copy { get; set; } = true;
        public bool Download { get; set; } = true;
        public bool Write { get; set; } = true;
        public bool WriteContents { get; set; } = true;
        public bool Read { get; set; } = true;
        public bool Upload { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}