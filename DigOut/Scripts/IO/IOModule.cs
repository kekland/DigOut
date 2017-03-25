using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace DigOut
{
    public static class IOModule
    {
        public static string FilePath = "\\World1.map";

        static StreamWriter Writer;
        static StreamReader Reader;
        public static void WriteString(string s)
        {
            Writer.Write(s);
        }

        public static void ManualOpen()
        {
            Writer = new StreamWriter(FilePath, true);
        }

        public static void ManualClose()
        {
            Writer.Close();
        }
        public static void Init()
        {
            FilePath = Directory.GetCurrentDirectory() + FilePath;
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath);
            }
        }

        public static void ClearText()
        {
            Writer = new StreamWriter(FilePath, false);
            Writer.Close();
        }
        public static void Write2DArray(int xc, int yc, int[,] a)
        {
            Writer = new StreamWriter(FilePath, true);
            Writer.Write(xc + "," + yc + "\r\n");
            for(int x = 0; x < a.GetLength(0); x++)
            {
                string ln = "";
                for(int y = 0; y < a.GetLength(1); y++)
                {
                    ln += a[x, y] + " ";
                }
                ln.Remove(ln.Length - 1, 1);
                WriteString(ln + "\r\n");
            }
            Writer.Close();
        }

        public static string ReadFull()
        {
            Reader = new StreamReader(FilePath);
            string text = Reader.ReadToEnd();
            Reader.Close();
            return text;
        }
    }
}
