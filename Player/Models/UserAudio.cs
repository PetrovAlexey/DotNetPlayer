using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Player.Models
{
    public enum Status
    {
        Added = 1,
        Liked = 2,
        LikeAdd = 3,
        Owner = 4,
        Default = 0
    }
    public class UserAudio
    {
        public string Id { get; set; }
        public Guid AudioId { get; set; }
        public User User { get; set; }
        public Audio Audio { get; set; }
        public Status Status { get; set; }
    }
}