using System;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class Game
    {
        public string Title { get; set; }
        public string Genre  { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();
        public string Overview { get; set; } = "";
        public float Rating { get; set; } = 0;

        public override bool Equals(object obj)
        {
            var game = obj as Game;
            if(game == null)
            {
                return false;
            }
            else
            {
                return this.Title.Equals(game.Title);
            }
        }

        public void AddReview(Review r)
        {
            Reviews.Add(r);
            int cont = 0;
            foreach (var review in Reviews)
            {
                cont += review.Rate;
            }
            Rating = cont / Reviews.Count;
        }

        public void RemoveReviews()
        {
            if (Reviews != null)
            {
                this.Reviews.Clear();
            }
        }

        public string GetReviewsToPrint()
        {
            if (Reviews == null) 
            {
                return null;
            }
            string ret = "";
            int count = 1;
            foreach (var review in Reviews)
            {
                ret += $"{count++}. {review.Comment}\n";
            }
            return ret;
        }
    }
}