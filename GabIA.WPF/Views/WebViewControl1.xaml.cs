using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.WinForms;

namespace GabIA.WPF.Views
{
    public partial class WebViewControl1 : UserControl
    {
        public bool IsInitialized { get; private set; } = false;

        public WebViewControl1()
        {
            InitializeComponent();
            InitializeAsync();
        }


        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.NavigationStarting += WebView_NavigationStarting;
            webView.NavigationCompleted += WebView_NavigationCompleted;
        }


        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Console.WriteLine("Navigation is starting.");
        }
        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Definir o nível de zoom desejado, por exemplo, 0.5 para 50% de zoom
            double zoomLevel = 0.25;
            string script = $"document.body.style.zoom = '{zoomLevel}';";
            await webView.ExecuteScriptAsync(script);
        }
    }
}

