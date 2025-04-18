using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.DAL
{
    public class DatabaseManager
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DatabaseManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public GabIADbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<GabIADbContext>();
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));

            return new GabIADbContext(optionsBuilder.Options);
        }

        public void InitializeDatabase()
        {
            using (var context = CreateDbContext())
            {
                DbInitializer.Initialize(context);
            }
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                using (var context = CreateDbContext())
                {
                    return await context.Database.CanConnectAsync();
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EnsureDatabaseCreated()
        {
            try
            {
                using (var context = CreateDbContext())
                {
                    await context.Database.EnsureCreatedAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> MigrateDatabase()
        {
            try
            {
                using (var context = CreateDbContext())
                {
                    await context.Database.MigrateAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
