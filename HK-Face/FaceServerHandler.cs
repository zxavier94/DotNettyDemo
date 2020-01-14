using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HK_Face
{
    class FaceServerHandler : ChannelHandlerAdapter
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override bool IsSharable => true;

        public override void ChannelActive(IChannelHandlerContext context)
        {
            logger.Info("打开连接:[{0}]", context.Channel.RemoteAddress.ToString());
            //base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            logger.Info("关闭连接:[{0}]", context.Channel.RemoteAddress.ToString());
            base.ChannelInactive(context);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            logger.Error(exception);
            context.CloseAsync();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IByteBuffer buffer = null;
            try
            {
                if (message != null)
                {
                    logger.Info("read from [{0}]", context.Channel.RemoteAddress.ToString());
                    buffer = message as IByteBuffer;
                    if (buffer != null)
                    {
                        logger.Info("Received from client: [{0}], msg: [{1}]", context.Channel.RemoteAddress.ToString(), buffer.ToString(Encoding.UTF8));
                    }

                    //context.WriteAsync(message);
                    context.Channel.WriteAndFlushAsync(message);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        #region 为什么实现 SimpleChannelInboundHandler<object> 使用以下代码 只能接受一段消息,并且最后连接还断开?
        // 
        //protected override void ChannelRead0(IChannelHandlerContext context, object message)
        //{
        //    IByteBuffer buffer = null;
        //    try
        //    {
        //        if(message != null)
        //        {
        //            logger.Info("read from [{0}]", context.Channel.RemoteAddress.ToString());
        //            buffer = message as IByteBuffer;
        //            if (buffer != null)
        //            {
        //                //logger.Info("Received from client: [{0}], msg: [{1}]", context.Channel.RemoteAddress.ToString(), buffer.ToString(Encoding.UTF8));
        //            }

        //            //context.WriteAsync(message);
        //            context.Channel.WriteAndFlushAsync(message);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e);
        //        throw;
        //    }
        //}
        #endregion
    }
}
