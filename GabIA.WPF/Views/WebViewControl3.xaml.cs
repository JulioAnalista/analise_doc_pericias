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

namespace GabIA.WPF.Views
{
    public partial class WebViewControl3 : UserControl
    {
        public WebViewControl3()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await webView2.EnsureCoreWebView2Async(null);
            webView2.NavigationStarting += WebView2_NavigationStarting;
            webView2.NavigationCompleted += WebView2_NavigationCompleted;
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Console.WriteLine("WebView2 navigation is starting.");
        }

        private async void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Definir o nível de zoom desejado, por exemplo, 0.5 para 50% de zoom
            double zoomLevel = 0.15;
            string script = $"document.body.style.zoom = '{zoomLevel}';";
            await webView2.ExecuteScriptAsync(script);
        }
    }
}