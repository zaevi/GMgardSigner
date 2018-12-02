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

        public const int MaxLoginTimes = 2;

        static List<string> Args = null;

        static bool Silent = false;

        static async Task Main(string[] args)
        {
            Args = new List<string>(args.Select(a => a.StartsWith("-") ? a.ToLower() : a));
            if (Args.Contains("-s")) Silent = true;

            User user = null;
            bool hasLogined = false;
            try
            {
                user = GetUser(args);
                hasLogined = await user.CheckLoginAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"初始化失败 ({ex.GetType().Name}: {ex.Message})");
                Exit(-1);
            }

            if(!hasLogined)
            {
                for (int i = 1; i <= MaxLoginTimes; i++)
                {
                    Console.Write("尝试登录...");
                    try
                    {
                        await user.LoginAsync();
                        Console.WriteLine("成功!");
                        SaveUser(user);
                        break;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"失败! ({ex.GetType().Name}: {ex.Message})");
                        if(i == MaxLoginTimes)
                        {
                            Console.WriteLine($"连续{MaxLoginTimes}次登录失败, 可能账密有误; 也有可能脸太黑, 验证码均验证失败...");
                            Exit(-1);
                        }
                    }
                }
            }

            try
            {
                var result = await user.SignInAsync();
                if(result.Success)
                    Console.WriteLine($"签到成功! 经验+{result.Exp}, 已签到{result.Days}天");
                else
                    Console.WriteLine($"今天已签到. 已连签{result.Days}天");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"签到失败! ({ex.GetType().Name}: {ex.Message})");
                Exit(-1);
            }

            Exit(0);
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

        static void Exit(int code)
        {
            if(!Silent)
            {
                Console.Write("按下任意键退出...");
                Console.ReadKey();
            }
            Environment.Exit(code);
        }
    }
}
