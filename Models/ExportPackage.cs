namespace InstructiionsApp.Models
{
    public class ExportPackage
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }
        public List<FileItem> Attachments { get; set; } = new List<FileItem>();
    }

    public class FileItem
    {
        public string FileName { get; set; }
        public string Base64Data { get; set; } 
    }
}