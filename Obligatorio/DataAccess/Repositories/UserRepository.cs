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
            if (!this.Users.Contains(user))
            {
                this.Users.Add(user);
                Console.WriteLine($"User Id {user.Id} has been registered.");
            }
        }

        public void listAllUsers()
        {
            Console.WriteLine("Registered users are: ");
            int userPos = 0;
            foreach (var user in this.Users)
            {
                Console.WriteLine($"{++userPos}. {user.Id}");
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
