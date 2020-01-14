using HK_Face.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace HK_Face
{
    public partial class HKFaceService : ServiceBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        const string logPath = @"C:\HKFaceLog.txt";

        public HKFaceService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            using (FileStream stream = new FileStream(logPath, FileMode.Append))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                try
                {
                    NettyServerInit.Instance.Start();
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
                //writer.WriteLine($"{DateTime.Now},服务启动！");
                logger.Info("服务启动,时间:[{Date}], 端口号:[{port}]", DateTime.Now, FaceServerHelper.Configuration["port"]);
            }
        }

        protected override void OnStop()
        {
            using (FileStream stream = new FileStream(logPath, FileMode.Append))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                //writer.WriteLine($"{DateTime.Now},服务停止！");
                logger.Info("服务停止,时间:[{0}]", DateTime.Now);
                NLog.LogManager.Shutdown(); // Flush and close down internal threads and timers
            }
        }
    }
}
