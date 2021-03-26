using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        public static string reverseString(String str)
        {
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        ///     Check for internet connection
        /// </summary>
        /// <returns>true if internet connection is available</returns>
        public static bool CheckInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
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
        ///     Checks if website loads correctly
        /// </summary>
        /// <param name="url">website to load</param>
        /// <param name="timeoutMs">timeout to quit trying to connect</param>
        /// <returns>True if connection is good, false if something is not right</returns>
        public static bool CheckWebSiteLoad(string url, int timeoutMs)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Timeout = timeoutMs;
            request.Method = "GET";
            try
            {
                var res = (HttpWebResponse) request.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }


            return false;
        }

        /// <summary>
        ///     Pings an url
        /// </summary>
        /// <param name="url">domain to ping</param>
        /// <returns>true if any ping returns otherwise false</returns>
        public static bool PingDomain(string url)
        {
            var ping = new Ping();

            var result = ping.Send(url);

            if (result.Status != IPStatus.Success)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Shows message box with error configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Error message inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showError(Window owner, String message)
        {
            return MessageBox.Show(owner, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        ///     Shows message box with warning configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Warning message inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showWarning(Window owner, String message)
        {
            return MessageBox.Show(owner, message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        ///     Shows message box with information configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Message inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showMessage(Window owner, String message, String title)
        {
            return MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        ///     Shows message box with question configurations
        /// </summary>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Question inside message box</param>
        /// <returns></returns>
        public static MessageBoxResult showConfirmation(Window owner, String message, String title)
        {
            return MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        /// <summary>
        ///     Randomize List elements
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="source">The list</param>
        /// <returns>Randomized list</returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            var rnd = new Random();
            return source.OrderBy(item => rnd.Next());
        }

        public static long SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (long i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        /// <summary>
        ///     Byte Convertions Class
        /// </summary>
        public static class Convertions
        {
            /// <summary>
            ///     Returns ASCII Text value of a Byte Array
            /// </summary>
            /// <param name="arr">Byte array to convert</param>
            /// <returns></returns>
            public static string ByteArrToAscii(byte[] arr)
            {
                var hexString = BitConverter.ToString(arr, 0, arr.Length).Replace("00", "");
                var sb = new StringBuilder();
                foreach (var s in Regex.Split(hexString, "-"))
                {
                    for (var i = 0; i <= s.Length - 2; i += 2)
                    {
                        sb.Append(
                            Convert.ToString(Convert.ToChar(Int32.Parse(s.Substring(i, 2), NumberStyles.HexNumber))));
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            ///     Alternative method to convert byte array into ASCII text string
            /// </summary>
            /// <param name="inp">Byte array to convert</param>
            /// <returns></returns>
            public static string ByteToASCII(byte[] inp)
            {
                return Encoding.ASCII.GetString(inp);
            }

            /// <summary>
            ///     Convert Integer into Hexadecimal string
            /// </summary>
            /// <param name="num">Integer to convert</param>
            /// <returns></returns>
            public static string IntToHex(int num)
            {
                return num.ToString("X");
            }

            /// <summary>
            ///     Convert Hexadecimal string to long
            /// </summary>
            /// <param name="hex">Hexadecimal number in string</param>
            /// <returns></returns>
            public static long HexToDec(string hex)
            {
                return Int64.Parse(hex, NumberStyles.HexNumber);
            }

            /// <summary>
            ///     Converts string to byte array
            /// </summary>
            /// <param name="str">String to convert to byte array</param>
            /// <returns></returns>
            public static byte[] StringToByteArray(String str)
            {
                var NumberChars = str.Length;
                var bytes = new byte[NumberChars/2];
                for (var i = 0; i < NumberChars; i += 2)
                    bytes[i/2] = Convert.ToByte(str.Substring(i, 2), 16);
                return bytes;
            }
        }

        /// <summary>
        ///     Class to handle INI Setting files
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
            ///     Write value into a key inside an ini file
            /// </summary>
            /// <param name="Section">Section inside ini file</param>
            /// <param name="Key">Variable inside section</param>
            /// <param name="Value">Value to assign to the key</param>
            public void IniWriteValue(string Section, string Key, string Value)
            {
                WritePrivateProfileString(Section, Key, Value, path);
            }

            /// <summary>
            ///     Read a key from an ini file
            /// </summary>
            /// <param name="Section">Section inside ini file</param>
            /// <param name="Key">Variable inside section</param>
            /// <returns>String with value of key</returns>
            public string IniReadValue(string Section, string Key)
            {
                var temp = new StringBuilder(255);
                var i = GetPrivateProfileString(Section, Key, "", temp,
                    255, path);
                return temp.ToString();
            }
        }

        /// <summary>
        ///     Class to handle logs of application
        /// </summary>
        public class Log
        {
            private readonly string log_folder;

            public Log(String LogPath)
            {
                log_folder = LogPath;
            }

            /// <summary>
            ///     Write new line into log file
            /// </summary>
            /// <param name="text">text to write</param>
            public void WriteLine(String text)
            {
                var dir = new DirectoryInfo(log_folder);
                var min = 0;
                var number = 0;
                foreach (var file in dir.GetFiles("*.log"))
                {
                    number = Int32.Parse(file.Name.Replace(".log", "").Replace("CELO_LOG_", ""));
                    if (number > min)
                    {
                        min = number;
                    }
                }

                var dt = DateTime.Now;
                var dateFormat = String.Format("{0}:{1}:{2}:{3}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                try
                {
                    File.AppendAllText(log_folder + @"\CELO_LOG_" + min + ".log",
                        dateFormat + " : " + text + Environment.NewLine);
                }
                catch
                {
                }
            }

            /// <summary>
            ///     Create new log file
            /// </summary>
            public void CreateNew()
            {
                if (!Directory.Exists(log_folder))
                {
                    Directory.CreateDirectory(log_folder);
                }

                var dir = new DirectoryInfo(log_folder);
                var min = 0;
                var number = 0;
                foreach (var file in dir.GetFiles())
                {
                    number = Int32.Parse(file.Name.Replace(".log", "").Replace("CELO_LOG_", ""));
                    if (number > min)
                    {
                        min = number;
                    }
                }

                var log_name = "CELO_LOG_" + (min + 1) + ".log";


                File.WriteAllText(log_folder + @"\" + log_name,
                    "NEFFWARE - CELO ENHANCED LOG FILE" + Environment.NewLine + DateTime.Now.ToString("F") +
                    Environment.NewLine + Environment.NewLine);
            }
        }

        public class FastCRC32
        {
            private static readonly uint[] crc_32_tab = // CRC polynomial 0xedb88320 
            {
                0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f,
                0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
                0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2,
                0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
                0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
                0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
                0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c,
                0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
                0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423,
                0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
                0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x01db7106,
                0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
                0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d,
                0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
                0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
                0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
                0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7,
                0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
                0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa,
                0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
                0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81,
                0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
                0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84,
                0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
                0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
                0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
                0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8, 0xa1d1937e,
                0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
                0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55,
                0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
                0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28,
                0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
                0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f,
                0x72076785, 0x05005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
                0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
                0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
                0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69,
                0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
                0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc,
                0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
                0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693,
                0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
                0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
            };

            internal uint CheckSum { get; set; }

            private static uint UPDC32(byte octet, uint crc)
            {
                return (crc_32_tab[((crc) ^ octet) & 0xff] ^ ((crc) >> 8));
            }

            internal uint AddToCRC32(int c)
            {
                return AddToCRC32((ushort) c);
            }

            internal uint AddToCRC32(ushort c)
            {
                byte lowByte, hiByte;
                lowByte = (byte) (c & 0x00ff);
                hiByte = (byte) (c >> 8);
                CheckSum = UPDC32(hiByte, CheckSum);
                CheckSum = UPDC32(lowByte, CheckSum);
                return ~CheckSum;
            }

            /// <summary>
            ///     Compute a checksum for a given string.
            /// </summary>
            /// <param name="text">The string to compute the checksum for.</param>
            /// <returns>The computed checksum.</returns>
            public static uint CRC32String(string text)
            {
                uint oldcrc32;
                oldcrc32 = 0xFFFFFFFF;
                var len = text.Length;
                ushort uCharVal;
                byte lowByte, hiByte;

                for (var i = 0; len > 0; i++)
                {
                    --len;
                    uCharVal = text[len];
                    unchecked
                    {
                        lowByte = (byte) (uCharVal & 0x00ff);
                        hiByte = (byte) (uCharVal >> 8);
                    }
                    oldcrc32 = UPDC32(hiByte, oldcrc32);
                    oldcrc32 = UPDC32(lowByte, oldcrc32);
                }

                return ~oldcrc32;
            }

            /// <summary>
            ///     Compute a checksum for a given array of bytes.
            /// </summary>
            /// <param name="bytes">The array of bytes to compute the checksum for.</param>
            /// <returns>The computed checksum.</returns>
            public static uint CRC32Bytes(byte[] bytes)
            {
                uint oldcrc32;
                oldcrc32 = 0xFFFFFFFF;
                var len = bytes.Length;

                for (var i = 0; len > 0; i++)
                {
                    --len;
                    oldcrc32 = UPDC32(bytes[len], oldcrc32);
                }
                return ~oldcrc32;
            }
        }

        public class SimpleTripleDES
        {
            private const string encryptionKey = "[26nasSS3dZZk5la2s]3Sd";

            public static string Encrypt3DES(string toEncrypt, string salt, bool useHashing = true)
            {
                byte[] keyArray;
                var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

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
                var cTransform = tdes.CreateEncryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();

                // Return the encrypted data into unreadable string format
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }

            public static string Decrypt3DES(string cipherString, string salt, bool useHashing = true)
            {
                byte[] keyArray;
                var toEncryptArray = Convert.FromBase64String(cipherString.Replace(' ', '+'));

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

                var cTransform = tdes.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();

                // Return the Clear decrypted TEXT
                return Encoding.UTF8.GetString(resultArray);
            }
        }

        /// <summary>
        ///     Class to handle steam api
        /// </summary>
        public static class Steam
        {
            /// <summary>
            ///     Steam Developer Key to access steam api
            /// </summary>
            private const String DevKey = "D5818526BB103AD0F74740C3A264878F";

            /// <summary>
            ///     Gets time played from a steam user
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
                        var json =
                            web.DownloadString(
                                String.Format(
                                    "http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&steamid={1}&format=json",
                                    DevKey, id));
                        if (json != null)
                        {
                            var ret = Regex.Split(json, "\n");
                            var z = 0;
                            foreach (var s in ret)
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
            ///     Get Steam user nickname
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
                        var json =
                            web.DownloadString(
                                String.Format(
                                    "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}",
                                    DevKey, id));
                        var obj = JObject.Parse(json);
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