using System;
using System.Collections;
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
    [Route ("api/users/{userId}/[Controller]")]
    [ApiController]

    public class MessagesController : ControllerBase {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController (IDatingRepository Repo, IMapper mapper) {
            _repo = Repo;
            _mapper = mapper;
        }

        [HttpGet ("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage (int userId, int id) {

            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();

            var messageFromRepo = await _repo.GetMessage (id);

            if (messageFromRepo == null)
                return NotFound ();
            return Ok (messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages (int userId, [FromQuery] MessageParams messageParams) {

            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();
            messageParams.userId = userId;

            var messageFromRepo = await _repo.GetMessagesForUser (messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>> (messageFromRepo);

            Response.AddPagination (messageFromRepo.CurrentPage, messageFromRepo.PageSize,
                messageFromRepo.TotalCount, messageFromRepo.TotalPages);

            return Ok (messages);

        }

        [HttpGet ("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread (int userId, int recipientId) {

            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();

            var messageFromRepo = await _repo.GetMessageThread (userId, recipientId);
            var threadMessage = _mapper.Map<IEnumerable<MessageToReturnDto>> (messageFromRepo);

            return Ok (threadMessage);
        }

        [HttpPost]
        public async Task<IActionResult> GreateMessage (int userId, [FromBody] MessageFromCreationDto messageFromCreationDto) {

            var sender = await _repo.GetUser (userId);

            if (sender.Id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();

            messageFromCreationDto.SenderId = userId;
            var recipientId = await _repo.GetUser (messageFromCreationDto.reciptionId);
            if (recipientId == null)
                return BadRequest ("Could Not Found User");

            var message = _mapper.Map<Message> (messageFromCreationDto);

            _repo.Add (message);

            if (await _repo.SaveAll ()) {
                var messageToReturn = _mapper.Map<MessageToReturnDto> (message);
                return CreatedAtRoute ("GetMessage", new { id = message.Id }, messageToReturn);
            }

            throw new Exception ("Creation The Message Falied ");

        }

        [HttpPost ("{id}")]
        public async Task<IActionResult> DeleteMessage (int id, int userId) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();

            var messageFromRepo = await _repo.GetMessage (id);

            if (messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;

            if (messageFromRepo.ReciptionId == userId)
                messageFromRepo.ReciptionDeleted = true;

            if (messageFromRepo.SenderDeleted && messageFromRepo.ReciptionDeleted)
                _repo.Delete (messageFromRepo);

            if (await _repo.SaveAll ())
                return NoContent ();
            throw new Exception ("Error Deleting The Message");

        }

        [HttpPost ("{id}/read")]

        public async Task<IActionResult> MarkAsRead (int userId, int id) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value))
                return Unauthorized ();
            var message = await _repo.GetMessage (id);
            if(message == null)
            return NotFound("message Not Found");
            if (message.ReciptionId != userId)
                return Unauthorized ();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll ();
            return NoContent ();

        }

    }
}