using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Player.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public IEnumerable<UserAudio> Audios { get; set; }
    }
}
