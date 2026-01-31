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

        /// <summary> 
        /// Gets or sets a value that filters which content types are eligible for upload in the <see cref="SfFileManager{TValue}"/>. 
        /// </summary> 
        /// <value> 
        /// An <see cref="UploadContentFilter"/> specifying the allowed content types for upload. The default value is <see cref="UploadContentFilter.All"/>. 
        /// </value> 
        /// <remarks> 
        /// <para> 
        /// This property determines which items are allowed to be uploaded based on their type and your application's validation rules. Items that do not satisfy the configured filter are rejected before the upload begins. 
        /// </para> 
        /// <para>Typical usage scenarios include restricting uploads to files only, folders only by the application logic.</para> 
        /// <para>Allowed values include (implementation may vary):</para> 
        /// <list type="bullet"> 
        /// <item><description><see cref="UploadContentFilter.All"/> — No filtering; both files and folders are allowed.</description></item> 
        /// <item><description><see cref="UploadContentFilter.FilesOnly"/> — Only files are allowed.</description></item> 
        /// <item><description><see cref="UploadContentFilter.FoldersOnly"/> — Only folders are allowed.</description></item> 
        /// </list> 
        /// <para> 
        /// This property is not nullable. Changing the value affects subsequent uploads and does not retroactively validate already queued uploads. 
        /// </para> 
        /// </remarks> 
        /// <example> 
        /// <code><![CDATA[ 
        /// // Restrict uploads to files only 
        /// fileManager.UploadContentFilter = UploadContentFilter.FilesOnly; 
        /// 
        /// // Allow both files and folders (default) 
        /// fileManager.UploadContentFilter = UploadContentFilter.All; 
        /// ]]></code> 
        /// </example> 
        public UploadContentFilter UploadContentFilter { get; set; } = UploadContentFilter.All;
    }
}