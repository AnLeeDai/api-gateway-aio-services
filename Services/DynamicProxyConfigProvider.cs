using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using ApiGateway.Configuration;

namespace ApiGateway.Services;

public class DynamicProxyConfigProvider : IProxyConfigProvider
{
    private readonly TextGenerateServiceConfig _serviceConfig;
    private readonly AppHealthCheckConfig _healthConfig;
    private readonly LoadBalancingConfig _loadBalancingConfig;
    private volatile InMemoryConfigProvider _configProvider = null!;

    public DynamicProxyConfigProvider(
        IOptions<TextGenerateServiceConfig> serviceConfig,
        IOptions<AppHealthCheckConfig> healthConfig,
        IOptions<LoadBalancingConfig> loadBalancingConfig)
    {
        _serviceConfig = serviceConfig.Value;
        _healthConfig = healthConfig.Value;
        _loadBalancingConfig = loadBalancingConfig.Value;
        
        UpdateConfig();
    }

    public IProxyConfig GetConfig() => _configProvider.GetConfig();

    private void UpdateConfig()
    {
        var routes = new[]
        {
            new RouteConfig
            {
                RouteId = "text-generate-main-api",
                ClusterId = "cluster-text-generate",
                Match = new RouteMatch
                {
                    Path = "/api/text-generate/{action}",
                    Methods = new[] { "GET", "POST", "PUT", "DELETE" }
                },
                Transforms = new[]
                {
                    new Dictionary<string, string> { { "PathPattern", "/api/{action}" } }
                }
            },
            new RouteConfig
            {
                RouteId = "text-generate-bank-bill",
                ClusterId = "cluster-text-generate",
                Match = new RouteMatch
                {
                    Path = "/api/bank-bill/{**catchall}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string> { { "PathPattern", "/api/bank-bill/{**catchall}" } }
                }
            },
            new RouteConfig
            {
                RouteId = "text-generate-fallback",
                ClusterId = "cluster-text-generate",
                Match = new RouteMatch
                {
                    Path = "/api/proxy/{**catchall}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string> { { "PathPattern", "/api/{**catchall}" } }
                }
            }
        };

        var clusters = new[]
        {
            new ClusterConfig
            {
                ClusterId = "cluster-text-generate",
                LoadBalancingPolicy = _loadBalancingConfig.Policy,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    {
                        "primary",
                        new DestinationConfig { Address = _serviceConfig.BaseUrl }
                    }
                },
                HealthCheck = new Yarp.ReverseProxy.Configuration.HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.Parse(_healthConfig.Interval),
                        Timeout = TimeSpan.Parse(_healthConfig.Timeout),
                        Policy = "ConsecutiveFailures",
                        Path = _healthConfig.Path
                    },
                    Passive = new PassiveHealthCheckConfig
                    {
                        Enabled = true,
                        Policy = "TransportFailureRate",
                        ReactivationPeriod = TimeSpan.Parse(_healthConfig.ReactivationPeriod)
                    }
                }
            }
        };

        var config = new InMemoryConfigProvider(routes, clusters);
        _configProvider = config;
    }
}
