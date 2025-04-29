using Autodesk.AutoCAD.ApplicationServices;
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
using System.Windows.Shapes;

namespace ACTest
{
    /// <summary>
    /// TableTemplateViewHorizon.xaml 的交互逻辑
    /// </summary>
    public partial class TableTemplateViewHorizon : Window
    {
        public TableTemplateViewHorizon(Document document)
        {
            InitializeComponent();
            this.DataContext = new TableTemplateViewModel(document);
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
