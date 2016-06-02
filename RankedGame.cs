using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HQMRanked
{
    public class RankedGame
    {
        public bool InProgress = false;
        public List<User> Players = new List<User>();


        public void StartGame()
        {
            InProgress = true;
        }

        public void EndGame()
        {
            InProgress = false;
            
            //calculate rating differences
            //save new ratings
            //post results
        }

        

        
        
        
    }
}
