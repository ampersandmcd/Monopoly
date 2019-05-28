using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Graphics
    {
        private static int[] free_parking_loc = { 40, 76 };

        public static string[] render(List<Player> players, List<Property> board, List<int[]> spaces_player, List<int[]> spaces_houses, 
            string[] original_board_art, int free_parking)
        {
            // create copy when rendering board
            string[] board_art = new string[original_board_art.Length];
            Array.Copy(original_board_art, board_art, original_board_art.Length);

            // place players
            foreach (Player p in players)
            {
                int index = p.get_position();
                int row = spaces_player[index][0];
                int col = spaces_player[index][1];
                place_player(p, ref board_art, row, col);
            }

            // place houses
            foreach (Property property in board)
            {
                int index = property.get_position();
                int row = spaces_houses[index][0];
                int col = spaces_houses[index][1];
                place_house(property, ref board_art, row, col);
            }

            // place free parking
            int free_parking_len = free_parking.ToString().Length;
            board_art[free_parking_loc[0]] = board_art[free_parking_loc[0]].Substring(0, free_parking_loc[1]) + 
                free_parking + 
                board_art[free_parking_loc[0]].Substring(free_parking_loc[1] + free_parking_len);


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

        private static void place_house(Property property, ref string[] board_art, int row, int col)
        {
            // create string of players at space
            string current = board_art[row].Substring(col, 5); // take 5-length substring in case of max. 5 houses
            string updated = "";
            if (property.owned())
            {
                if (property.get_houses() == 5) // hotel
                {
                    updated = "Hotel";
                }
                else if (property.mortgaged())
                {
                    updated = "mmmmm";
                }
                else // everything else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i < (property.get_houses()))
                        {
                            updated += "h";
                        }
                        else
                        {
                            updated += "-";
                        }
                    }
                }
            }
            else
            {
                updated = current;
            }

            // update board
            board_art[row] = board_art[row].Substring(0, col) + updated + board_art[row].Substring(col + 5);
        }

    }
}
