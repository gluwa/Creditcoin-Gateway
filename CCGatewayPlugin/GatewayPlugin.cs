using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CCGatewayPlugin
{
    public interface ICCGatewayPluginAsync
    {
        Task<Tuple<bool, string>> Run(IConfiguration cfg, string[] command);
    }
}
