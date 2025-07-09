using StackExchange.Redis;

namespace ProjectTemplate.Application.Common.Interfaces;
public interface IRedisServer
{
    IServer Server { get; }
}
