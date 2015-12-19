using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEMT
{
    class ServiceOppetArkiv
    {
        protected String url, type;

        public ServiceOppetArkiv(String type, String url)
        {
            this.url = url;
            this.type = type;
        }

        public void download()
        {

        }
    }
}
