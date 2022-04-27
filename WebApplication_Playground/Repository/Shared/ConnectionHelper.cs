using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Shared
{
    public sealed class ConnectionHelper
    {

        private readonly Dictionary<string, string> _connections;

        private readonly Func<string, string> _getConnection;

        public ConnectionHelper(Dictionary<string,string> connections, Func<string,string> getConnection)
        {
            this._connections = connections;
            this._getConnection = getConnection;
        }

        public string getConnection(string connectingClassName)
        {
            return this._getConnection(this._connections[connectingClassName]);
        }

    }
}
