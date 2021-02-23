using Microsoft.Extensions.Configuration;

namespace CPQ.ExcelToSQL
{
    public interface IConnectionsString
    {
        string GetConnectionString();
    }

    public class ConnectionsString : IConnectionsString
    {
        public IConfiguration Configuration { get; }

        public ConnectionsString(IConfiguration config)
        {
            Configuration = config;
        }

        public string GetConnectionString()
        {
            return Configuration.GetConnectionString("DefaultConnection");
        }
    }
}
