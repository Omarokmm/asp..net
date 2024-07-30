using System;

namespace DatingApp.API.Dtos {
    public class MessageToReturnDto {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderKnownAs { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int ReciptionId { get; set; }
        public string ReciptionKnownAs { get; set; }
        public string ReciptionPhotoUrl { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessagesSent { get; set; }
    }
}