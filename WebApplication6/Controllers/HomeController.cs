using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication6.Dto;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
    }
}