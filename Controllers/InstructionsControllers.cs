using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InstructiionsApp.Models;

namespace InstructiionsApp.Controllers
{
    public class InstructionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        public InstructionsController(AppDbContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, string searchCategory)
        {
            var instructions = from i in _context.Instructions select i;
            if (!string.IsNullOrEmpty(searchString))
                instructions = instructions.Where(s => s.Title.Contains(searchString) || s.Content.Contains(searchString));
            if (!string.IsNullOrEmpty(searchCategory))
                instructions = instructions.Where(x => x.Category == searchCategory);
            return View(await instructions.OrderByDescending(i => i.CreatedAt).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instruction instruction, List<IFormFile> uploadedFiles)
        {
            if (ModelState.IsValid)
            {
                instruction.CreatedAt = DateTime.Now;
                instruction.UpdatedAt = DateTime.Now;
                _context.Add(instruction);
                await _context.SaveChangesAsync();
                if (uploadedFiles != null && uploadedFiles.Count > 0)
                {
                    string folderPath = Path.Combine(_appEnvironment.WebRootPath, "uploads", instruction.Id.ToString());
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    foreach (var file in uploadedFiles)
                    {
                        if (file.Length > 0)
                        {
                            string safeFileName = Path.GetFileName(file.FileName);
                            string filePath = Path.Combine(folderPath, safeFileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            _context.Attachments.Add(new Attachment { InstructionId = instruction.Id, FileName = safeFileName, FilePath = $"/uploads/{instruction.Id}/{safeFileName}", FileType = file.ContentType, UploadedAt = DateTime.Now });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(instruction);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var instruction = await _context.Instructions.Include(i => i.Attachments).FirstOrDefaultAsync(m => m.Id == id);
            if (instruction == null) return NotFound();
            return View(instruction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var instruction = await _context.Instructions.FindAsync(id);
            if (instruction == null) return NotFound();
            return View(instruction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Instruction instruction)
        {
            if (id != instruction.Id) return NotFound();
            if (ModelState.IsValid)
            {
                instruction.UpdatedAt = DateTime.Now;
                _context.Update(instruction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(instruction);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var instruction = await _context.Instructions.FirstOrDefaultAsync(m => m.Id == id);
            if (instruction == null) return NotFound();
            return View(instruction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instruction = await _context.Instructions.FindAsync(id);
            if (instruction != null)
            {
                _context.Instructions.Remove(instruction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportToJson(int id)
        {
            var instruction = await _context.Instructions.Include(i => i.Attachments).FirstOrDefaultAsync(m => m.Id == id);
            if (instruction == null) return NotFound();

            var package = new ExportPackage { Title = instruction.Title, Category = instruction.Category, Content = instruction.Content };

            foreach (var attach in instruction.Attachments)
            {
                string relativePath = attach.FilePath.TrimStart('/');
                string fullPath = Path.Combine(_appEnvironment.WebRootPath, relativePath);
                if (System.IO.File.Exists(fullPath))
                {
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                    package.Attachments.Add(new FileItem { FileName = attach.FileName, Base64Data = Convert.ToBase64String(fileBytes) });
                }
            }
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(package, new JsonSerializerOptions { WriteIndented = true });
            return File(jsonBytes, "application/json", $"{instruction.Title.Replace(" ", "_")}.json");
        }
    }
}