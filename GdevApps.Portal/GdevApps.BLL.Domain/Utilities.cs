using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace GdevApps.BLL.Domain
{
    public static class Utilities
    {
        public static string RoundNumber(string roundSetup, int decimalRound, string numberStr)
        {
            int number = 0;
            if (int.TryParse(numberStr, out number))
            {
                return RoundNumber(roundSetup, decimalRound, number);
            }
            return numberStr;
        }

        public static string RoundNumber(string roundSetup, int decimalRound, int number)
        {
            roundSetup = roundSetup?.ToUpperInvariant() ?? "";
            var decimalNumber = Math.Pow(10, decimalRound);
            switch (roundSetup)
            {
                case "ROUND UP":
                    return (Math.Ceiling(number * decimalNumber) / decimalNumber).ToString();
                case "ROUND DOWN":
                    return (Math.Floor(number * decimalNumber) / decimalNumber).ToString();
                case "ROUND TO NEAREST":
                    return (Math.Round(number * decimalNumber) / decimalNumber).ToString();
                default:
                    return number.ToString();
            }
        }

        public static GradebookGrade GetNumberFromBrakets(string grade)
        {
            if (string.IsNullOrWhiteSpace(grade))
            {
                return new GradebookGrade
                {
                    Grade = null,
                    Total = null
                };
            }

            grade = Regex.Replace(grade, @"\s+", "");
            var gradeMath = Regex.Match(grade, @"([0-9]*([.][0-9]*)?)\[([0-9]*([.][0-9]*)?)\]$");

            // result index [0] - a.b[x.y], [1] - a.b, [2] - .b , [3] - x.y, [4] - .y
            var ab = gradeMath.Groups[1].Value;
            var xy = gradeMath.Groups[3].Value;

            if (gradeMath.Success && !string.IsNullOrWhiteSpace(ab) && !string.IsNullOrWhiteSpace(xy))
            {
                double studentGrade;
                double studentTotal;
                Double.TryParse(ab, out studentGrade);
                Double.TryParse(xy, out studentTotal);

                return new GradebookGrade
                {
                    Grade = studentGrade,
                    Total = studentTotal
                };
            }

            return new GradebookGrade
            {
                Grade = null,
                Total = null
            };
        }

        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
    }

}