using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.EJ2.FileManager.Base
{
    public class AccessDetails
    {
        public string Role { get; set; }
        public IEnumerable<FileRule> FileRules { get; set; }
        public IEnumerable<FolderRule> FolderRules { get; set; }
    }
    public class FileRule
    {
        public Permission Copy { get; set; }
        public Permission Download { get; set; }
        public Permission Edit { get; set; }
        public string Path { get; set; }
        public Permission Read { get; set; }
        public string Role { get; set; }
    }
    public class FolderRule
    {
        public Permission Copy { get; set; }
        public Permission Download { get; set; }
        public Permission Edit { get; set; }
        public Permission EditContents { get; set; }
        public string Path { get; set; }
        public Permission Read { get; set; }
        public string Role { get; set; }
        public Permission Upload { get; set; }
    }
    public enum Permission
    {
        Allow,
        Deny
    }

}
