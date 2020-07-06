using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
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
using TagLib;

namespace Player.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserContext _db;
        private UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, UserContext db, UserManager<User> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Download()
        {
            return View();
        }

        public async Task<IActionResult> MyAudio()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(null);
            }
            var result = from audios in _db.Audios
                join useraudio in _db.UserAudios on audios.AudioId equals useraudio.AudioId
                         where useraudio.Id == user.Id
                select new Audio
                {
                    Name = audios.Name,
                    Path = audios.Path,
                    AudioId = audios.AudioId,
                    AuthorId = audios.AuthorId,
                    Label = audios.Label,
                    Song = audios.Song
                };

                return View(result);
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(null);
            }

            var audios = _db.Audios.OrderByDescending(p => p.Users.Count()).ToList();
            var result = from a in _db.UserAudios
                group a by a.AudioId
                into g
                select new
                {
                    name = g.Key,
                    count = g.Count()
                };
            ViewData["dict"] = result.ToDictionary(v => v.name, v => v.count);

            return View(audios);
        }


        public IActionResult Play(Guid id)
        {
            return View(_db.Audios.Where(file => file.AudioId.Equals(id))?.FirstOrDefault());
        }

        public FileResult GetFileFromBytes(byte[] bytesIn)
        {
            return File(bytesIn, "image/png");
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

                var audioFile = TagLib.File.Create(filePath);
                TagLib.IPicture pic = null;
                if (audioFile.Tag.Pictures.Length >= 1)
                {
                    pic = audioFile.Tag.Pictures[0];  //pic contains data for image.
                }

                if (pic != null)
                {
                    using (var stream = new MemoryStream(pic.Data.Data))
                    {
                        Audio file = new Audio
                        {
                            Name = uploadFile.FileName,
                            Path = Path.Combine("Files", $"{user.Id}", $"{uploadFile.FileName}"),
                            AudioId = new Guid(),
                            AuthorId = user.Id,
                            Label = stream.ToArray(),
                            Song = audioFile.Tag.Album
                        };

                        var userAudio = new UserAudio
                        {
                            User = user,
                            Audio = file,
                            Status = Status.Added
                        };

                        _db.UserAudios.Add(userAudio);
                        _db.Audios.Add(file);
                        _db.SaveChanges();
                    }
                }
                else
                {
                    Audio file = new Audio
                    {
                        Name = uploadFile.FileName,
                        Path = Path.Combine("Files", $"{user.Id}", $"{uploadFile.FileName}"),
                        AudioId = new Guid(),
                        AuthorId = user.Id,
                        Label = null,
                        Song = audioFile.Tag.Album
                    };

                    var userAudio = new UserAudio
                    {
                        User = user,
                        Audio = file,
                        Status = Status.Added
                    };

                    _db.UserAudios.Add(userAudio);
                    _db.Audios.Add(file);
                    _db.SaveChanges();
                }
                

                
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id != null)
            {
                var user = await _userManager.GetUserAsync(User);
                Audio audio = null;
                if (await _userManager.IsInRoleAsync(user, "admin"))
                { 
                    audio = await _db.Audios.FirstOrDefaultAsync(p => p.AudioId.Equals(id));
                }
                else
                {
                    audio = await _db.Audios.Where(p => p.AuthorId.Equals(user.Id)).FirstOrDefaultAsync(p => p.AudioId.Equals(id));
                }
                
                if (audio != null)
                {
                    _db.Audios.Remove(audio);
                    await _db.SaveChangesAsync();
                    System.IO.File.Delete(Path.Combine("wwwroot", audio.Path));
                    return RedirectToAction("Index");
                }

                return Forbid();
            }
            return NotFound();
        }


        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add(Guid? id)
        {
            
            var user = await _userManager.GetUserAsync(User);
            var currentFile = await _db.UserAudios.Where(p => p.Id.Equals(user.Id)).FirstOrDefaultAsync(p => p.AudioId.Equals(id));
            if (currentFile != null)
            {
                return RedirectToAction("MyAudio");
            }
            var file = _db.Audios.FirstOrDefault(p => p.AudioId.Equals(id));

            if (file == null)
            {
                return NotFound();
            }
            var userAudio = new UserAudio
            {
                User = user,
                Audio = file,
                Status = Status.Added
            };

            _db.UserAudios.Add(userAudio);
            _db.SaveChanges(); 

            return RedirectToAction("MyAudio");
        }

        [HttpPost]
        [ActionName("Remove")]
        public async Task<IActionResult> Remove(Guid? id)
        {

            var user = await _userManager.GetUserAsync(User);
            var file = await _db.UserAudios.Where(p => p.Id.Equals(user.Id)).FirstOrDefaultAsync(p => p.AudioId.Equals(id));
            if (file == null)
            {
                return NotFound();
            }

            _db.UserAudios.Remove(file);
            _db.SaveChanges();

            return RedirectToAction("MyAudio");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
