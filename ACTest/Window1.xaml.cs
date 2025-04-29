using ACTest.Base;
using ACTest.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
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
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Line = Autodesk.AutoCAD.DatabaseServices.Line;

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
            //cmbColors.ItemsSource = typeof(Colors).GetProperties();
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
        //0429 插入文字位置测试
        public static DBText AddTextToCell(Database db, Transaction tr, Point3d insertionPoint, string text, double cellWidth,
TextAlignment alignment, string textStyle = "Standard", string layer = "0", double textHeight = 2.5)
        {
            // 处理空文本
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "-";
            }
            // 计算对齐偏移量
            double offsetX = 0;
            switch (alignment)
            {
                case TextAlignment.Center:
                    offsetX = 0;
                    break;
                case TextAlignment.Left:
                default:
                    offsetX = textHeight / 2; // 左边距为文字高度的一半
                    break;
            }
            // 调整插入点
            Point3d adjustedPoint = new Point3d(insertionPoint.X + offsetX, insertionPoint.Y, insertionPoint.Z);
            // 创建文本对象
            DBText dbText = new DBText
            {
                //Position = adjustedPoint,
                Position = insertionPoint,
                TextString = text,
                Height = textHeight,
                Layer = layer,
                Justify = AttachmentPoint.MiddleCenter // 垂直居中
            };
            dbText.AlignmentPoint =dbText.Position;//默认放到原点
            if (alignment== TextAlignment.Left)
            {
                dbText.HorizontalMode = TextHorizontalMode.TextLeft;
            }
            // 设置文本样式
            TextStyleTable st = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            if (st.Has(textStyle))
            {
                dbText.TextStyleId = st[textStyle];
            }
            // 添加到模型空间
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            btr.AppendEntity(dbText);
            tr.AddNewlyCreatedDBObject(dbText, true);
            MessageBox.Show(insertionPoint.X.ToString() + "\n" + insertionPoint.Y.ToString() + "\n" + insertionPoint.Z.ToString());
            return dbText;
        }
        public enum TextAlignment { Left, Center }
        public ICommand GetWordCommand => new BaseBindingCommand(GetWord);
        private void GetWord(object obj)
        {
            using (Transaction tr = Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                // 确保文本样式存在
                TextStyleTable st = (TextStyleTable)tr.GetObject(Database.TextStyleTableId, OpenMode.ForRead);
                TextStyleTableRecord tstr = new TextStyleTableRecord();
                if (!st.Has(Font))
                {
                    tstr.Name = Font;
                    tstr.Font = new FontDescriptor(Font, false, false, 0, 0);
                    tstr.XScale = 0.7;
                    st.UpgradeOpen();
                    st.Add(tstr);
                    tr.AddNewlyCreatedDBObject(tstr, true);
                    st.DowngradeOpen();
                }
                else
                {
                    // 修改已有文本样式 
                    ObjectId styleId = st[Font];
                    TextStyleTableRecord existingStyle = tr.GetObject(styleId, OpenMode.ForWrite) as TextStyleTableRecord;
                    // 更新样式属性
                    existingStyle.XScale = 0.7;  // 修改宽度因子
                    existingStyle.Font = new FontDescriptor(Font, false, false, 0, 0);
                    existingStyle.TextSize = 0;
                    existingStyle.IsVertical = false;
                    existingStyle.ObliquingAngle = 0;
                }
                Editor ed = Document.Editor;
                PromptPointResult ppr = ed.GetPoint("\n请选择表格左上角插入点: ");
                if (ppr.Status != PromptStatus.OK) return;
                Point3d insertionPoint = ppr.Value;

                // 计算单元格原点
                //double cellX = insertionPoint.X + GetColumnWidthSum(j, baseCellWidth, colWidthFactors);
                double cellX = insertionPoint.X + 2000;
                double cellY = insertionPoint.Y - 400;
                // 计算文本插入点（垂直居中）
                //Point3d textPoint = new Point3d(cellX, cellY - rowHeight / 2, insertionPoint.Z);
                //Point3d textPoint = new Point3d(cellX, cellY - 200, insertionPoint.Z);
                Point3d textPoint = insertionPoint;

                // 添加文本到单元格
                //AddTextToCell(Database, tr, textPoint, tableData[i, j], baseCellWidth * colWidthFactors[j], (TextAlignment)columnAlignments[j],
                //    "Standard", "0", TextSize * TableScale);
                AddTextToCell(Database, tr, textPoint, "testWord", 2000, TextAlignment.Center, "Standard", "0", 300);
                tr.Commit();
            }
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
                ObjectId currentLayerId = Database.Clayer;
                foreach (ObjectId item in lt)
                {
                    LayerTableRecord ltr = trans.GetObject(item, OpenMode.ForRead) as LayerTableRecord;
                    // 简单过滤：排除包含"|"的图层名(外部参照图层格式为"XrefName|LayerName")
                    if (!ltr.Name.Contains("|"))
                    {
                        layerList.Add(new LayerInfo
                        {
                            Name = ltr.Name,
                            Color = CADColorHelper.GetWpfColor(ltr.Color),
                            IsCurrentLayer = (item == currentLayerId)
                        });
                    }
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
        public ICommand GetSelectionCommand => new BaseBindingCommand(GetSelection);
        private void GetSelection(object obj)
        {
            Editor ed = Document.Editor;
            //ed.WriteMessage(SelectedLayer);
            //ed.WriteMessage(SelectedLayer.Name+"\n"+SelectedLayer.Color.G.ToString());
            Document doc = Document;
            Database db = Document.Database;
            try
            {
                // 1. 获取用户选择的插入点
                PromptPointResult ppr = ed.GetPoint("\n请选择表格左上角插入点: ");
                if (ppr.Status != PromptStatus.OK) return;
                Point3d insertionPoint = ppr.Value;
                // 2. 获取当前图层
                string currentLayer;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    currentLayer = (db.Clayer.GetObject(OpenMode.ForRead) as LayerTableRecord).Name;
                    tr.Commit();
                }

                // 3. 定义表格参数
                int rowCount = 8;
                int columnCount = 5;
                double BaseWidth = 100;
                double rowHeight = 5;

                // 4. 计算表格总尺寸
                double totalWidth = columnCount * BaseWidth;
                double totalHeight = rowCount * rowHeight;

                //创建表格线(直通)
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    // 6. 创建水平线
                    for (int i = 0; i <= rowCount; i++)
                    {
                        Point3d start = new Point3d(insertionPoint.X, insertionPoint.Y - (i * rowHeight), insertionPoint.Z);
                        Point3d end = new Point3d(insertionPoint.X + totalWidth, insertionPoint.Y - (i * rowHeight), insertionPoint.Z);
                        Line horizontalLine = new Line(start, end);
                        horizontalLine.Layer = currentLayer;
                        btr.AppendEntity(horizontalLine);
                        tr.AddNewlyCreatedDBObject(horizontalLine, true);
                    }
                    // 7. 创建垂直线
                    for (int j = 0; j <= columnCount; j++)
                    {
                        Point3d start = new Point3d(insertionPoint.X + (j * BaseWidth), insertionPoint.Y, insertionPoint.Z);
                        Point3d end = new Point3d(insertionPoint.X + (j * BaseWidth), insertionPoint.Y - totalHeight, insertionPoint.Z);
                        Line verticalLine = new Line(start, end);
                        verticalLine.Layer = currentLayer;
                        btr.AppendEntity(verticalLine);
                        tr.AddNewlyCreatedDBObject(verticalLine, true);
                    }
                    DrawOutLine(insertionPoint, totalWidth, totalHeight, tr, btr);
                    tr.Commit();
                }

                ////创建表格线（按单元格画）
                //List<Line> allLines = new List<Line>();
                //using (Transaction tr = db.TransactionManager.StartTransaction())
                //{
                //    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                //    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //    // 6. 为每个单元格绘制四边
                //    for (int row = 0; row < rows; row++)
                //    {
                //        for (int col = 0; col < columns; col++)
                //        {
                //            // 计算单元格四个角点
                //            Point3d topLeft = new Point3d(insertionPoint.X + col * cellWidth, insertionPoint.Y - row * rowHeight, insertionPoint.Z);
                //            Point3d topRight = new Point3d(topLeft.X + cellWidth, topLeft.Y, topLeft.Z);
                //            Point3d bottomLeft = new Point3d(topLeft.X, topLeft.Y - rowHeight, topLeft.Z);
                //            Point3d bottomRight = new Point3d(topRight.X, topRight.Y - rowHeight, topRight.Z);
                //            // 创建单元格四边
                //            Line[] cellLines = new Line[4]
                //            {
                //        new Line(topLeft, topRight),     // 上边
                //        new Line(topRight, bottomRight), // 右边
                //        new Line(bottomRight, bottomLeft), // 下边
                //        new Line(bottomLeft, topLeft)    // 左边
                //            };
                //            // 设置图层并添加到图形和集合
                //            foreach (Line line in cellLines)
                //            {
                //                line.Layer = currentLayer;
                //                allLines.Add(line);
                //                btr.AppendEntity(line);
                //                tr.AddNewlyCreatedDBObject(line, true);
                //            }
                //        }
                //    }
                //    // 7. 消除重叠线
                //    RemoveDuplicateLines(allLines, btr, tr);
                //    //外框线
                //    DrawOutLine(insertionPoint, totalWidth, totalHeight, tr, btr);
                //    tr.Commit();
                //}

                ed.WriteMessage("\n成功创建 {0}行×{1}列 表格。", rowCount, columnCount);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n错误: " + ex.Message);
            }
        }
        private static void DrawOutLine(Point3d insertionPoint, double totalWidth, double totalHeight, Transaction tr, BlockTableRecord btr)
        {
            // 计算外框四个角点
            Point3d topLeftA = insertionPoint;
            Point3d topRightA = new Point3d(topLeftA.X + totalWidth, topLeftA.Y, topLeftA.Z);
            Point3d bottomLeftA = new Point3d(topLeftA.X, topLeftA.Y - totalHeight, topLeftA.Z);
            Point3d bottomRightA = new Point3d(topRightA.X, topRightA.Y - totalHeight, topRightA.Z);
            //    // 创建外框四条边
            //    Line[] outline = new Line[4]
            //    {
            //new Line(topLeftA, topRightA),     // 上边
            //new Line(topRightA, bottomRightA), // 右边
            //new Line(bottomRightA, bottomLeftA), // 下边
            //new Line(bottomLeftA, topLeftA)    // 左边
            //    };
            //    // 设置到0图层并添加到图形
            //    foreach (Line line in outline)
            //    {
            //        line.Layer = "0";
            //        btr.AppendEntity(line);
            //        tr.AddNewlyCreatedDBObject(line, true);
            //    }
            // 创建一个 Polyline 对象
            Autodesk.AutoCAD.DatabaseServices.Polyline polyline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            // 添加四个角点到 Polyline
            polyline.AddVertexAt(0, new Point2d(topLeftA.X, topLeftA.Y), 0, 0, 0);
            polyline.AddVertexAt(1, new Point2d(topRightA.X, topRightA.Y), 0, 0, 0);
            polyline.AddVertexAt(2, new Point2d(bottomRightA.X, bottomRightA.Y), 0, 0, 0);
            polyline.AddVertexAt(3, new Point2d(bottomLeftA.X, bottomLeftA.Y), 0, 0, 0);
            // 闭合 Polyline
            polyline.Closed = true;
            // 设置图层并添加到图形
            polyline.Layer = "0";
            btr.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }

        // 消除重复线段的方法
        private void RemoveDuplicateLines(List<Line> lines, BlockTableRecord btr, Transaction tr)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    if (IsDuplicateLine(lines[i], lines[j]))
                    {
                        lines[j].Erase();
                        lines.RemoveAt(j);
                        j--; // 调整索引
                    }
                }
            }
        }
        // 判断两条线是否重合
        private bool IsDuplicateLine(Line line1, Line line2)
        {
            return (line1.StartPoint.IsEqualTo(line2.StartPoint) &&
                   (line1.EndPoint.IsEqualTo(line2.EndPoint)) ||
                   (line1.StartPoint.IsEqualTo(line2.EndPoint)) &&
                   (line1.EndPoint.IsEqualTo(line2.StartPoint)));
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
    //public class CustomTableDrawer
    //{
    //    [CommandMethod("DrawCustomTable")]
    //    public void DrawCustomTable()
    //    {
    //        // 获取当前数据库和编辑器
    //        Database db = HostApplicationServices.WorkingDatabase;
    //        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
    //        // 提示用户选择左上角点
    //        PromptPointOptions ptOpts = new PromptPointOptions("\n请选择表格的左上角点: ");
    //        PromptPointResult ptRes = ed.GetPoint(ptOpts);
    //        if (ptRes.Status != PromptStatus.OK)
    //            return;
    //        Point3d insertionPoint = ptRes.Value;
    //        // 定义表格参数
    //        int rows = 3;
    //        int columns = 5;
    //        double cellWidth = 100.0;
    //        double rowHeight = 5.0;
    //        // 获取当前图层
    //        ObjectId currentLayerId = db.Clayer;
    //        // 开始事务
    //        using (Transaction tr = db.TransactionManager.StartTransaction())
    //        {
    //            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
    //            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
    //            // 绘制表格主体（所有线条）
    //            DrawTableBody(btr, insertionPoint, rows, columns, cellWidth, rowHeight, currentLayerId);
    //            // 绘制外边框（使用图层"bbb"）
    //            DrawTableBorder(btr, insertionPoint, rows, columns, cellWidth, rowHeight, "bbb");
    //            tr.Commit();
    //        }
    //        ed.WriteMessage("\n自定义表格已成功创建。");
    //    }
    //    /// <summary>
    //    /// 绘制表格主体（内部线条）
    //    /// </summary>
    //    private void DrawTableBody(BlockTableRecord btr, Point3d insertionPoint,
    //                             int rows, int columns, double cellWidth, double rowHeight,
    //                             ObjectId layerId)
    //    {
    //        // 绘制水平线（不包括外边框）
    //        for (int row = 1; row < rows; row++) // 从1到rows-1
    //        {
    //            double y = insertionPoint.Y - row * rowHeight;
    //            Line hLine = new Line(
    //                new Point3d(insertionPoint.X, y, 0),
    //                new Point3d(insertionPoint.X + columns * cellWidth, y, 0)
    //            );
    //            hLine.LayerId = layerId;
    //            btr.AppendEntity(hLine);
    //            btr.AddNewlyCreatedDBObject(hLine, true);
    //        }
    //        // 绘制垂直线（不包括外边框）
    //        for (int col = 1; col < columns; col++) // 从1到columns-1
    //        {
    //            double x = insertionPoint.X + col * cellWidth;
    //            Line vLine = new Line(
    //                new Point3d(x, insertionPoint.Y, 0),
    //                new Point3d(x, insertionPoint.Y - rows * rowHeight, 0)
    //            );
    //            vLine.LayerId = layerId;
    //            btr.AppendEntity(vLine);
    //            btr.AddNewlyCreatedDBObject(vLine, true);
    //        }
    //    }
    //    /// <summary>
    //    /// 绘制表格外边框（使用指定图层）
    //    /// </summary>
    //    private void DrawTableBorder(BlockTableRecord btr, Point3d insertionPoint,
    //                                int rows, int columns, double cellWidth, double rowHeight,
    //                                string layerName)
    //    {
    //        // 获取指定图层ID
    //        ObjectId layerId;
    //        using (Transaction tr = btr.Database.TransactionManager.StartTransaction())
    //        {
    //            LayerTable lt = (LayerTable)tr.GetObject(btr.Database.LayerTableId, OpenMode.ForRead);
    //            if (!lt.Has(layerName))
    //            {
    //                // 如果图层不存在，则创建
    //                LayerTableRecord ltr = new LayerTableRecord();
    //                ltr.Name = layerName;
    //                layerId = lt.Add(ltr);
    //                tr.AddNewlyCreatedDBObject(ltr, true);
    //            }
    //            else
    //            {
    //                layerId = lt[layerName];
    //            }
    //            tr.Commit();
    //        }
    //        // 绘制外边框线条
    //        // 上边线
    //        Line topLine = new Line(
    //            insertionPoint,
    //            new Point3d(insertionPoint.X + columns * cellWidth, insertionPoint.Y, 0)
    //        );
    //        topLine.LayerId = layerId;
    //        btr.AppendEntity(topLine);
    //        btr.AddNewlyCreatedDBObject(topLine, true);
    //        // 下边线
    //        Line bottomLine = new Line(
    //            new Point3d(insertionPoint.X, insertionPoint.Y - rows * rowHeight, 0),
    //            new Point3d(insertionPoint.X + columns * cellWidth, insertionPoint.Y - rows * rowHeight, 0)
    //        );
    //        bottomLine.LayerId = layerId;
    //        btr.AppendEntity(bottomLine);
    //        btr.AddNewlyCreatedDBObject(bottomLine, true);
    //        // 左边线
    //        Line leftLine = new Line(
    //            insertionPoint,
    //            new Point3d(insertionPoint.X, insertionPoint.Y - rows * rowHeight, 0)
    //        );
    //        leftLine.LayerId = layerId;
    //        btr.AppendEntity(leftLine);
    //        btr.AddNewlyCreatedDBObject(leftLine, true);
    //        // 右边线
    //        Line rightLine = new Line(
    //            new Point3d(insertionPoint.X + columns * cellWidth, insertionPoint.Y, 0),
    //            new Point3d(insertionPoint.X + columns * cellWidth, insertionPoint.Y - rows * rowHeight, 0)
    //        );
    //        rightLine.LayerId = layerId;
    //        btr.AppendEntity(rightLine);
    //        btr.AddNewlyCreatedDBObject(rightLine, true);
    //    }
    //}
    //public class TableGenerator
    //{
    //    [CommandMethod("CreateTable")]
    //    public void CreateTable()
    //    {
    //        // 获取当前数据库和编辑器
    //        Database db = HostApplicationServices.WorkingDatabase;
    //        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
    //        // 提示用户选择左上角点
    //        PromptPointOptions ptOpts = new PromptPointOptions("\n请选择表格的左上角点: ");
    //        PromptPointResult ptRes = ed.GetPoint(ptOpts);
    //        if (ptRes.Status != PromptStatus.OK)
    //            return;
    //        Point3d insertionPoint = ptRes.Value;
    //        // 定义表格参数
    //        int rows = 3;
    //        int columns = 5;
    //        double cellWidth = 100.0;
    //        double rowHeight = 5.0;
    //        // 计算表格的总宽度和高度
    //        double tableWidth = columns * cellWidth;
    //        double tableHeight = rows * rowHeight;
    //        // 创建表格对象
    //        using (Transaction tr = db.TransactionManager.StartTransaction())
    //        {
    //            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
    //            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
    //            // 创建表格对象
    //            Autodesk.AutoCAD.DatabaseServices.Table table = new Autodesk.AutoCAD.DatabaseServices.Table();
    //            table.Position = insertionPoint;
    //            table.NumRows = rows;
    //            table.NumColumns = columns;
    //            table.SetRowHeight(rowHeight, false); // 设置所有行的高度
    //            table.SetColumnWidth(cellWidth, false); // 设置所有列的宽度
    //            // 设置表格样式（可选）
    //            // 如果需要自定义样式，可以在这里设置
    //            // 添加表格到模型空间
    //            btr.AppendEntity(table);
    //            tr.AddNewlyCreatedDBObject(table, true);
    //            // 设置单元格内容（可选）
    //            for (int row = 0; row < rows; row++)
    //            {
    //                for (int col = 0; col < columns; col++)
    //                {
    //                    Cell cell = table.GetCell(row, col);
    //                    // 设置单元格内容，例如 "行row+1, 列col+1"
    //                    cell.TextString = $"行{row + 1}, 列{col + 1}";
    //                    table.SetCellTextString(row, col, cell.TextString);
    //                }
    //            }
    //            // 更新表格布局
    //            table.GenerateLayout();
    //            tr.Commit();
    //        }
    //        ed.WriteMessage("\n表格已成功创建。");
    //    }
    //}
}
