using BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    public class GameRepository
    {
        public List<Game> Games {get; private set;}

        public GameRepository()
        {
            this.Games = new List<Game>();
        }
        public bool addGame(Game game)
        {
            if (!this.Games.Contains(game))
            {
                this.Games.Add(game);
                return true;
            }
            return false;
        }

        public void listAllGames()
        {
            Console.WriteLine("Registered games are: ");
            int gamePos = 0;
            foreach (var game in this.Games)
            {
                Console.WriteLine($"{++gamePos}. {game.Title}");
            }
        }

        public bool IsRegistered(string titleToCompare) 
        {
            foreach (var game in Games)
            {
                if (game.Title.Equals(titleToCompare)) 
                {
                    return true;
                }
            }
            return false;
        }

        public int GetIndex(string gameToModify)
        {
            for(int i = 0; i < this.Games.Count; i++)
            {
                if(Games[i].Title == gameToModify)
                {
                    return i;
                }
            }
            return -1;
        }

        public string DeleteGame(int gameIndex)
        {
            string gameDeleted = "";
            try
            {
                gameDeleted = Games[gameIndex].Title;
                Games[gameIndex].RemoveReviews();
                Games.RemoveAt(gameIndex);
                return gameDeleted;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }
    }
}
