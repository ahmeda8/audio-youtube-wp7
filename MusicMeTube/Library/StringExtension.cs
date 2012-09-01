using System;
using System.Net;

namespace MusicMeTube.Library
{
    public class StringExtension
    {
        public static string TitleCase(string str)
        {
            string[] splt = str.Split(' ');
            string return_val ="";
            foreach(string s in splt)
            {

                string v = s.ToLower();
                char[] c = s.ToCharArray();
                c[0] = char.ToUpper(c[0]);
                v = string.Concat(c);
                return_val = string.Concat(return_val, " ", v);
            }

            return return_val;
        }

    }
}
