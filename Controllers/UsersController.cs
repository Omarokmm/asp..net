using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers {

    [ServiceFilter (typeof (LogUserActivaty))]
    [Authorize]
    [Route ("api/[Controller]")]
    public class UsersController : ControllerBase {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController (IDatingRepository repo, IMapper mapper) {

            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser ([FromQuery] UserParams userParams) {
            var currentUserId = int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser (currentUserId);
            userParams.userId = currentUserId;

            if (string.IsNullOrEmpty (userParams.Gender)) {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }
            var users = await _repo.GetUsers (userParams);
            var userToReturn = _mapper.Map<IEnumerable<UserForListDto>> (users);

            Response.AddPagination (users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok (userToReturn);
        }

        [HttpGet ("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser (int id) {

            var user = await _repo.GetUser (id);
            var userToReturn = _mapper.Map<UserForDetailedDto> (user);
            return Ok (userToReturn);
        }

        [HttpPut ("{id}")]
        public async Task<IActionResult> UpdateUser (int id, [FromBody] UserForUpdate userForUpdate) {

            if (id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();

            var userFromRepo = await _repo.GetUser (id);
            _mapper.Map<UserForUpdate, User> (userForUpdate, userFromRepo);

            if (await _repo.SaveAll ())
                return NoContent ();

            throw new Exception ($"Updating user {id} Falied on Saved");

            // [HttpPut ("{id}")]
            // public async Task<IActionResult> UpdateUser (int id, UserForUpdateDto userForUpdate) {

            //     if (id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
            //         return Unauthorized ();
            //     }

            //     var userFromRepo = await _repo.GetUser (id);
            //      var user=_mapper.Map(userForUpdate,userFromRepo);

            //     if (await _repo.SaveAll ())
            //         return NoContent ();

            //     //throw new Exception ($"Updating user {id} Falied on Saved");
            //     return Ok(userFromRepo);
            // }
        }

        [HttpPost ("{id}/like/{reciptionId}")]
        public async Task<IActionResult> AddLike (int id, int reciptionId) {

            if (id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();

            var getLike = await _repo.GetLike (id, reciptionId);

            if (getLike != null)
                return BadRequest ("the user is already Liked ");

            if (await _repo.GetUser (reciptionId) == null)
                return NotFound ();

            var like = new Like {
                LikerId = id,
                LikeeId = reciptionId
            };

            _repo.Add<Like> (like);
            if (await _repo.SaveAll ()) {
                return Ok ();
            }

            return BadRequest("Falied To Like User ");
        }

    }
}