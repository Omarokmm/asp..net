using System;

namespace DatingApp.API.Dtos {
    public class MessageFromCreationDto {
        public int SenderId { get; set; }
        public int reciptionId { get; set; }
        public DateTime MessagesSent { get; set; }

        public string Content { get; set; }

        public MessageFromCreationDto () {
            MessagesSent = DateTime.Now;
        }
    }
}