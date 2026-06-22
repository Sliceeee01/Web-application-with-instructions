using System;

namespace InstructiionsApp.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        // Внешний ключ для связи с таблицей Instructions
        public int InstructionId { get; set; }
        
        // Навигационное свойство
        public Instruction Instruction { get; set; }

        public string FileName { get; set; }
        
        public string FilePath { get; set; }
        
        public string FileType { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}