using System;
using System.Collections.Generic;
using System.Linq;

namespace Syncfusion.EJ2.FileManager.Base
{
    public class AccessPermission
    {
        /// <summary>
        /// Gets or sets access to copy a file or folder.
        /// </summary>
        public bool Copy { get; set; } = true;

        /// <summary>
        /// Gets or sets permission to download a file or folder.
        /// </summary>
        public bool Download { get; set; } = true;

        /// <summary>
        /// Gets or sets permission to write a file or folder.
        /// </summary>
        public bool Write { get; set; } = true;

        /// <summary>
        /// Gets or sets permission to write the content of folder.
        /// </summary>
        public bool WriteContents { get; set; } = true;

        /// <summary>
        /// Gets or sets access to read a file or folder.
        /// </summary>
        public bool Read { get; set; } = true;

        /// <summary>
        /// Gets or sets permission to upload to the folder.
        /// </summary>
        public bool Upload { get; set; } = true;

        /// <summary>
        /// Gets or sets the access message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}