using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HK_Face
{
    class G301FrameDecoder : ByteToMessageDecoder
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        readonly int minFrameLength = 13;
        readonly int maxFrameLength = 50 * 1024 + 13;

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            object decoded = this.Decode(context, input);
            if (decoded != null)
            {
                output.Add(decoded);
            }

        }

        protected virtual object Decode(IChannelHandlerContext context, IByteBuffer input)
        {
            try
            {
                if (input.ReadableBytes > 0)
                {

                    int readerIndex = input.ReaderIndex;
                    // int actualFrameLength = frameLengthInt - this.initialBytesToStrip;
                    int actualFrameLength = 2;
                    IByteBuffer frame = null;
                    if (input.ReadableBytes >= actualFrameLength)
                    {
                        frame = this.ExtractFrame(context, input, readerIndex, actualFrameLength);
                        input.SetReaderIndex(readerIndex + actualFrameLength);
                    }
                    else
                    {
                        frame = this.ExtractFrame(context, input, readerIndex, input.ReadableBytes);
                        input.SetReaderIndex(readerIndex + input.ReadableBytes);
                    }
                    return frame;
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            return null;
        }

        protected virtual IByteBuffer ExtractFrame(IChannelHandlerContext context, IByteBuffer buffer, int index, int length)
        {
            IByteBuffer buff = buffer.Slice(index, length);
            buff.Retain();
            return buff;
        }
    }
}
