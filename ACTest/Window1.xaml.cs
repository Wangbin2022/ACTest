using ACTest.Base;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        Database Database;
        public Window1(Document document)
        {
            InitializeComponent();
            this.DataContext = new ViewModel(document);
            Database = document.Database;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ////0306 用户交互
            ////Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //PromptPointOptions ppo = new PromptPointOptions("input point");
            //ppo.AllowNone = true;
            //PromptPointResult ppr = GetPoint(ppo);
            //Point3d p1 = new Point3d(0, 0, 0);
            //Point3d p2 = new Point3d();
            //if (ppr.Status == PromptStatus.Cancel) return;
            //if (ppr.Status == PromptStatus.OK) p1 = ppr.Value;
            //ppo.Message = "2nd Point input.";
            //ppo.BasePoint = p1;
            //ppo.UseBasePoint = true;
            //ppr = GetPoint(ppo);
            //if (ppr.Status == PromptStatus.Cancel) return;
            //if (ppr.Status == PromptStatus.None) return;
            //if (ppr.Status == PromptStatus.OK) p2 = ppr.Value;
            //AddLineToModelSpace(Database, p1, p2);
            ////例程结束
        }

        //public PromptPointResult GetPoint(PromptPointOptions ppo)
        //{
        //    Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //    ppo.AllowNone = true;
        //    return ed.GetPoint(ppo);
        //}
        //public ObjectId AddEntityToModelSpace(Database db, Entity ent)
        //{
        //    ObjectId objectId = ObjectId.Null;
        //    using (Transaction trans = db.TransactionManager.StartTransaction())
        //    {
        //        BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
        //        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        //        objectId = btr.AppendEntity(ent);
        //        trans.AddNewlyCreatedDBObject(ent, true);
        //        trans.Commit();
        //    }
        //    return objectId;
        //}
        //public ObjectId AddLineToModelSpace(Database db, Point3d startPoint, Point3d endPoint)
        //{
        //    return AddEntityToModelSpace(db, new Autodesk.AutoCAD.DatabaseServices.Line(startPoint, endPoint));
        //}
    }

    public class ViewModel : ObserverableObject
    {
        Database Database;
        Document Document;
        public ViewModel(Document document)
        {
            Document = document;
            Database = document.Database;
            //Editor ed = document.Editor;
            //ed.WriteMessage("New20250420,222");
            //LayerNames = GetLayerNames();
        }
        //public ObservableCollection<string> LayerNamesWithCurrent { get; } = new ObservableCollection<string>();
        //private ObservableCollection<string> _layerNames;
        //public ObservableCollection<string> LayerNames
        //{
        //    get => _layerNames;
        //    set
        //    {
        //        _layerNames = value;
        //        LayerNamesWithCurrent.Clear();
        //        LayerNamesWithCurrent.Add("使用当前图层");
        //        foreach (var item in _layerNames)
        //        {
        //            LayerNamesWithCurrent.Add(item);
        //        }
        //        OnPropertyChanged(nameof(LayerNames));
        //        OnPropertyChanged(nameof(LayerNamesWithCurrent));
        //    }
        //}
        //// 当前选中项
        //private string _selectedLayer = "使用当前图层";
        //public string SelectedLayer
        //{
        //    get => _selectedLayer;
        //    set
        //    {
        //        _selectedLayer = value;
        //        OnPropertyChanged(nameof(SelectedLayer));
        //    }
        //}
        ////SelectedItem="使用当前图层"
        //private ObservableCollection<string> GetLayerNames()
        //{
        //    ObservableCollection<string> layerList = new ObservableCollection<string>();
        //    using (Transaction trans = Database.TransactionManager.StartTransaction())
        //    {
        //        LayerTable lt = trans.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
        //        //要先对图层内容检查才能后续
        //        lt.GenerateUsageData();
        //        foreach (ObjectId item in lt)
        //        {
        //            LayerTableRecord ltr = item.GetObject(OpenMode.ForRead) as LayerTableRecord;
        //            layerList.Add(ltr.Name);
        //        }
        //        trans.Commit();
        //    }
        //    return layerList;
        //}
        //public List<string> LayerNames { get; set; } 
        public ICommand GetSelectionCommand => new BaseBindingCommand(GetSelection);
        private void GetSelection(object obj)
        {
            Editor ed = Document.Editor;
            //ed.WriteMessage(SelectedLayer);
        }
        public string Font { get; set; } = "宋体";
    }
    public class LayerInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsCurrentLayer { get; set; }

        // 将 AutoCAD 颜色索引转换为 WPF Color
        //public static Color GetWpfColor(Autodesk.AutoCAD.Colors.Color acColor)
        //{
        //    if (acColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByAci)
        //    {
        //        // 处理索引颜色
        //        var sysColorR = Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb(((byte)acColor.ColorIndex));
        //        var sysColorG = Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb(((byte)acColor.ColorIndex));
        //        var sysColorB = Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb(((byte)acColor.ColorIndex));
        //        return Color.FromRgb(sysColorR, sysColorG, sysColorB);
        //    }
        //    else if (acColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByColor)
        //    {
        //        // 处理真彩色
        //        return Color.FromRgb(acColor.Red, acColor.Green, acColor.Blue);
        //    }
        //    return Colors.Black; // 默认颜色
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
