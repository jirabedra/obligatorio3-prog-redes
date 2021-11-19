using BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    public class UserRepository
    {
        public List<User> Users { get; private set; }

        public UserRepository()
        {
            this.Users = new List<User>();
        }

        public void addUser(User user)
        {
                Users.Add(user);
                Console.WriteLine($"User Id {user.Id} has been registered.");
        }

        public void listAllUsers()
        {
            Console.WriteLine($"There are {Users.Count} users registered. Registered users are: ");
            foreach (var user in Users)
            {
                Console.WriteLine($"User: {user.Id}, Nickname: {user.Nickname}");
            }
        }

        public void DeleteGame(string gameToDelete)
        {
            Game g = new Game { Title = gameToDelete };
            foreach (var user in Users)
            {
                if (user.Games.Contains(g)) 
                {
                    user.Games.RemoveAll(game => g.Equals(game));
                }
            }
        }

        public void DeleteUser(User user)
        {
            Users.Remove(user);
        }
    }
}
