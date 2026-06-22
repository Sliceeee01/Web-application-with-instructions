using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InstructiionsApp.Models
{
    public class Instruction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название инструкции")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        public string Category { get; set; }

        [Display(Name = "Краткое описание")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Текст инструкции не может быть пустым")]
        [Display(Name = "Текст инструкции")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Связь "Один ко многим": у одной инструкции может быть много прикрепленных файлов
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
