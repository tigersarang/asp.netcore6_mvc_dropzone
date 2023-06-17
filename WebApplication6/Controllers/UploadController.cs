using Microsoft.AspNetCore.Mvc;
using WebApplication6.Dto;

namespace WebApplication6.Controllers
{
    [Route("[controller]/[action]")]
    public class UploadController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return BadRequest(new { code = -1, message = "파일이 없습니다." });
                }

                string newName = Guid.NewGuid().ToString() + "___" + file.FileName;
                string upDir = Path.Combine(Directory.GetCurrentDirectory(), GV.I.TD, newName);

                using (FileStream fs = new FileStream(upDir, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }
                return Ok(new { newName = upDir });
            } catch (Exception ex)
            {
                return BadRequest(new { code = -2, message = ex.Message });
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
                return Ok();
            }

            return NotFound();
        }
    }
}
