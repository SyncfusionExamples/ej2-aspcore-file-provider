using System;
using System.Collections.Generic;
using System.Linq;
#if EJ2_DNX
using System.Web;
#endif

namespace Syncfusion.EJ2.FileManager.Base
{
    public class AccessDetails
    {
        public string Role { get; set; }
        public IEnumerable<AccessRule> AccessRules { get; set; }
    }

    /// <summary> 
    /// Specifies the content categories that are eligible for upload in the <see cref="SfFileManager{TValue}"/> and related components. 
    /// </summary> 
    /// <remarks> 
    /// <para> 
    /// Use this enumeration to control which items (files, folders, or both) the application accepts during upload operations. 
    /// This filter is typically applied by properties such as <c>UploadContentFilter</c> to validate user selections before an upload begins. 
    /// </para> 
    /// <list type="bullet"> 
    /// <item><description><see cref="All"/> <20> No filtering is applied; both files and folders are allowed.</description></item> 
    /// <item><description><see cref="FilesOnly"/> <20> Only file items are allowed; folders are rejected.</description></item> 
    /// <item><description><see cref="FoldersOnly"/> <20> Only folders are allowed; files are rejected.</description></item> 
    /// </list> 
    /// </remarks> 
    /// <example> 
    /// <code><![CDATA[ 
    /// // Allow only files to be uploaded 
    /// fileManager.UploadContentFilter = UploadContentFilter.FilesOnly; 
    /// 
    /// // Allow both files and folders (default in many scenarios) 
    /// fileManager.UploadContentFilter = UploadContentFilter.All; 
    /// ]]></code> 
    /// </example> 
    public enum UploadContentFilter
    {
        /// <summary> 
        /// Allows both files and folders to be uploaded without restriction. 
        /// </summary> 
        /// <remarks> 
        /// Use this value when the application must accept any item type during upload. 
        /// </remarks> 
        All = 0,
        /// <summary> 
        /// Restricts uploads to files only; folder items are not permitted. 
        /// </summary> 
        /// <remarks> 
        /// Use this value when the upload workflow or storage target only supports file items (for example, document-only 
        /// libraries or when folder creation is disabled). 
        /// </remarks> 
        FilesOnly,
        /// <summary> 
        /// Restricts uploads to folders only; file items are not permitted. 
        /// </summary> 
        /// <remarks> 
        /// Use this value when users are expected to upload folder structures (for example, bulk directory transfers) and 
        /// individual files should be disallowed in the current context. 
        /// </remarks> 
        FoldersOnly
    }

    public class AccessRule
    {
        public Permission Copy { get; set; }
        public Permission Download { get; set; }
        public Permission Write { get; set; }
        public string Path { get; set; }
        public Permission Read { get; set; }
        public string Role { get; set; }
        public Permission WriteContents { get; set; }
        public Permission Upload { get; set; }
        public bool IsFile { get; set; }
        public string Message { get; set; }
    }
    public enum Permission
    {
        Allow,
        Deny
    }
}