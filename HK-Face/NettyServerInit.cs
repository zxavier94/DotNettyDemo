

namespace HK_Face
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Codecs;
    using DotNetty.Handlers.Logging;
    using DotNetty.Handlers.Tls;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Transport.Libuv;
    using HK_Face.Common;

    class NettyServerInit
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Instance
        private static NettyServerInit server = new NettyServerInit();
        public static NettyServerInit Instance => server;

        private NettyServerInit()
        {

        }
        #endregion

        /// <summary>
        /// 服务器是否已运行
        /// </summary>
        private bool isServerRunning = false;
        /// <summary>
        /// 关闭侦听器事件
        /// </summary>
        private ManualResetEvent ClosingArrivedEvent = new ManualResetEvent(false);
        /// <summary>
        /// 启动服务
        /// </summary>

        public void Start()
        {
            try
            {
                logger.Info("isServerRunning:[{0}]", isServerRunning);
                if (isServerRunning)
                {
                    ClosingArrivedEvent.Set();  // 停止侦听
                }
                else
                {
                    //线程池任务
                    ThreadPool.QueueUserWorkItem(ThreadPoolCallback);
                }
            }
            catch (Exception exp)
            {
                logger.Error(exp);
            }
        }

        private void ThreadPoolCallback(object state)
        {
            RunServerAsync().Wait();
        }

        private async Task RunServerAsync()
        {
            // FaceServerHelper.init();
            Environment.SetEnvironmentVariable("io.netty.allocator.numDirectArenas", "0");

            IEventLoopGroup bossGroup;
            IEventLoopGroup workerGroup;

            logger.Info("UseLibuv:[{0}]", ServerSettings.UseLibuv);
            if (ServerSettings.UseLibuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
            }
            else
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();
            }

            X509Certificate2 tlsCertificate = null;
            logger.Info("IsSsl:[{0}]", ServerSettings.IsSsl);
            if (ServerSettings.IsSsl)
            {
                tlsCertificate = new X509Certificate2(Path.Combine(FaceServerHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            }
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);

                if (ServerSettings.UseLibuv)
                {
                    bootstrap.Channel<TcpServerChannel>();
                }
                else
                {
                    bootstrap.Channel<TcpServerSocketChannel>();
                }

                bootstrap
                    .Option(ChannelOption.SoBacklog, 100)  // 设置网络IO参数等
                    .Option(ChannelOption.SoKeepalive, true)  // 保持连接
                    //.Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast("tls", TlsHandler.Server(tlsCertificate));
                        }
                        //pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        //pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        //pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("G301 decoder", new G301FrameDecoder());
                        pipeline.AddLast("echo", new FaceServerHandler());
                    }));

                IChannel boundChannel = await bootstrap.BindAsync(ServerSettings.Port);

                //运行至此处，服务启动成功
                isServerRunning = true;

                ClosingArrivedEvent.Reset();
                ClosingArrivedEvent.WaitOne();

                await boundChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }
    }
}
