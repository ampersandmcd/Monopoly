using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Card
    {
        private string tag;
        private string type;
        private string category;
        private string name;
        private string description;
        private int effect;
        private string div = "---------------------------------------------------------";

        public Card(string[] attributes)
        {
            tag = attributes[1];
            type = attributes[2];
            category = attributes[3];
            name = attributes[4];
            description = attributes[5];
            effect = Convert.ToInt32(attributes[6]);
        }

        public string get_tag()
        {
            return tag;
        }

        public string get_category()
        {
            return category;
        }

        public int get_effect()
        {
            return effect;
        }

        public string get_type()
        {
            return type;
        }

        public override string ToString()
        {
            return String.Format("\n{0}\n{1}\n\n{2}\n\n{3}\n{0}\n", div, type, name, description);
        }
    }
}
