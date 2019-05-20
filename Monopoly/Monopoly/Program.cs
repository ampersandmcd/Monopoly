using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class Program
    {
        static void Main(string[] args)
        {
            // parse property file into memory
            List<Property> properties = new List<Property>();
            string path = @"M:\monopoly\beginner_board.csv";
            var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(new string[] { ";" });
            // skip first row;
            parser.ReadFields();
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields()[0].Split(',');
                properties.Add(new Property(row));
            }

            foreach (Property p in properties)
            {
                Console.WriteLine(p + "\n");
            }
        }
    }
}
