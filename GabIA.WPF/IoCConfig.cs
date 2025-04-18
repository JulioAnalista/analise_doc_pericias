using Autofac;
using GabIA.DAL;
using GabIA.BLL;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace GabIA.WPF
{
    public class IoCConfig
    {
        public static IContainer ConfigureContainer(IConfigurationRoot configuration)
        {
            var builder = new ContainerBuilder();

            // Registre suas classes aqui
            // Exemplo:
            builder.RegisterType<ProcessoDAL>().AsSelf();
            builder.RegisterType<ProcessoBLL>().AsSelf();

            // Registre a classe MainWindow
            builder.RegisterType<MainWindow>().AsSelf();

            // Registre a connectionString
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.RegisterInstance(connectionString).As<string>();

            // Registrar WpfPrincipal
            builder.RegisterType<wpfPrincipal>();

            return builder.Build();

            // Cria e retorna o contêiner
        }
    }
}


/*

using Autofac;
using GabIA.DAL;
using GabIA.BLL;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace GabIA.WPF
{
    public class IoCConfig
    {
        public static IContainer ConfigureContainer(IConfigurationRoot configuration)
        {
            var builder = new ContainerBuilder();

            // Registre suas classes aqui
            // Exemplo:
            builder.RegisterType<ProcessoDAL>().AsSelf();
            builder.RegisterType<ProcessoBLL>().AsSelf();

            // Registre outros componentes conforme necessário
            // Por exemplo, você pode registrar uma string de conexão do banco de dados como um parâmetro
            // Registre a classe MainWindow
            builder.RegisterType<MainWindow>().AsSelf();

            // Cria e retorna o contêiner
            return builder.Build();
        }
    }
}
/**/
