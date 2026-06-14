using Milestone.Models;

namespace Milestone.Interfaces {
    public interface IUserManager
    {
        public List<UserModel> GetAllUsers();
        public UserModel GetUserById(int id);
        public int AddUser(UserModel user);
        public void DeleteUser(UserModel user);
        public void UpdateUser(UserModel user);
        public int CheckCredentials(string username, string password);
    }
}