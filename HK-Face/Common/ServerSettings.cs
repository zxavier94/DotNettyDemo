﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HK_Face.Common
{
    public static class ServerSettings
    {
        public static bool IsSsl
        {
            get
            {
                string ssl = FaceServerHelper.Configuration["ssl"];
                return !string.IsNullOrEmpty(ssl) && bool.Parse(ssl);
            }
        }

        public static int Port => int.Parse(FaceServerHelper.Configuration["port"]);

        public static bool UseLibuv
        {
            get
            {
                string libuv = FaceServerHelper.Configuration["libuv"];
                return !string.IsNullOrEmpty(libuv) && bool.Parse(libuv);
            }
        }
    }
}
