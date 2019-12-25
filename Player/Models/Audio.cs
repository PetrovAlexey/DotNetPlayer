using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Player.Models
{
    public class Audio
    {
        public Guid AudioId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string AuthorId { get; set; }
        public byte[] Label { get; set; }
        public string Song { get; set; }
        public IEnumerable<UserAudio> Users { get; set; }
    }
}
