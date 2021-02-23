using System;
using System.IO;
using CPQ.ExcelToSQL.Managers;

namespace CPQ.ExcelToSQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string path = @"C:\Users\stefanolo\Desktop\CPQ 21.04\DCC Cliente Matrice Persone Giuridiche v3.0_ncb.xlsx";

            var manager = new ExcelManager();
            using FileStream fs = File.Open(path, FileMode.Open);
            var result = manager.ImportExcelCPQDocs(fs).Result;
            foreach (var mess in result)
                Console.WriteLine(mess);
        }
    }
}
