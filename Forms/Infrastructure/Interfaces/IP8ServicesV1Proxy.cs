using P8ServicesV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Infrastructure.Interfaces
{
    public interface IP8ServicesV1Proxy
    {
        Task<P8ServiceClient> GetClientAsync();
    }
}
