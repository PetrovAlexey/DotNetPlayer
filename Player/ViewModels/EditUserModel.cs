using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Player.ViewModels
{
    public class CreateUserModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int Year { get; set; }
    }
    public class EditUserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public int Year { get; set; }
    }
}
