using System;

namespace DatingApp.API.Models {
    public class Message {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int ReciptionId { get; set; }
        public User Reciption { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessagesSent { get; set; }
        public bool SenderDeleted { get; set; }
        public bool ReciptionDeleted { get; set; }
    }
}