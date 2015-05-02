using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace CELO_Enhanced
{
    public static class Utilities
    {
        /// <summary>
        /// Check for internet connection
        /// </summary>
        /// <returns>true if internet connection is available</returns>
        public static bool CheckInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (Stream stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Shows message box with error configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Error message inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showError(Window owner, String message)
        {
            return MessageBox.Show(owner, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Shows message box with warning configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Warning message inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showWarning(Window owner, String message)
        {
            return MessageBox.Show(owner, message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Shows message box with information configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Message inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showMessage(Window owner, String message, String title)
        {
            return MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows message box with question configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Question inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showConfirmation(Window owner, String message, String title)
        {
            return MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        /// <summary>
        /// Randomize List elements
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="source">The list</param>
        /// <returns>Randomized list</returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            var rnd = new Random();
            return source.OrderBy(item => rnd.Next());
        }

        /// <summary>
        /// Byte Convertions Class
        /// </summary>
        public static class Convertions
        {
            /// <summary>
            /// Returns ASCII Text value of a Byte Array
            /// </summary>
            /// <param name="arr">Byte array to convert</param>
            /// <returns></returns>
            public static string ByteArrToAscii(byte[] arr)
            {
                String hexString = BitConverter.ToString(arr, 0, arr.Length).Replace("00", "");
                var sb = new StringBuilder();
                foreach (string s in Regex.Split(hexString, "-"))
                {
                    for (int i = 0; i <= s.Length - 2; i += 2)
                    {
                        sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(s.Substring(i, 2), NumberStyles.HexNumber))));
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            /// Alternative method to convert byte array into ASCII text string
            /// </summary>
            /// <param name="inp">Byte array to convert</param>
            /// <returns></returns>
            public static string ByteToASCII(byte[] inp)
            {
                return Encoding.ASCII.GetString(inp);
            }

            /// <summary>
            /// Convert Integer into Hexadecimal string
            /// </summary>
            /// <param name="num">Integer to convert</param>
            /// <returns></returns>
            public static string IntToHex(int num)
            {
                return num.ToString("X");
            }

            /// <summary>
            /// Convert Hexadecimal string to long
            /// </summary>
            /// <param name="hex">Hexadecimal number in string</param>
            /// <returns></returns>
            public static long HexToDec(string hex)
            {
                return Int64.Parse(hex, NumberStyles.HexNumber);
            }

            /// <summary>
            /// Converts string to byte array
            /// </summary>
            /// <param name="str">String to convert to byte array</param>
            /// <returns></returns>
            public static byte[] StringToByteArray(String str)
            {
                int NumberChars = str.Length;
                var bytes = new byte[NumberChars/2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i/2] = Convert.ToByte(str.Substring(i, 2), 16);
                return bytes;
            }

        }

        /// <summary>
        /// Class to handle INI Setting files
        /// </summary>
        public class INIFile
        {
            public string path;

            public INIFile(string INIPath)
            {
                path = INIPath;
            }

            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section,
                string key, string val, string filePath);

            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section,
                string key, string def, StringBuilder retVal,
                int size, string filePath);


            /// <summary>
            /// Write value into a key inside an ini file
            /// </summary>
            /// <param name="Section">Section inside ini file</param>
            /// <param name="Key">Variable inside section</param>
            /// <param name="Value">Value to assign to the key</param>
            public void IniWriteValue(string Section, string Key, string Value)
            {
                WritePrivateProfileString(Section, Key, Value, path);
            }


            /// <summary>
            /// Read a key from an ini file
            /// </summary>
            /// <param name="Section">Section inside ini file</param>
            /// <param name="Key">Variable inside section</param>
            /// <returns>String with value of key</returns>
            public string IniReadValue(string Section, string Key)
            {
                var temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Section, Key, "", temp,
                    255, path);
                return temp.ToString();
            }
        }

        /// <summary>
        /// Class to handle logs of application
        /// </summary>
        public class Log
        {
            private readonly string path;

            public Log(String LogPath)
            {
                path = LogPath;
            }

            /// <summary>
            /// Write new line into log file
            /// </summary>
            /// <param name="text">text to write</param>
            public void WriteLine(String text)
            {
                StringBuilder bd = new StringBuilder();
                bd.Append(DateTime.Now.ToString("G"));
                bd.Append(" - " + text);
                bd.Append(Environment.NewLine + "" + Environment.NewLine);
                File.AppendAllText(path, bd.ToString());
            }

            /// <summary>
            /// Create new log file
            /// </summary>
            public void CreateNew()
            {
                File.Delete(path);
                File.WriteAllText(path, "");
            }
        }

        public class SimpleTripleDES
        {
            private const string encryptionKey = "[26nasSS3dZZk5la2s]3Sd";

            public static string Encrypt3DES(string toEncrypt, string salt, bool useHashing = true)
            {
                byte[] keyArray;
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

                // If hashing use get hashcode regards to your key
                if (useHashing)
                {
                    var hashmd5 = new MD5CryptoServiceProvider();
                    keyArray =
                        hashmd5.ComputeHash(Encoding.UTF8.GetBytes(salt + encryptionKey + salt[0] + encryptionKey[4]));
                    hashmd5.Clear();
                }
                else
                    keyArray = Encoding.UTF8.GetBytes(encryptionKey);

                // Set the secret key for the tripleDES algorithm
                var tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                // Transform the specified region of bytes array to resultArray
                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();

                // Return the encrypted data into unreadable string format
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }

            public static string Decrypt3DES(string cipherString, string salt, bool useHashing = true)
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(cipherString.Replace(' ', '+'));

                if (useHashing)
                {
                    // If hashing was used get the hash code with regards to your key
                    var hashmd5 = new MD5CryptoServiceProvider();
                    keyArray =
                        hashmd5.ComputeHash(Encoding.UTF8.GetBytes(salt + encryptionKey + salt[0] + encryptionKey[4]));
                    hashmd5.Clear();
                }
                else
                {
                    // If hashing was not implemented get the byte code of the key
                    keyArray = Encoding.UTF8.GetBytes(encryptionKey);
                }

                // Set the secret key for the tripleDES algorithm
                var tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();

                // Return the Clear decrypted TEXT
                return Encoding.UTF8.GetString(resultArray);
            }
        }

        /// <summary>
        /// Class to handle steam api
        /// </summary>
        public static class Steam
        {
            /// <summary>
            /// Steam Developer Key to access steam api
            /// </summary>
            private const String DevKey = "D5818526BB103AD0F74740C3A264878F";

            /// <summary>
            /// Gets time played from a steam user
            /// </summary>
            /// <param name="id">Steam ID</param>
            /// <param name="gameID">Game ID</param>
            /// <returns>Returns time played or -1 if steam profile is private</returns>
            public static long getTimePlayed(long id, int gameID)
            {
                try
                {
                    var web = new WebClient();
                    using (web)
                    {
                        string json =
                            web.DownloadString(
                                String.Format(
                                    "http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&steamid={1}&format=json",
                                    DevKey, id));
                        if (json != null)
                        {
                            string[] ret = Regex.Split(json, "\n");
                            int z = 0;
                            foreach (string s in ret)
                            {
                                if (s.Contains("\"appid\": " + gameID + ","))
                                {
                                    if (ret[z + 1].Contains("_2weeks"))
                                    {
                                        return Int64.Parse(Regex.Split(ret[z + 2], ":")[1].Trim());
                                    }
                                    return Int64.Parse(Regex.Split(ret[z + 1], ":")[1].Trim());
                                }
                                z++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return -1;
                }
                return -1;
            }

            /// <summary>
            /// Get Steam user nickname
            /// </summary>
            /// <param name="id">Steam ID</param>
            /// <returns>User nickname</returns>
            public static string getNick(long id)
            {
                try
                {
                    var web = new WebClient();
                    using (web)
                    {
                        string json =
                            web.DownloadString(
                                String.Format(
                                    "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}",
                                    DevKey, id));
                        JObject obj = JObject.Parse(json);
                        if (obj.HasValues)
                        {
                            return obj["response"]["players"][0]["personaname"].ToString();
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                return null;
            }
        }
    }
}