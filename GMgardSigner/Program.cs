using System;
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

        static async Task Main(string[] args)
        {
            var user = GetUser(args);
            if(!(await user.CheckLoginAsync()))
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
                            Environment.Exit(-1);
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
                Environment.Exit(-1);
            }

            Environment.Exit(0);
        }

        static User GetUser(string[] args)
        {
            string username = null, password = null;

            if (args.Length > 0)
                username = args[0];
            else
            {
                Console.Write("输入用户名: ");
                username = Console.ReadLine();
            }

            var path = GetUserDataPath(username);
            if(File.Exists(path))
            {
                try
                {
                    return File.ReadAllBytes(path).ToUser();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                }
            }

            if(args.Length > 1)
            {
                password = args[1];
            }
            else
            {
                Console.Write("输入密码: ");
                password = Console.ReadLine();
                Console.SetCursorPosition(10, Console.CursorTop - 1);
                Console.WriteLine(new string('*', password.Length));
            }

            return User.Create(username, password);
        }

        public static void SaveUser(User user)
        {
            try
            {
                Directory.CreateDirectory(AppDataPath);
                var path = GetUserDataPath(user.Username);
                File.WriteAllBytes(path, user.ToBytes());
            }
            catch { }
        }

        public static string GetUserDataPath(string username)
        {
            using (var md5 = MD5.Create())
            {
                byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
                return AppDataPath + string.Join("", s.Select(c => c.ToString("x")));
            }
        }
    }
}
