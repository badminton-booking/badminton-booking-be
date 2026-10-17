using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SampleService = be_booking_badminton.Application.Services.Sample;

namespace be_booking_badminton.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<SampleService.IService, SampleService.Service>();

        return services;
    }
}
