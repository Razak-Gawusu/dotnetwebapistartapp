using System.Reflection;
using DbUp;

namespace Innout.Persistence.Database;

public static class DbInitializer
{
    public static void Initializer(string connectionString)
    {
        // ensure the postgres Database exists
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
        .PostgresqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .WithTransaction()
        .LogToConsole()
        .Build();

        // run scripts by order
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}