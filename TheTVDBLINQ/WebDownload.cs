using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TheTVDBLINQ
{
    public class WebDownload : WebClient
    {
        public int Timeout { get; set; }

        public WebDownload()
            : this(15000)
        {
        }

        public WebDownload(int timeout)
        {
            this.Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            if (webRequest != null)
                webRequest.Timeout = this.Timeout;
            return webRequest;
        }
    }
}
