using ProjectTemplate.Application.Common.Interfaces;
using StackExchange.Redis;

namespace ProjectTemplate.Application.Common.Services;
public class RedisServer(IServer server) : IRedisServer
{
    public IServer Server => server;
}
