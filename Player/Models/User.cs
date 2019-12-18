using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Player.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public int Year { get; set; }
    }
}
