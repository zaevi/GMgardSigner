using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GMgardSigner
{
    class Program
    {
        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\GMgardSigner\";

        static List<string> Args = null;

        static bool Silent = false;

        static async Task Main(string[] args)
        {
            Args = new List<string>(args.Select(a => a.StartsWith("-") ? a.ToLower() : a));
            if (Args.Contains("-s")) Silent = true;

            try
            {
                var user = GetUser(args);
                if(await Signer.Execute(user))
                {
                    SaveUser(user);
                }
            }
            catch(Exception ex)
            {
                Error($"({ex.GetType().Name}: {ex.Message})");
            }

            if (!Args.Contains("-s"))
            {
                Console.Write("按下任意键退出...");
                Console.ReadKey();
            }
        }

        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log(message);
            Console.ResetColor();
        }

        static User GetUser(string[] args)
        {
            User user = null;

            var username = GetParameterValue("-u") ?? GetInput("输入账号: ") ?? throw new ArgumentException("没有提供账号");

            var path = GetUserDataPath(username);
            if(File.Exists(path))
            {
                try
                {
                    user = File.ReadAllBytes(path).ToUser();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"({ex.GetType().Name}: {ex.Message})");
                }
            }

            var password = GetParameterValue("-p");
            if (user == null)
            {
                password = password ?? GetInput("输入密码: ", true) ?? throw new ArgumentException("没有提供密码");
                user = User.Create(username, password);
            }
            else if(password != null)
            {
                user.Password = password; // 更新密码
            }

            return user;
        }

        public static void SaveUser(User user)
        {
            try
            {
                Directory.CreateDirectory(AppDataPath);
                var path = GetUserDataPath(user.Username);
                File.WriteAllBytes(path, user.ToBytes());
            }
            catch(Exception ex)
            {
                Console.WriteLine($"({ex.GetType().Name}: {ex.Message})");
            }
        }

        public static string GetUserDataPath(string username)
        {
            using (var md5 = MD5.Create())
            {
                byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
                return AppDataPath + string.Join("", s.Select(c => c.ToString("x")));
            }
        }

        static string GetParameterValue(string key)
        {
            var index = Args.IndexOf(key) + 1;
            return (index > 0 && Args.Count > index) ? Args[index] : null; 
        }

        static string GetInput(string message, bool mask = false)
        {
            if (Silent) return null;
            Console.Write(message);
            var left = Console.CursorLeft;
            var value = Console.ReadLine();
            if(mask)
            {
                Console.SetCursorPosition(left, Console.CursorTop - 1);
                Console.WriteLine(new string('*', value.Length));
            }
            return value;
        }
    }
}
