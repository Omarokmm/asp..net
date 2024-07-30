using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [Route ("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController (IAuthRepository repo, IConfiguration config, IMapper mapper) {
            _mapper = mapper;
            _config = config;
            _repo = repo;

        }

        [HttpPost ("register")]
        public async Task<IActionResult> Rigister (UserToRegisterDto userToRegisterDto) {

            // if(!ModelState.IsValid)
            // return BadRequest(ModelState);
            userToRegisterDto.Username = userToRegisterDto.Username.ToLower ();

            if (await _repo.UserExists (userToRegisterDto.Username))
                return BadRequest ("Username already exists ");

            var userToCreat = _mapper.Map<User> (userToRegisterDto);

            var createdUser = await _repo.Rigister (userToCreat, userToRegisterDto.Password);
            var userToReturn = _mapper.Map<UserForDetailedDto> (createdUser);

            return CreatedAtRoute ("GetUser", new { controller = "Users", id = createdUser.Id }, userToReturn);

        }

        [HttpPost ("Login")]

        public async Task<IActionResult> Login (UserForLoginDtos userForLogin) {

            var userForRepo = await _repo.Login (userForLogin.Username.ToLower (), userForLogin.Password);

            if (userForRepo == null)
                return Unauthorized ();

            //create claims from IdUser And NameUser 
            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, userForRepo.Id.ToString ()),
                new Claim (ClaimTypes.Name, userForRepo.Username)
            };

            //Generate Key For create credencials 
            var key = new SymmetricSecurityKey (Encoding.UTF8
                .GetBytes (_config.GetSection ("AppSettings:Token").Value));

            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);

            // create description gor tokens 
            var tokenDescriptor = new SecurityTokenDescriptor {

                Subject = new ClaimsIdentity (claims),
                Expires = DateTime.Now.AddDays (1),
                SigningCredentials = creds

            };
            //create the token for the  client
            var tokenHandler = new JwtSecurityTokenHandler ();
            var token = tokenHandler.CreateToken (tokenDescriptor);
            var user = _mapper.Map<UserForListDto> (userForRepo);
            return Ok (
                new {

                    token = tokenHandler.WriteToken (token),
                        user
                }

            );

        }

    }
}