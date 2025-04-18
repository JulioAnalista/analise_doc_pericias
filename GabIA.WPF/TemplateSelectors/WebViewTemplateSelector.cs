using GabIA.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace GabIA.WPF.TemplateSelectors
{
    public class WebViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WebViewControl1Template { get; set; }
        public DataTemplate WebViewControl2Template { get; set; }
        public DataTemplate WebViewControl3Template { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is WebViewControl1ViewModel)
                return WebViewControl1Template;
            if (item is WebViewControl2ViewModel)
                return WebViewControl2Template;
            if (item is WebViewControl3ViewModel)
                return WebViewControl3Template;

            return base.SelectTemplate(item, container);
        }
    }
}
