using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication6.Dto;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            List<Board> boards = _dbContext.Boards.Include(b => b.BoardFiles).ToList();
            return View(boards);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string message)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = message});
        }

        [HttpPost]
        public async Task<IActionResult> SaveForm([FromForm] FormDto filePaths)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {

                try
                {
                    if (filePaths == null)
                    {
                        return BadRequest(new { code = -1, message = "파일이 없습니다." });
                    }

                    Board board = new Board
                    {
                        Title = filePaths.Title
                    };
                    _dbContext.Add(board);


                    string upDir = Path.Combine(Directory.GetCurrentDirectory(), GV.I.TD, GV.I.UD);

                    if (Directory.Exists(upDir) == false)
                    {
                        Directory.CreateDirectory(upDir);
                    }

                    List<BoardFile> boardFiles = new List<BoardFile>();
                    foreach (var filePath in filePaths.files)
                    {
                        string upFileName = Path.Combine(upDir, Path.GetFileName(filePath));
                        System.IO.File.Move(filePath, upFileName);
                        boardFiles.Add(
                            new BoardFile
                            {
                                FileName = Path.GetFileName(filePath).Split("___")[1],
                                LinkFileName = upFileName
                            }
                        );
                    }

                    board.BoardFiles = boardFiles;
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Ok(new { newName = upDir });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { code = -2, message = ex.Message });
                }
            }

        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new Exception("파일명이 없습니다.");
                }

                //string upDir = Path.Combine(Directory.GetCurrentDirectory(), GV.I.TD, GV.I.UD, fileName);

                if (System.IO.File.Exists(fileName) == false)
                {
                    throw new Exception("파일이 없습니다.");
                }

                MemoryStream memory = new MemoryStream();
                using (var stream = new FileStream(fileName, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                string mimeType = GetContentType(fileName);

                if (string.IsNullOrEmpty(mimeType))
                {
                    throw new Exception("지원되지 않는 확장자입니다.");
                }

                memory.Position = 0;
                return File(memory, mimeType, Path.GetFileName(fileName).Split("__")[1]);
            } catch (Exception ex)
            {
                return RedirectToAction("Error","Home", new { message = ex.Message });
            }
        }

        private string GetContentType(string fileName)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (types.ContainsKey(ext) == false)
            {
                return null;
            }
            return types[ext]; 
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".jpg", "image/jpeg"},
                {".png", "image/png"},
                {".gif", "image/gif"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".csv", "text/csv"},
                {".ppt", "application/vnd.ms-powerpoint"},
                {".pptx", "application/vnd.openxmlformatsofficedocument.presentationml.presentation"},
                {".zip", "application/zip"},
                {".7z", "application/x-7z-compressed"},
                {".rar", "application/x-rar-compressed"}
            };
        }
    }
}