using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GMgardSigner
{
    public static class Signer
    {

        public async static Task<bool> Execute(User user)
        {
            try
            {
                var hasLogined = await user.CheckLoginAsync();
                if (!hasLogined)
                {
                    Log("尝试登录...");
                    await user.LoginAsync();
                }
                Log("登录成功.");

                var signInResult = await user.SignInAsync();
                if (signInResult.Success)
                    Log($"签到成功! 经验+{signInResult.Exp}, 已签到{signInResult.Days}天");
                else
                    Log($"今天已签到. 已连签{signInResult.Days}天");

                return true;
            }
            catch (HttpRequestException ex) { Error($"请求异常: {ex.Message}"); }
            catch (GMgardException ex) { Error($"应用异常: {ex.Message}"); }
            catch (Exception ex)
            {
                // Error($"未知异常: ({ex.GetType().Name}: {ex.Message})");
                throw;
            }
            return false;
        }

        static void Log(string text) => Program.Log(text);

        static void Error(string text) => Program.Error(text);
    }
}
