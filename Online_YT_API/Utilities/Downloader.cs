using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resources
{
    class Downloader : IDownloader
    {

        public event GenericEvntHandler Completed;

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }


        public Entry Current
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
