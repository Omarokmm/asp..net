using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data {
    public class DatingRepository : IDatingRepository {
        private readonly DataContext _context;

        public DatingRepository (DataContext context) {
            _context = context;
        }
        public void Add<T> (T entity) where T : class {
            _context.Add (entity);
        }

        public void Delete<T> (T entity) where T : class {
            _context.Remove (entity);
        }

        public async Task<Like> GetLike (int userId, int reciptionId) {

            return await _context.Likes.FirstOrDefaultAsync (u => u.LikerId == userId && u.LikeeId == reciptionId);

        }

        public async Task<Photo> GetMainPhotoForUser (int userId) {
            return await _context.Photos.Where (p => p.UserId == userId).FirstOrDefaultAsync (u => u.IsMain);
        }

        public async Task<Photo> GetPhoto (int id) {
            var photo = await _context.Photos.FirstOrDefaultAsync (d => d.Id == id);

            return photo;

        }

        public async Task<User> GetUser (int id) {
            var user = await _context.Users.Include (p => p.Photos).FirstOrDefaultAsync (u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers (UserParams userParams) {

            var users = _context.Users.Include (p => p.Photos)
                .OrderByDescending (u => u.LastActive).AsQueryable ();

            users = users.Where (u => u.Id != userParams.userId);
            users = users.Where (u => u.Gender == userParams.Gender);

            if (userParams.Likers) {
                var userLikers = await GetUserLikes (userParams.userId, userParams.Likers);
                users = users.Where (u => userLikers.Contains (u.Id));
            }
            if (userParams.Likees) {
                var userLikees = await GetUserLikes (userParams.userId, userParams.Likers);
                users = users.Where (u => userLikees.Contains (u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99) {

                var MinDob = DateTime.Today.AddYears (-userParams.MaxAge - 1);
                var MaxDob = DateTime.Today.AddYears (-userParams.MinAge - 1);
                users = users.Where (u => u.DateOfBirdth >= MinDob && u.DateOfBirdth <= MaxDob);
            }

            if (!string.IsNullOrEmpty (userParams.orderBy)) {
                switch (userParams.orderBy) {
                    case "created":
                        users = users.OrderByDescending (u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending (u => u.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync (users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes (int id, bool likers) {

            var userLikes = await _context.Users.
            Include (x => x.Likers).
            Include (x => x.Likees).
            FirstOrDefaultAsync (u => u.Id == id);

            if (likers) {
                return userLikes.Likers.Where (u => u.LikeeId == id).Select (i => i.LikerId);
            } else {
                return userLikes.Likees.Where (u => u.LikerId == id).Select (i => i.LikeeId);
            }

        }

        public async Task<bool> SaveAll () {

            return await _context.SaveChangesAsync () > 0;
        }

        public async Task<Message> GetMessage (int id) {
            return await _context.Messages.FirstOrDefaultAsync (m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser (MessageParams messageParams) {

            var messages = _context.Messages.Include (u => u.Sender)
                .ThenInclude (p => p.Photos).Include (u => u.Reciption)
                .ThenInclude (p => p.Photos).AsQueryable ();

            switch (messageParams.MessageContainer) {
                case "Inbox":
                    messages = messages.Where (u => u.ReciptionId == messageParams.userId && u.ReciptionDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where (u => u.SenderId == messageParams.userId && u.SenderDeleted==false );
                    break;
                default:
                    messages = messages.Where (u => u.ReciptionId == messageParams.userId 
                    && u.ReciptionDeleted ==false && u.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending (p => p.MessagesSent);

            return await PagedList<Message>.CreateAsync (messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread (int userId, int recipientId) {

            var messages = await _context.Messages.Include (u => u.Sender)
                .ThenInclude (p => p.Photos).Include (u => u.Reciption)
                .ThenInclude (p => p.Photos)
                .Where (m => m.ReciptionId == userId && m.ReciptionDeleted == false 
                    &&  m.SenderId == recipientId ||
                    m.SenderDeleted == false &&
                    m.SenderId == userId && m.ReciptionId == recipientId)
                .OrderByDescending (m => m.MessagesSent).ToListAsync ();

            return messages;
        }
    }
}