using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Graphics
    {
        private static List<int[]> spaces = new List<int[]>() { // index: space index, first int: row number, second int: col number
            new int[2] { 56, 130 }, // go
            new int[2] { 56, 116 },
            new int[2] { 56, 104 },
            new int[2] { 56, 92 },
            new int[2] { 56, 80 },
            new int[2] { 56, 68 },
            new int[2] { 56, 56 },
            new int[2] { 56, 44 },
            new int[2] { 56, 32 },
            new int[2] { 56, 20 },
            new int[2] { 57, 4 }, // jail
            new int[2] { 50, 4 }, // st. charles
            new int[2] { 45, 4 }, 
            new int[2] { 40, 4 }, 
            new int[2] { 35, 4 }, 
            new int[2] { 30, 4 }, 
            new int[2] { 25, 4 }, 
            new int[2] { 20, 4 }, 
            new int[2] { 15, 4 }, 
            new int[2] { 10, 4 }, 
            new int[2] { 4, 4 }, // free parking
            new int[2] { 4, 20 }, // kentucky ave
            new int[2] { 4, 32 }, 
            new int[2] { 4, 44 }, 
            new int[2] { 4, 56 }, 
            new int[2] { 4, 68 }, 
            new int[2] { 4, 80 }, 
            new int[2] { 4, 92 }, 
            new int[2] { 4, 104 }, 
            new int[2] { 4, 116 }, 
            new int[2] { 4, 130 }, // go to jail
            new int[2] { 10, 130 }, // pacific ave
            new int[2] { 15, 130 }, 
            new int[2] { 20, 130 }, 
            new int[2] { 25, 130 }, 
            new int[2] { 30, 130 }, 
            new int[2] { 35, 130 }, 
            new int[2] { 40, 130 }, 
            new int[2] { 45, 130 }, 
            new int[2] { 50, 130 }, // boardwalk

            
        };

        public static string[] render(List<Player> players, List<Property> board, string[] original_board_art)
        {
            // create copy when rendering board
            string[] board_art = new string[original_board_art.Length];
            Array.Copy(original_board_art, board_art, original_board_art.Length);

            foreach (Player p in players)
            {
                int index = p.get_position();
                int row = spaces[index][0];
                int col = spaces[index][1];
                place_player(p, ref board_art, row, col);
            }
            
            return board_art;
        }

        private static void place_player(Player p, ref string[] board_art, int row, int col)
        {
            // create string of players at space
            string current = board_art[row].Substring(col, 8); // take 8-length substring in case of max. 8 players
            string updated = "";
            bool set = false;
            for (int i = 0; i < current.Length; i++)
            {
                if ((current[i] == ' ' || current[i] == '_') && set == false)
                {
                    updated += p.get_name()[0]; //append first letter of that player's name to the space
                    set = true;
                }
                else
                {
                    updated += current[i];
                }
            }

            // update board
            board_art[row] = board_art[row].Substring(0, col) + updated + board_art[row].Substring(col + 8);
        }

        private static void reset_board()
        {

        }

    }
}
