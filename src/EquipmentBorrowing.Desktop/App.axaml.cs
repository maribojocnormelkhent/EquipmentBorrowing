using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            var mainWindowViewModel = provider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // Seed data, kept as singletons so state (who borrowed what) is
        // preserved as the user navigates between views.
        var seedStudents = new List<Student>
        {
            new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true),
            new Student(2, "Maria Santos", isAllowedToBorrow: false),
        };

        var seedEquipment = new List<Equipment>
        {
            new Equipment(100, "Digital Multimeter", isAvailable: true),
            new Equipment(101, "Oscilloscope", isAvailable: true),
            new Equipment(102, "Function Generator", isAvailable: true),
        };

        services.AddSingleton<IStudentRepository>(new InMemoryStudentRepository(seedStudents));
        services.AddSingleton<IEquipmentRepository>(new InMemoryEquipmentRepository(seedEquipment));
        services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();

        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}