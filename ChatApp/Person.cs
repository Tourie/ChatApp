using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ChatApp
{
    public class Person
    {
        public Person(string id, IPEndPoint localIp, IPEndPoint remoteIp)
        {
            Id = id;
            LocalIp = localIp;
            RemoteIp = remoteIp;
        }

        public string Id { get; init; }
        public IPEndPoint LocalIp { get; init; }
        public IPEndPoint RemoteIp { get; set; }
    }
}
