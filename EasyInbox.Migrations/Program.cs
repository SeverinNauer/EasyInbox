using System;
using EasyInbox.Migrations.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace EasyInbox.Migrations
{
    //TODO Create Docker file
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = CreateServices();

            using (var scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }
        }
        private static IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                            // Add common FluentMigrator services
                            .AddFluentMigratorCore()
                            .ConfigureRunner(rb => rb
                                // Add SQLite support to FluentMigrator
                                .AddPostgres()
                                // Set the connection string
                                .WithGlobalConnectionString("Server = 127.0.0.1; Port = 5432; Database = easyinbox; User Id = postgres; Password = admin") //TODO Store as user secret
                                            // Define the assembly containing the migrations
                                            .ScanIn(typeof(Initialize).Assembly).For.Migrations())
                            // Enable logging to console in the FluentMigrator way
                            .AddLogging(lb => lb.AddFluentMigratorConsole())
                            // Build the service provider
                            .BuildServiceProvider(false);
        }

        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
            //runner.MigrateDown(20201024084300);
        }
    }
}
