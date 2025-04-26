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
using System.Globalization;
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
            LayerNames = GetLayerNames();
            SelectedLayer = LayerNamesWithCurrent.FirstOrDefault();
        }
        //0426加combobox颜色框
        public ObservableCollection<LayerInfo> LayerNamesWithCurrent { get; } = new ObservableCollection<LayerInfo>();
        private ObservableCollection<LayerInfo> _layerNames;
        public ObservableCollection<LayerInfo> LayerNames
        {
            get => _layerNames;
            set
            {
                _layerNames = value;
                UpdateLayerNamesWithCurrent();
                OnPropertyChanged(nameof(LayerNames));
            }
        }
        // 当前选中项
        private LayerInfo _selectedLayer;
        public LayerInfo SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                _selectedLayer = value;
                OnPropertyChanged(nameof(SelectedLayer));
                HandleLayerSelection(value);
            }
        }
        private void UpdateLayerNamesWithCurrent()
        {
            LayerNamesWithCurrent.Clear();
            LayerNamesWithCurrent.Add(new LayerInfo
            {
                Name = "使用当前图层",
                Color = Colors.Transparent,
                IsCurrentLayer = true
            });
            foreach (var item in _layerNames)
            {
                LayerNamesWithCurrent.Add(item);
            }
        }
        private ObservableCollection<LayerInfo> GetLayerNames()
        {
            ObservableCollection<LayerInfo> layerList = new ObservableCollection<LayerInfo>();
            using (Transaction trans = Database.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                lt.GenerateUsageData();
                // 获取当前图层ID
                ObjectId currentLayerId = Database.Clayer;
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = item.GetObject(OpenMode.ForRead) as LayerTableRecord;
                    layerList.Add(new LayerInfo
                    {
                        Name = ltr.Name,
                        Color = LayerInfo.GetWpfColor(ltr.Color),
                        IsCurrentLayer = (item == currentLayerId)
                    });
                }
                trans.Commit();
            }
            return layerList;
        }
        private void HandleLayerSelection(LayerInfo selectedLayer)
        {
            if (selectedLayer == null) return;
            if (selectedLayer.IsCurrentLayer)
            {
                //// 处理"使用当前图层"逻辑
                //var currentLayerName = GetCurrentLayerName();
                //Document.Editor.WriteMessage($"\n将使用当前图层: {currentLayerName}");
            }
            else
            {
                // 处理普通图层选择
                Document.Editor.WriteMessage($"\n已选择图层: {selectedLayer.Name}");
            }
        }

        //0425
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
        ////public List<string> LayerNames { get; set; }
        public ICommand GetSelectionCommand => new BaseBindingCommand(GetSelection);
        private void GetSelection(object obj)
        {
            Editor ed = Document.Editor;
            //ed.WriteMessage(SelectedLayer);
            ed.WriteMessage(SelectedLayer.Name+"\n"+SelectedLayer.Color.G.ToString());
        }
        public string Font { get; set; } = "宋体";
    }
    public class LayerInfo : ObserverableObject
    {
        public string Name { get; set; }
        //public Color Color { get; set; }
        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        public bool IsCurrentLayer { get; set; }
        // 将 AutoCAD 颜色索引转换为 WPF Color
        public static Color GetWpfColor(Autodesk.AutoCAD.Colors.Color acColor)
        {
            if (acColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByAci)
            {
                return GetRGBFromACI((short)acColor.ColorIndex);
            }
            else if (acColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByColor)
            {
                // 处理真彩色
                return Color.FromRgb(acColor.Red, acColor.Green, acColor.Blue);
            }
            return Colors.White; // 默认颜色
        }
        public static Color GetRGBFromACI(short colorIndex)
        {
            Color color = new Color();
            var lookup = ACItoRGBLookup();
            //lookup.TryGetValue(colorIndex, out List<short> rgbValues);
            if (lookup.TryGetValue(colorIndex, out List<short> rgbValues))
            {
                color.R = (byte)rgbValues[0];
                color.G = (byte)rgbValues[1];
                color.B = (byte)rgbValues[2];
            }
            // 如果找不到，返回默认黑色
            return color;
        }
        public short GetACIfromRGB(short r, short g, short b)
        {
            Dictionary<short, List<short>> lookup = ACItoRGBLookup();
            //now get color using shortest dist calc and see if it matches
            double minDist = double.MaxValue;
            short match = 0; // this will end up with our answer
            for (short i = 1; i <= 255; ++i)
            {
                List<short> lookupcolor = lookup[i];
                double dist =
                    Math.Pow(r - lookupcolor[0], 2) +
                    Math.Pow(g - lookupcolor[1], 2) +
                    Math.Pow(b - lookupcolor[2], 2);
                if (dist < minDist)
                {
                    minDist = dist;
                    match = i;
                }
            }
            return match;
        }
        public static Dictionary<short, List<short>> ACItoRGBLookup()
        {
            Dictionary<short, List<short>> ret = new Dictionary<short, List<short>>();
            ret.Add(1, new List<short>() { 255, 0, 0 });
            ret.Add(2, new List<short>() { 255, 255, 0 });
            ret.Add(3, new List<short>() { 0, 255, 0 });
            ret.Add(4, new List<short>() { 0, 255, 255 });
            ret.Add(5, new List<short>() { 0, 0, 255 });
            ret.Add(6, new List<short>() { 255, 0, 255 });
            ret.Add(7, new List<short>() { 255, 255, 255 });
            ret.Add(8, new List<short>() { 128, 128, 128 });
            ret.Add(9, new List<short>() { 192, 192, 192 });
            ret.Add(10, new List<short>() { 255, 0, 0 });
            ret.Add(11, new List<short>() { 255, 127, 127 });
            ret.Add(12, new List<short>() { 204, 0, 0 });
            ret.Add(13, new List<short>() { 204, 102, 102 });
            ret.Add(14, new List<short>() { 153, 0, 0 });
            ret.Add(15, new List<short>() { 153, 76, 76 });
            ret.Add(16, new List<short>() { 127, 0, 0 });
            ret.Add(17, new List<short>() { 127, 63, 63 });
            ret.Add(18, new List<short>() { 76, 0, 0 });
            ret.Add(19, new List<short>() { 76, 38, 38 });
            ret.Add(20, new List<short>() { 255, 63, 0 });
            ret.Add(21, new List<short>() { 255, 159, 127 });
            ret.Add(22, new List<short>() { 204, 51, 0 });
            ret.Add(23, new List<short>() { 204, 127, 102 });
            ret.Add(24, new List<short>() { 153, 38, 0 });
            ret.Add(25, new List<short>() { 153, 95, 76 });
            ret.Add(26, new List<short>() { 127, 31, 0 });
            ret.Add(27, new List<short>() { 127, 79, 63 });
            ret.Add(28, new List<short>() { 76, 19, 0 });
            ret.Add(29, new List<short>() { 76, 47, 38 });
            ret.Add(30, new List<short>() { 255, 127, 0 });
            ret.Add(31, new List<short>() { 255, 191, 127 });
            ret.Add(32, new List<short>() { 204, 102, 0 });
            ret.Add(33, new List<short>() { 204, 153, 102 });
            ret.Add(34, new List<short>() { 153, 76, 0 });
            ret.Add(35, new List<short>() { 153, 114, 76 });
            ret.Add(36, new List<short>() { 127, 63, 0 });
            ret.Add(37, new List<short>() { 127, 95, 63 });
            ret.Add(38, new List<short>() { 76, 38, 0 });
            ret.Add(39, new List<short>() { 76, 57, 38 });
            ret.Add(40, new List<short>() { 255, 191, 0 });
            ret.Add(41, new List<short>() { 255, 223, 127 });
            ret.Add(42, new List<short>() { 204, 153, 0 });
            ret.Add(43, new List<short>() { 204, 178, 102 });
            ret.Add(44, new List<short>() { 153, 114, 0 });
            ret.Add(45, new List<short>() { 153, 133, 76 });
            ret.Add(46, new List<short>() { 127, 95, 0 });
            ret.Add(47, new List<short>() { 127, 111, 63 });
            ret.Add(48, new List<short>() { 76, 57, 0 });
            ret.Add(49, new List<short>() { 76, 66, 38 });
            ret.Add(50, new List<short>() { 255, 255, 0 });
            ret.Add(51, new List<short>() { 255, 255, 127 });
            ret.Add(52, new List<short>() { 204, 204, 0 });
            ret.Add(53, new List<short>() { 204, 204, 102 });
            ret.Add(54, new List<short>() { 153, 153, 0 });
            ret.Add(55, new List<short>() { 153, 153, 76 });
            ret.Add(56, new List<short>() { 127, 127, 0 });
            ret.Add(57, new List<short>() { 127, 127, 63 });
            ret.Add(58, new List<short>() { 76, 76, 0 });
            ret.Add(59, new List<short>() { 76, 76, 38 });
            ret.Add(60, new List<short>() { 191, 255, 0 });
            ret.Add(61, new List<short>() { 223, 255, 127 });
            ret.Add(62, new List<short>() { 153, 204, 0 });
            ret.Add(63, new List<short>() { 178, 204, 102 });
            ret.Add(64, new List<short>() { 114, 153, 0 });
            ret.Add(65, new List<short>() { 133, 153, 76 });
            ret.Add(66, new List<short>() { 95, 127, 0 });
            ret.Add(67, new List<short>() { 111, 127, 63 });
            ret.Add(68, new List<short>() { 57, 76, 0 });
            ret.Add(69, new List<short>() { 66, 76, 38 });
            ret.Add(70, new List<short>() { 127, 255, 0 });
            ret.Add(71, new List<short>() { 191, 255, 127 });
            ret.Add(72, new List<short>() { 102, 204, 0 });
            ret.Add(73, new List<short>() { 153, 204, 102 });
            ret.Add(74, new List<short>() { 76, 153, 0 });
            ret.Add(75, new List<short>() { 114, 153, 76 });
            ret.Add(76, new List<short>() { 63, 127, 0 });
            ret.Add(77, new List<short>() { 95, 127, 63 });
            ret.Add(78, new List<short>() { 38, 76, 0 });
            ret.Add(79, new List<short>() { 57, 76, 38 });
            ret.Add(80, new List<short>() { 63, 255, 0 });
            ret.Add(81, new List<short>() { 159, 255, 127 });
            ret.Add(82, new List<short>() { 51, 204, 0 });
            ret.Add(83, new List<short>() { 127, 204, 102 });
            ret.Add(84, new List<short>() { 38, 153, 0 });
            ret.Add(85, new List<short>() { 95, 153, 76 });
            ret.Add(86, new List<short>() { 31, 127, 0 });
            ret.Add(87, new List<short>() { 79, 127, 63 });
            ret.Add(88, new List<short>() { 19, 76, 0 });
            ret.Add(89, new List<short>() { 47, 76, 38 });
            ret.Add(90, new List<short>() { 0, 255, 0 });
            ret.Add(91, new List<short>() { 127, 255, 127 });
            ret.Add(92, new List<short>() { 0, 204, 0 });
            ret.Add(93, new List<short>() { 102, 204, 102 });
            ret.Add(94, new List<short>() { 0, 153, 0 });
            ret.Add(95, new List<short>() { 76, 153, 76 });
            ret.Add(96, new List<short>() { 0, 127, 0 });
            ret.Add(97, new List<short>() { 63, 127, 63 });
            ret.Add(98, new List<short>() { 0, 76, 0 });
            ret.Add(99, new List<short>() { 38, 76, 38 });
            ret.Add(100, new List<short>() { 0, 255, 63 });
            ret.Add(101, new List<short>() { 127, 255, 159 });
            ret.Add(102, new List<short>() { 0, 204, 51 });
            ret.Add(103, new List<short>() { 102, 204, 127 });
            ret.Add(104, new List<short>() { 0, 153, 38 });
            ret.Add(105, new List<short>() { 76, 153, 95 });
            ret.Add(106, new List<short>() { 0, 127, 31 });
            ret.Add(107, new List<short>() { 63, 127, 79 });
            ret.Add(108, new List<short>() { 0, 76, 19 });
            ret.Add(109, new List<short>() { 38, 76, 47 });
            ret.Add(110, new List<short>() { 0, 255, 127 });
            ret.Add(111, new List<short>() { 127, 255, 191 });
            ret.Add(112, new List<short>() { 0, 204, 102 });
            ret.Add(113, new List<short>() { 102, 204, 153 });
            ret.Add(114, new List<short>() { 0, 153, 76 });
            ret.Add(115, new List<short>() { 76, 153, 114 });
            ret.Add(116, new List<short>() { 0, 127, 63 });
            ret.Add(117, new List<short>() { 63, 127, 95 });
            ret.Add(118, new List<short>() { 0, 76, 38 });
            ret.Add(119, new List<short>() { 38, 76, 57 });
            ret.Add(120, new List<short>() { 0, 255, 191 });
            ret.Add(121, new List<short>() { 127, 255, 223 });
            ret.Add(122, new List<short>() { 0, 204, 153 });
            ret.Add(123, new List<short>() { 102, 204, 178 });
            ret.Add(124, new List<short>() { 0, 153, 114 });
            ret.Add(125, new List<short>() { 76, 153, 133 });
            ret.Add(126, new List<short>() { 0, 127, 95 });
            ret.Add(127, new List<short>() { 63, 127, 111 });
            ret.Add(128, new List<short>() { 0, 76, 57 });
            ret.Add(129, new List<short>() { 38, 76, 66 });
            ret.Add(130, new List<short>() { 0, 255, 255 });
            ret.Add(131, new List<short>() { 127, 255, 255 });
            ret.Add(132, new List<short>() { 0, 204, 204 });
            ret.Add(133, new List<short>() { 102, 204, 204 });
            ret.Add(134, new List<short>() { 0, 153, 153 });
            ret.Add(135, new List<short>() { 76, 153, 153 });
            ret.Add(136, new List<short>() { 0, 127, 127 });
            ret.Add(137, new List<short>() { 63, 127, 127 });
            ret.Add(138, new List<short>() { 0, 76, 76 });
            ret.Add(139, new List<short>() { 38, 76, 76 });
            ret.Add(140, new List<short>() { 0, 191, 255 });
            ret.Add(141, new List<short>() { 127, 223, 255 });
            ret.Add(142, new List<short>() { 0, 153, 204 });
            ret.Add(143, new List<short>() { 102, 178, 204 });
            ret.Add(144, new List<short>() { 0, 114, 153 });
            ret.Add(145, new List<short>() { 76, 133, 153 });
            ret.Add(146, new List<short>() { 0, 95, 127 });
            ret.Add(147, new List<short>() { 63, 111, 127 });
            ret.Add(148, new List<short>() { 0, 57, 76 });
            ret.Add(149, new List<short>() { 38, 66, 76 });
            ret.Add(150, new List<short>() { 0, 127, 255 });
            ret.Add(151, new List<short>() { 127, 191, 255 });
            ret.Add(152, new List<short>() { 0, 102, 204 });
            ret.Add(153, new List<short>() { 102, 153, 204 });
            ret.Add(154, new List<short>() { 0, 76, 153 });
            ret.Add(155, new List<short>() { 76, 114, 153 });
            ret.Add(156, new List<short>() { 0, 63, 127 });
            ret.Add(157, new List<short>() { 63, 95, 127 });
            ret.Add(158, new List<short>() { 0, 38, 76 });
            ret.Add(159, new List<short>() { 38, 57, 76 });
            ret.Add(160, new List<short>() { 0, 63, 255 });
            ret.Add(161, new List<short>() { 127, 159, 255 });
            ret.Add(162, new List<short>() { 0, 51, 204 });
            ret.Add(163, new List<short>() { 102, 127, 204 });
            ret.Add(164, new List<short>() { 0, 38, 153 });
            ret.Add(165, new List<short>() { 76, 95, 153 });
            ret.Add(166, new List<short>() { 0, 31, 127 });
            ret.Add(167, new List<short>() { 63, 79, 127 });
            ret.Add(168, new List<short>() { 0, 19, 76 });
            ret.Add(169, new List<short>() { 38, 47, 76 });
            ret.Add(170, new List<short>() { 0, 0, 255 });
            ret.Add(171, new List<short>() { 127, 127, 255 });
            ret.Add(172, new List<short>() { 0, 0, 204 });
            ret.Add(173, new List<short>() { 102, 102, 204 });
            ret.Add(174, new List<short>() { 0, 0, 153 });
            ret.Add(175, new List<short>() { 76, 76, 153 });
            ret.Add(176, new List<short>() { 0, 0, 127 });
            ret.Add(177, new List<short>() { 63, 63, 127 });
            ret.Add(178, new List<short>() { 0, 0, 76 });
            ret.Add(179, new List<short>() { 38, 38, 76 });
            ret.Add(180, new List<short>() { 63, 0, 255 });
            ret.Add(181, new List<short>() { 159, 127, 255 });
            ret.Add(182, new List<short>() { 51, 0, 204 });
            ret.Add(183, new List<short>() { 127, 102, 204 });
            ret.Add(184, new List<short>() { 38, 0, 153 });
            ret.Add(185, new List<short>() { 95, 76, 153 });
            ret.Add(186, new List<short>() { 31, 0, 127 });
            ret.Add(187, new List<short>() { 79, 63, 127 });
            ret.Add(188, new List<short>() { 19, 0, 76 });
            ret.Add(189, new List<short>() { 47, 38, 76 });
            ret.Add(190, new List<short>() { 127, 0, 255 });
            ret.Add(191, new List<short>() { 191, 127, 255 });
            ret.Add(192, new List<short>() { 102, 0, 204 });
            ret.Add(193, new List<short>() { 153, 102, 204 });
            ret.Add(194, new List<short>() { 76, 0, 153 });
            ret.Add(195, new List<short>() { 114, 76, 153 });
            ret.Add(196, new List<short>() { 63, 0, 127 });
            ret.Add(197, new List<short>() { 95, 63, 127 });
            ret.Add(198, new List<short>() { 38, 0, 76 });
            ret.Add(199, new List<short>() { 57, 38, 76 });
            ret.Add(200, new List<short>() { 191, 0, 255 });
            ret.Add(201, new List<short>() { 223, 127, 255 });
            ret.Add(202, new List<short>() { 153, 0, 204 });
            ret.Add(203, new List<short>() { 178, 102, 204 });
            ret.Add(204, new List<short>() { 114, 0, 153 });
            ret.Add(205, new List<short>() { 133, 76, 153 });
            ret.Add(206, new List<short>() { 95, 0, 127 });
            ret.Add(207, new List<short>() { 111, 63, 127 });
            ret.Add(208, new List<short>() { 57, 0, 76 });
            ret.Add(209, new List<short>() { 66, 38, 76 });
            ret.Add(210, new List<short>() { 255, 0, 255 });
            ret.Add(211, new List<short>() { 255, 127, 255 });
            ret.Add(212, new List<short>() { 204, 0, 204 });
            ret.Add(213, new List<short>() { 204, 102, 204 });
            ret.Add(214, new List<short>() { 153, 0, 153 });
            ret.Add(215, new List<short>() { 153, 76, 153 });
            ret.Add(216, new List<short>() { 127, 0, 127 });
            ret.Add(217, new List<short>() { 127, 63, 127 });
            ret.Add(218, new List<short>() { 76, 0, 76 });
            ret.Add(219, new List<short>() { 76, 38, 76 });
            ret.Add(220, new List<short>() { 255, 0, 191 });
            ret.Add(221, new List<short>() { 255, 127, 223 });
            ret.Add(222, new List<short>() { 204, 0, 153 });
            ret.Add(223, new List<short>() { 204, 102, 178 });
            ret.Add(224, new List<short>() { 153, 0, 114 });
            ret.Add(225, new List<short>() { 153, 76, 133 });
            ret.Add(226, new List<short>() { 127, 0, 95 });
            ret.Add(227, new List<short>() { 127, 63, 111 });
            ret.Add(228, new List<short>() { 76, 0, 57 });
            ret.Add(229, new List<short>() { 76, 38, 66 });
            ret.Add(230, new List<short>() { 255, 0, 127 });
            ret.Add(231, new List<short>() { 255, 127, 191 });
            ret.Add(232, new List<short>() { 204, 0, 102 });
            ret.Add(233, new List<short>() { 204, 102, 153 });
            ret.Add(234, new List<short>() { 153, 0, 76 });
            ret.Add(235, new List<short>() { 153, 76, 114 });
            ret.Add(236, new List<short>() { 127, 0, 63 });
            ret.Add(237, new List<short>() { 127, 63, 95 });
            ret.Add(238, new List<short>() { 76, 0, 38 });
            ret.Add(239, new List<short>() { 76, 38, 57 });
            ret.Add(240, new List<short>() { 255, 0, 63 });
            ret.Add(241, new List<short>() { 255, 127, 159 });
            ret.Add(242, new List<short>() { 204, 0, 51 });
            ret.Add(243, new List<short>() { 204, 102, 127 });
            ret.Add(244, new List<short>() { 153, 0, 38 });
            ret.Add(245, new List<short>() { 153, 76, 95 });
            ret.Add(246, new List<short>() { 127, 0, 31 });
            ret.Add(247, new List<short>() { 127, 63, 79 });
            ret.Add(248, new List<short>() { 76, 0, 19 });
            ret.Add(249, new List<short>() { 76, 38, 47 });
            ret.Add(250, new List<short>() { 51, 51, 51 });
            ret.Add(251, new List<short>() { 91, 91, 91 });
            ret.Add(252, new List<short>() { 132, 132, 132 });
            ret.Add(253, new List<short>() { 173, 173, 173 });
            ret.Add(254, new List<short>() { 214, 214, 214 });
            ret.Add(255, new List<short>() { 255, 255, 255 });
            return ret;
        }
    }
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? FontWeights.Bold : FontWeights.Normal;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
