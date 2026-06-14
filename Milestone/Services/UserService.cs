using Milestone.Data;
using Milestone.Interfaces;
using Milestone.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Milestone.Services {
    public class UserService : IUserManager
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        private static string HashPassword(string password, byte[] salt) =>
            Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32));

        public List<UserModel> GetAllUsers() =>
            _db.Users.ToList();

        public UserModel GetUserById(int id) =>
            _db.Users.Find(id);

        public int AddUser(UserModel user)
        {
            user.Salt = RandomNumberGenerator.GetBytes(16);
            user.PasswordHash = HashPassword(user.PasswordHash, user.Salt);
            _db.Users.Add(user);
            return _db.SaveChanges();
        }

        public void DeleteUser(UserModel user)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }

        public void UpdateUser(UserModel user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
        }

        public int CheckCredentials(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return 0;

            var hash = HashPassword(password, user.Salt);
            return hash == user.PasswordHash ? user.Id : 0;
        }
    }
}