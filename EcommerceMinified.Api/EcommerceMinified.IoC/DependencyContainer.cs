using AutoMapper;
using EcommerceMinified.Application.Caching;
using EcommerceMinified.Application.Publishers;
using EcommerceMinified.Application.Services;
using EcommerceMinified.Data.Postgres.Context;
using EcommerceMinified.Data.Repository;
using EcommerceMinified.Data.Rest.Repository;
using EcommerceMinified.Domain.Interfaces.Caching;
using EcommerceMinified.Domain.Interfaces.Publishers;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.RestRepository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.Mapper;
using EcommerceMinified.Domain.Validators;
using EcommerceMinified.MsgContracts.Command;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace EcommerceMinified.IoC;

public class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
    {
        #region Postgres
        var postgresConnectionString = configuration.GetSection("DatabasePostgres").Value ?? "";

        services.AddDbContext<PostgresDbContext>(options =>
            {
                if (!string.IsNullOrEmpty(postgresConnectionString))
                {
                    options.UseNpgsql(postgresConnectionString);
                }
            }
        );
        #endregion

        #region UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        #endregion

        #region Mapper
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        });

        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
        #endregion

        #region Services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductInfoPublisherService, ProductInfoPublisherService>();
        #endregion

        #region RestRepository
        services.AddScoped<IHubMinifiedRestRespository, HubMinifiedRestRespository>();
        #endregion

        #region FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CustomerValidator>();
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
        #endregion

        #region Redis
        var redisConnectionString = configuration.GetSection("Redis").Value ?? "";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "EcommerceMinified";
        });
        services.AddScoped<IRedisService, RedisService>();
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
        string queueProductInfo = configuration.GetSection("Bus:ProductInfoQueue").Value ?? "";

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(busConnectionstring), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.Publish<ProductInfoCommand>();
            });
        });

        EndpointConvention.Map<ProductInfoCommand>(new Uri($"{busConnectionstring}/{queueProductInfo}"));

        #endregion
    }
}
