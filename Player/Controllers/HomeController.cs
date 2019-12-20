using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Player.Models;
using AppContext = Player.Models.AppContext;

namespace Player.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private AppContext _db;
        private UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, AppContext db, UserManager<User> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Download()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(null);
            }
            var audio = _db.Audio?.Where(file => file.AuthorId.Equals(user.Id)).ToList();
            if (audio != null) { 
                return View(_db.Audio?.Where(file => file.AuthorId.Equals(user.Id)).ToList());
            }
            return View(null);
        }


        public IActionResult Play(Guid id)
        {
            return View(_db.Audio.Where(file => file.Id.Equals(id))?.FirstOrDefault());
        }

        [HttpPost]
        [RequestSizeLimit(40000000)] 
        public async Task<IActionResult> UploadFile(IFormFile uploadFile)
        {
            if (uploadFile != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (!Directory.Exists(Path.Combine("wwwroot", "Files", $"{user.Id}")))
                {
                    // Create the directory.
                    Directory.CreateDirectory(Path.Combine("wwwroot", "Files", $"{user.Id}"));
                }
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("wwwroot", "Files", $"{user.Id}"), uploadFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadFile.CopyToAsync(fileStream);
                }
                
                Audio file = new Audio { Name = uploadFile.FileName, Path = Path.Combine("Files", $"{user.Id}", $"{uploadFile.FileName}"), AuthorId = user.Id, Id = new Guid()};
                _db.Audio.Add(file);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id != null)
            {
                Audio audio = await _db.Audio.FirstOrDefaultAsync(p => p.Id.Equals(id));
                if (audio != null)
                {
                    _db.Audio.Remove(audio);
                    await _db.SaveChangesAsync();
                    System.IO.File.Delete(Path.Combine("wwwroot", audio.Path));
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
