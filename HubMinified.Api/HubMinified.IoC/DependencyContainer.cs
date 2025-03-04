using HubMinified.Application.Consumer;
using HubMinified.Application.Services;
using HubMinified.Data.Mongo.Context;
using HubMinified.Data.Repository;
using HubMinified.Domain.Interfaces.Repository;
using HubMinified.Domain.Interfaces.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace HubMinified.IoC;

public class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
    {
        #region Mongo
        var mongoConnectionString = configuration.GetConnectionString("DatabaseMongo")!;

        services.AddScoped<MongoContext>(provider =>
            {
                return new MongoContext(mongoConnectionString);
            }
        );
        #endregion

        #region UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        #endregion

        #region Mapper
        // var mapperConfig = new MapperConfiguration(mc =>
        // {
        //     mc.AddProfile(new MappingProfile());
        // });

        // IMapper mapper = mapperConfig.CreateMapper();
        // services.AddSingleton(mapper);
        #endregion

        #region Services
        services.AddScoped<IFreightService, FreightService>();
        services.AddScoped<IProductService, ProductService>();
        #endregion

        #region ResiliencePipeline
        services.AddResiliencePipeline("default", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(2), // Tempo de espera inicial entre as tentativas de repetição
                MaxRetryAttempts = 2, // Número máximo de tentativas de repetição
                BackoffType = DelayBackoffType.Exponential, // Tipo de backoff (exponencial)
                UseJitter = true // Adiciona variação aleatória ao tempo de espera entre as tentativas de repetição
            });

            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5, // Se 50% das tentativas falharem, o circuito será aberto
                SamplingDuration = TimeSpan.FromSeconds(10), // As falhas são monitoradas durante um período de 10 segundos
                MinimumThroughput = 8, // Pelo menos 8 tentativas devem ocorrer durante o período de amostragem
                BreakDuration = TimeSpan.FromSeconds(30), // O circuito permanecerá aberto por 30 segundos antes de permitir novas tentativas
            });

            builder.AddTimeout(TimeSpan.FromSeconds(5)); // Adiciona um tempo limite de 5 segundos para as operações
        }
        );
        #endregion

        #region MassTransit
        string busConnectionstring = configuration.GetSection("Bus:ConnectionString").Value ?? "";

        services.AddMassTransit(x =>
            {
                x.AddConsumer<ProductInfoConsumerService>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(busConnectionstring), cfg =>
                        {
                            cfg.Username("guest");
                            cfg.Password("guest");
                        });

                    cfg.ConfigureEndpoints(context);
                });
            });
        #endregion
    }
}
