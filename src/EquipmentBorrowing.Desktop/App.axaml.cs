using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Infrastructure;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<App>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EquipmentBorrowingDbContext>();
                Task.Run(() => DatabaseSeeder.SeedAsync(dbContext)).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // Database unavailable at startup: the views report load errors and allow retry.
            }

            var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            // Load the first screen's data on startup.
            mainWindowViewModel.ShowEquipmentCommand.Execute(null);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(ServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("EquipmentBorrowing")
            ?? throw new InvalidOperationException(
                "Missing connection string 'EquipmentBorrowing'. " +
                "Set it with dotnet user-secrets or the ConnectionStrings__EquipmentBorrowing " +
                "environment variable (see README).");

        services.AddDbContext<EquipmentBorrowingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IStudentRepository, EfStudentRepository>();
        services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
        services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();

        services.AddScoped<BorrowEquipmentService>();
        services.AddScoped<ReturnEquipmentService>();
        services.AddScoped<StudentManagementService>();
        services.AddScoped<EquipmentManagementService>();

        services.AddScoped<CheckOutMultipleService>();
        services.AddSingleton<IFaultInjector, EnvironmentFaultInjector>();
        services.AddScoped<IBorrowingQueryRepository, EfBorrowingQueryRepository>();
        services.AddScoped<BorrowingQueryService>();

        services.AddSingleton<IUseCaseExecutor, UseCaseExecutor>();

        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddTransient<StudentsViewModel>();
        services.AddTransient<EquipmentManagementViewModel>();
        services.AddTransient<BorrowingSearchViewModel>();
        services.AddTransient<HistoryViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}