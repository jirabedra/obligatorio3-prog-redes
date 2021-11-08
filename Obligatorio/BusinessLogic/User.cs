using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic
{
    public class User
    {
        private static int _nextId = 1;
        public int Id { get; set; }

        public List<Game> Games { get; set; }

        public User(List<Game> games)
        {
            Id = _nextId;
            _nextId++;
            Games = games;
        }

        public User(int id, List<Game> games)
        {
            Id = id;
            Games = games;
        }


        public void AddGame(Game gameToAdd)
        {
            Games.Add(gameToAdd);
        }

        public override bool Equals(object obj)
        {
            var user = obj as User;
            if(user == null)
            {
                return false;
            }
            else
            {
                return this.Id.Equals(user.Id);
            }
        }
    }
}
