using ISO11820.Forms;
using ISO11820.Global;
using Microsoft.Extensions.Configuration;

namespace ISO11820;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // 加载配置
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        AppCtx.Initialize(config);

        Application.Run(new LoginForm());
    }
}