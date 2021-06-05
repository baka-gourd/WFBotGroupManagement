using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Mirai_CSharp;
using Mirai_CSharp.Models;

namespace WFBotQQGroupManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var miraiPassword = Environment.GetEnvironmentVariable("MIRAI_AUTHKEY")!;
            var qqNumber = Environment.GetEnvironmentVariable("MIRAI_QQ")!.ToLong();
            var miraiHost = Environment.GetEnvironmentVariable("MIRAI_HOST");
            var miraiPort = Environment.GetEnvironmentVariable("MIRAI_PORT")!.ToInt();

            var options = new MiraiHttpSessionOptions(miraiHost!, miraiPort, miraiPassword!);
            var mirai = new MiraiHttpSession();
            while (!mirai.Connected) await mirai.ConnectAsync(options, qqNumber);
            mirai.DisconnectedEvt += async (_, _) =>
            {
                while (true)
                {
                    try
                    {
                        await mirai.ConnectAsync(options, qqNumber);
                        return true;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000);
                    }
                }
            };

            
            const long wfbotGroupNumber = 878527767;

            mirai.GroupApplyEvt += async (_, eventArgs) =>
            {
                if (eventArgs.FromGroup != wfbotGroupNumber) return true;
                try
                {
                    var msg = eventArgs.Message.ToLower();
                    var keywords = new[] { "github", "黑猫" };
                    if (keywords.Any(keyword => msg.Contains(keyword)))
                    {
                        await mirai.HandleGroupApplyAsync(eventArgs, GroupApplyActions.Allow);
                        await SendGroupMessage($"已经自动放行加群申请.");
                        return true;
                    }

                    await SendGroupMessage("检测到新的加群申请.");
                }
                catch (Exception e)
                {
                    await SendGroupMessage($"处理自动加群时发生了问题: {e}");
                }
                return true;
            };

            await Task.Delay(-1);

            Task SendGroupMessage(string message) =>
                mirai.SendGroupMessageAsync(wfbotGroupNumber, new PlainMessage(message));
        }

    }
}
