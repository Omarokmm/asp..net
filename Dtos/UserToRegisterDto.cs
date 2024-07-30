using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos {
    public class UserToRegisterDto {

        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength (8, MinimumLength = 4, ErrorMessage = "you must specify password between 4 and  8 ")]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string KnownAs { get; set; }

        [Required]
        public DateTime DateOfBirdth { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }

        public UserToRegisterDto () {
            LastActive = DateTime.Now;
            Created = DateTime.Now;

        } 

    }
}