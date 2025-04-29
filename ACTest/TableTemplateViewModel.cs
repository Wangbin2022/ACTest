using ACTest.Base;
using ACTest.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;


namespace ACTest
{
    public class TableTemplateViewModel : ObserverableObject
    {
        Database Database;
        Document Document;
        private ObservableCollection<ColumnProperties> columnPropertiesList = new ObservableCollection<ColumnProperties>();
        public ObservableCollection<ColumnProperties> ColumnPropertiesList
        {
            get => columnPropertiesList;
            set
            {
                columnPropertiesList = value;
                OnPropertyChanged(nameof(columnPropertiesList));
            }
        }
        public string filePath = @"D:\temp\test.xml";
        public TableTemplateViewModel(Document document)
        {
            Document = document;
            Database = document.Database;
            LayerNames = GetLayerNames();
            SelectedFontLayer = LayerNamesWithCurrent.FirstOrDefault();
            SelectedInnerLineLayer = LayerNamesWithCurrent.FirstOrDefault();
            SelectedOutLineLayer = LayerNamesWithCurrent.FirstOrDefault();
        }
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
        private LayerInfo _selectedOutLineLayer;
        public LayerInfo SelectedOutLineLayer
        {
            get => _selectedOutLineLayer;
            set
            {
                _selectedOutLineLayer = value;
                OnPropertyChanged(nameof(SelectedOutLineLayer));
            }
        }
        private LayerInfo _selectedInnerLineLayer;
        public LayerInfo SelectedInnerLineLayer
        {
            get => _selectedInnerLineLayer;
            set
            {
                _selectedInnerLineLayer = value;
                OnPropertyChanged(nameof(SelectedInnerLineLayer));
            }
        }
        // 当前选中项
        private LayerInfo _selectedFontLayer;
        public LayerInfo SelectedFontLayer
        {
            get => _selectedFontLayer;
            set
            {
                _selectedFontLayer = value;
                OnPropertyChanged(nameof(SelectedFontLayer));
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
        // 根据选择更新 ComboBox 的 IsEnabled 状态
        private void UpdateComboBoxEnabledState(string selectedOption)
        {
            // 如果选择"内外框不同"，则启用 ComboBox（FrameNotSameLayer=true）
            // 如果选择"内外框相同"，则禁用 ComboBox（FrameNotSameLayer=false）
            FrameNotSameLayer = (selectedOption == "内外框不同");
        }
        private string isFrameSameLayer = "内外框相同";
        public string IsFrameSameLayer
        {
            get => isFrameSameLayer;
            set
            {
                if (isFrameSameLayer != value)
                {
                    isFrameSameLayer = value;
                    OnPropertyChanged(nameof(IsFrameSameLayer));
                    UpdateComboBoxEnabledState(value);
                }
            }
        }
        private bool frameNotSameLayer = false;  // 默认禁用
        public bool FrameNotSameLayer
        {
            get => frameNotSameLayer;
            set
            {
                if (frameNotSameLayer != value)
                {
                    frameNotSameLayer = value;
                    OnPropertyChanged(nameof(FrameNotSameLayer));
                }
            }
        }
        public string[] csvContents { get; set; }
        //要增加三种图层的判断和绑定，外框、内框、文字，默认均是当前图层，还要是否内外框相同图层bool。
        public ICommand DrawTableCommand => new BaseBindingCommand(DrawTable);
        //要重写画表格DrawTable方法
        private void DrawTable(object obj)
        {
            //MessageBox.Show(BaseWidth + "\n" + TextSize + "\n" + Font + "\n" + TableScale + "\n" + SelectedFontLayer.Name + "\n" + InnerLineStyle + "\n" + SelectedOutLineLayer.Name + "\n" + SelectedInnerLineLayer.Name);
            if (ColumnPropertiesList.Count() > 0 || !IsInModelSpace())
            {
                int rowCount = lines.Length;
                int columnCount = lines[0].Split(',').Length;
                double rowHeight;
                double baseCellWidth;
                //检查所在空间确定行高和字高
                //ed.WriteMessage("\n当前在模型空间操作");
                rowHeight = TextSize * 1.5 * TableScale; // 行高
                baseCellWidth = BaseWidth * TableScale; // 基准列宽 
                double[] colWidthFactors = new double[12];
                if (columnCount <= 12)
                {
                    colWidthFactors[0] = 3;
                    colWidthFactors[1] = 1.5;
                    colWidthFactors[2] = 2;
                    colWidthFactors[3] = 2.4;
                    colWidthFactors[4] = 3;
                    colWidthFactors[5] = 2;
                    colWidthFactors[6] = 1;
                    colWidthFactors[7] = 1;
                    colWidthFactors[8] = 1;
                    colWidthFactors[9] = 1;
                    colWidthFactors[10] = 1;
                    colWidthFactors[11] = 1;
                }
                else MessageBox.Show("tt", "csv列数太多请清理后添加");
                //要读取xml并替换WidthFactor内列宽度
                if (ColumnPropertiesList.Count() < 13)
                {
                    for (int i = 0; i < ColumnPropertiesList.Count(); i++)
                    {
                        colWidthFactors[i] = ColumnPropertiesList[i].Width;
                    }
                }
                //确定内外框线样式
                string outLineLayerName = "0";
                string innerLineLayerName = "0";
                using (Transaction tr = Database.TransactionManager.StartTransaction())
                {
                    ObjectId currentLayerId = Database.Clayer;
                    LayerTableRecord currentLayer = (LayerTableRecord)tr.GetObject(currentLayerId, OpenMode.ForRead);
                    // 确定外框图层名称
                    outLineLayerName = SelectedOutLineLayer.Name == "使用当前图层" ? currentLayer.Name : SelectedOutLineLayer.Name;
                    // 确定内框图层名称
                    if (IsFrameSameLayer == "内外框相同")
                    {
                        if (outLineLayerName != "使用当前图层")
                        {
                            innerLineLayerName = outLineLayerName; // 内外框图层相同，直接使用外框图层名称
                        }
                        else innerLineLayerName = currentLayer.Name;
                    }
                    else
                    {
                        innerLineLayerName = SelectedInnerLineLayer.Name == "使用当前图层" ? currentLayer.Name : SelectedInnerLineLayer.Name;
                    }
                    tr.Commit();
                }
                ////确定字体，字号等

                string[,] tableData = new string[rowCount, columnCount];
                TextAlignment[] columnAlignments = new TextAlignment[columnCount];
                //// 3. 解析每行数据
                for (int i = 0; i < rowCount; i++)
                {
                    string[] cells = lines[i].Split(',');
                    for (int j = 0; j < columnCount; j++)
                    {
                        string cellContent = cells[j].Trim();
                        TextAlignment alignment;
                        switch (ColumnPropertiesList[j].Alignment)
                        {

                            case "靠左":
                                alignment = TextAlignment.Left;
                                break;
                            default:
                            case "居中":
                                alignment = TextAlignment.Center;
                                break;
                        }
                        tableData[i, j] = cellContent;
                        // 第一行决定列对齐方式（可根据需要改为其他逻辑）
                        if (i == 0) columnAlignments[j] = alignment;
                    }
                }
                // 绘制表格
                //// 1. 获取用户选择的插入点
                //Editor ed = Document.Editor;
                //PromptPointResult ppr = ed.GetPoint("\n请选择表格左上角插入点: "); 
                //if (ppr.Status != PromptStatus.OK) return;
                //Point3d insertionPoint = ppr.Value;
                // 替换原来的ppr获取代码，使用Jig预览
                // 创建DrawJig实例
                TableDrawJig jig = new TableDrawJig(Point3d.Origin, rowCount, columnCount, rowHeight, baseCellWidth, colWidthFactors, TableScale);
                // 启动拖动预览
                PromptResult dragResult = Document.Editor.Drag(jig);
                if (dragResult.Status != PromptStatus.OK) return;
                Point3d insertionPoint = jig.InsertionPoint;


                double totalWidth = 0;
                for (int i = 0; i < columnCount; i++) totalWidth += baseCellWidth * colWidthFactors[i];
                double totalHeight = rowCount * rowHeight;
                switch (InnerLineStyle)
                {
                    case "通长绘制":
                        using (Transaction tr = Database.TransactionManager.StartTransaction())
                        {
                            BlockTable bt = tr.GetObject(Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                            // 6. 创建水平线
                            for (int i = 0; i < rowCount; i++)
                            {
                                Point3d start = new Point3d(insertionPoint.X, insertionPoint.Y - (i * rowHeight), insertionPoint.Z);
                                Point3d end = new Point3d(insertionPoint.X + totalWidth, insertionPoint.Y - (i * rowHeight), insertionPoint.Z);
                                Line horizontalLine = new Line(start, end);
                                horizontalLine.Layer = innerLineLayerName;
                                btr.AppendEntity(horizontalLine);
                                tr.AddNewlyCreatedDBObject(horizontalLine, true);
                            }
                            // 7. 创建垂直线
                            for (int j = 0; j < columnCount; j++)
                            {
                                Point3d start = new Point3d(insertionPoint.X + GetColumnWidthSum(j, baseCellWidth, colWidthFactors), insertionPoint.Y, insertionPoint.Z);
                                Point3d end = new Point3d(insertionPoint.X + GetColumnWidthSum(j, baseCellWidth, colWidthFactors), insertionPoint.Y - totalHeight, insertionPoint.Z);
                                Line verticalLine = new Line(start, end);
                                verticalLine.Layer = innerLineLayerName;
                                btr.AppendEntity(verticalLine);
                                tr.AddNewlyCreatedDBObject(verticalLine, true);
                            }
                            DrawOutLine(insertionPoint, totalWidth, totalHeight, tr, btr, outLineLayerName);
                            tr.Commit();
                        }
                        break;
                    default:
                        List<Line> allLines = new List<Line>();
                        using (Transaction tr = Database.TransactionManager.StartTransaction())
                        {
                            BlockTable bt = (BlockTable)tr.GetObject(Database.BlockTableId, OpenMode.ForRead);
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                            // 6. 为每个单元格绘制四边
                            for (int row = 0; row < rowCount; row++)
                            {
                                for (int col = 0; col < columnCount; col++)
                                {
                                    // 计算当前单元格的起始点（考虑列宽系数）
                                    double cellX = insertionPoint.X + GetColumnWidthSum(col, baseCellWidth, colWidthFactors);
                                    double cellY = insertionPoint.Y - row * rowHeight;
                                    // 计算单元格四个角点
                                    Point3d topLeft = new Point3d(cellX, cellY, insertionPoint.Z);
                                    Point3d topRight = new Point3d(cellX + baseCellWidth * colWidthFactors[col], cellY, insertionPoint.Z);
                                    Point3d bottomLeft = new Point3d(cellX, cellY - rowHeight, insertionPoint.Z);
                                    Point3d bottomRight = new Point3d(topRight.X, cellY - rowHeight, insertionPoint.Z);
                                    // 创建单元格四条边
                                    Line lineTop = new Line(topLeft, topRight);       // 上边
                                    Line lineRight = new Line(topRight, bottomRight); // 右边
                                    Line lineBottom = new Line(bottomLeft, bottomRight); // 下边
                                    Line lineLeft = new Line(bottomLeft, topLeft);    // 左边
                                    // 设置图层并添加到图形和集合
                                    Line[] cellLines = new Line[4] { lineTop, lineRight, lineBottom, lineLeft };
                                    foreach (Line line in cellLines)
                                    {
                                        line.Layer = innerLineLayerName;
                                        allLines.Add(line);
                                        btr.AppendEntity(line);
                                        tr.AddNewlyCreatedDBObject(line, true);
                                    }
                                }
                            }
                            // 7. 消除重叠线
                            RemoveDuplicateLines(allLines, btr, tr);
                            //外框线
                            DrawOutLine(insertionPoint, totalWidth, totalHeight, tr, btr, outLineLayerName);
                            tr.Commit();
                        }
                        break;
                }
                // 表内加字
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
                        //tstr.XScale = 0.7;
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
                        //existingStyle.XScale = 0.7;  // 修改宽度因子
                        existingStyle.Font = new FontDescriptor(Font, false, false, 0, 0);
                        existingStyle.TextSize = 0;
                        existingStyle.IsVertical = false;
                        existingStyle.ObliquingAngle = 0;
                    }
                    ObjectId currentLayerId = Database.Clayer;
                    LayerTableRecord currentLayer = (LayerTableRecord)tr.GetObject(currentLayerId, OpenMode.ForRead);
                    // 确定文字图层名称
                    string fontLayerName = SelectedFontLayer.Name == "使用当前图层" ? currentLayer.Name : SelectedFontLayer.Name;
                    for (int i = 0; i < rowCount; i++)
                    {
                        for (int j = 0; j < columnCount; j++)
                        {
                            // 计算单元格原点
                            double cellX = insertionPoint.X + GetColumnWidthSum(j, baseCellWidth, colWidthFactors);
                            double cellY = insertionPoint.Y - i * rowHeight;
                            // 计算文本插入点（垂直居中）
                            Point3d textPoint = new Point3d(cellX, cellY - rowHeight / 2, insertionPoint.Z);
                            // 添加文本到单元格
                            AddTextToCell(Database, tr, textPoint, tableData[i, j], baseCellWidth * colWidthFactors[j], (TextAlignment)columnAlignments[j],
                                Font, fontLayerName, TextSize * TableScale);
                            //AddTextToCell(Database, tr, textPoint, tableData[i, j], baseCellWidth * colWidthFactors[j], TextAlignment.Center,"Standard", "0", TextSize * TableScale);
                        }
                    }
                    tr.Commit();
                }
            }
            else MessageBox.Show("请在模型空间载入csv数据源表格并重试");
        }
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
                    offsetX = cellWidth / 2;
                    break;
                case TextAlignment.Left:
                default:
                    offsetX = textHeight; // 左边距为文字高度的一半
                    break;
            }
            // 调整插入点
            Point3d adjustedPoint = new Point3d(insertionPoint.X + offsetX, insertionPoint.Y, insertionPoint.Z);
            // 创建文本对象
            DBText dbText = new DBText
            {
                Position = adjustedPoint,
                TextString = text,
                Height = textHeight,
                Layer = layer,
                Justify = AttachmentPoint.MiddleCenter // 垂直居中
            };
            dbText.AlignmentPoint = dbText.Position;
            dbText.WidthFactor = 0.7;
            if (alignment == TextAlignment.Left)
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
            return dbText;
        }
        // 添加的辅助方法：计算列宽累计和
        private double GetColumnWidthSum(int columnIndex, double baseWidth, double[] widthFactors)
        {
            double sum = 0;
            for (int i = 0; i < columnIndex; i++)
            {
                sum += baseWidth * widthFactors[i];
            }
            return sum;
        }
        private static void DrawOutLine(Point3d insertionPoint, double totalWidth, double totalHeight, Transaction tr, BlockTableRecord btr, string outLineLayerName)
        {
            // 计算外框四个角点
            Point3d topLeftA = insertionPoint;
            Point3d topRightA = new Point3d(topLeftA.X + totalWidth, topLeftA.Y, topLeftA.Z);
            Point3d bottomLeftA = new Point3d(topLeftA.X, topLeftA.Y - totalHeight, topLeftA.Z);
            Point3d bottomRightA = new Point3d(topRightA.X, topRightA.Y - totalHeight, topRightA.Z);
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
            polyline.Layer = outLineLayerName;
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
        public bool IsInModelSpace()
        {
            Document doc = Document;
            Database db = Document.Database;
            // 三种条件同时满足才确定是模型空间
            return (db.TileMode &&
                    db.CurrentSpaceId == SymbolUtilityServices.GetBlockModelSpaceId(db));
        }
        public enum TextAlignment { Left, Center }
        //要重写AddTextToCell方法
        //private TextNote AddTextToCell(Document doc, Autodesk.Revit.DB.View view, XYZ insertionPoint, string text, double cellWidth, TextAlignment alignment, ElementId textTypeId)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //    {
        //        text = "-"; // 默认显示内容
        //    }
        //    // 根据对齐方式调整插入点X坐标
        //    double offsetX = 0;
        //    switch (alignment)
        //    {
        //        case TextAlignment.Center:
        //            offsetX = cellWidth / 3;
        //            break;
        //        case TextAlignment.Left:
        //        default:
        //            offsetX = 1.5 / 304.8; // 留0.1英尺左边距
        //            break;
        //    }
        //    XYZ adjustedPoint = new XYZ(insertionPoint.X + offsetX, insertionPoint.Y, insertionPoint.Z);
        //    TextNoteOptions noteOptions = new TextNoteOptions();
        //    noteOptions.TypeId = textTypeId;
        //    //noteOptions.HorizontalAlignment = HorizontalTextAlignment.Right;
        //    noteOptions.VerticalAlignment = VerticalTextAlignment.Middle;
        //    TextNote textNote = TextNote.Create(doc, view.Id, adjustedPoint, text, noteOptions);
        //    // 设置对齐参数// 1=左, 2=中, 3=右
        //    textNote.get_Parameter(BuiltInParameter.TEXT_ALIGN_HORZ).Set((int)alignment);
        //    return textNote;
        //}
        private int tableScale = 100;
        public int TableScale
        {
            get { return tableScale; }
            set { tableScale = value; }
        }
        private double baseWidth = 20.0;
        public double BaseWidth
        {
            get { return baseWidth; }
            set { baseWidth = value; }
        }
        private double textSize = 3.0;
        public double TextSize
        {
            get { return textSize; }
            set { textSize = value; }
        }
        public string InnerLineStyle { get; set; } = "通长绘制";
        public string Font { get; set; } = "宋体";
        public ICommand RemoveXmlCommand => new BaseBindingCommand(RemoveXml);
        private void RemoveXml(object obj)
        {
            if (SelectedTableSingle != null)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        int i = 0;
                        TableCollection tableCollection = XMLUtil.DeserializeFromXml<TableCollection>(filePath);
                        List<TableSingle> tss = tableCollection.tableSingles;
                        // 使用 RemoveAll 方法移除所有为 tableName 的元素
                        i = tss.RemoveAll(ts => ts.tableName == selectedTableSingle.tableName);
                        XMLUtil.SerializeToXml(filePath, tableCollection);
                        var itemsToRemove = TableSingles.Where(ts => ts.tableName == SelectedTableSingle.tableName).ToList();
                        foreach (var item in itemsToRemove)
                        {
                            TableSingles.Remove(item);
                        }
                        MessageBox.Show("tt", $"已删除表格样式{i}个");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("tt", ex.Message.ToString() + "aaa");
                }
            }
        }
        public ICommand ExportXmlCommand => new BaseBindingCommand(ExportXml);
        private void ExportXml(object obj)
        {
            if (tName == null)
            {
                MessageBox.Show("tt", "请输入表格名称");
                return;
            }
            TableSingle tableSingle = new TableSingle() { tableName = tName };
            tableSingle.tableEntities = new List<TableEntity>();
            foreach (ColumnProperties item in ColumnPropertiesList)
            {
                TableEntity entity1 = new TableEntity()
                {
                    entityName = item.Title,
                    entityWidth = item.Width,
                    entityAligh = item.Alignment,
                    //entityRow = item.RowCount
                };
                tableSingle.tableEntities.Add(entity1);
            }
            // 2. 检查 XML 文件是否存在，并加载现有数据（如果存在）
            TableCollection tableCollection;
            if (File.Exists(filePath))
            {
                // 反序列化现有 XML
                tableCollection = XMLUtil.DeserializeFromXml<TableCollection>(filePath);
                // 确保 tableSingles 列表已初始化
                if (tableCollection.tableSingles == null)
                {
                    tableCollection.tableSingles = new List<TableSingle>();
                }
            }
            else
            {
                tableCollection = new TableCollection();
                tableCollection.tableSingles = new List<TableSingle>();
            }
            tableCollection.tableSingles.Add(tableSingle);
            XMLUtil.SerializeToXml(filePath, tableCollection);
            MessageBox.Show("tt", "已生成表格新样式");
        }
        private bool _canExportXML = false;
        public bool CanExportXML
        {
            get { return _canExportXML; }
            set
            {
                if (_canExportXML != value)
                {
                    _canExportXML = value;
                    OnPropertyChanged(nameof(CanExportXML));
                }
            }
        }
        public ICommand GetCsvCommand => new BaseBindingCommand(GetCsv);
        private string[] lines { get; set; }
        private void GetCsv(object obj)
        {
            ColumnPropertiesList.Clear();
            //XmlDoc.Instance.Task.Run(app =>
            //{       //});
            OpenFileDialog fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Filter = "csv 文件 (*.csv)|*.csv";
            if (fDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string firstLine = File.ReadLines(fDialog.FileName).First();
                lines = File.ReadAllLines(fDialog.FileName);
                //csv异常监测
                int totalColumns = lines[0].Split(',').Length;
                if (totalColumns > 12)
                {
                    //ed.WriteMessage( "本工具暂不支持超过12列表格绘制");
                    MessageBox.Show("本工具暂不支持超过12列表格绘制");
                    return;
                }
                // 检查包含额外逗号的异常行数
                int rowsWithExtraCommas = 0;
                List<int> rowsWithExtraCommasPositions = new List<int>();
                for (int i = 0; i < lines.Length; i++)
                {
                    int columnCount = lines[i].Split(',').Length;
                    if (columnCount != totalColumns)
                    {
                        rowsWithExtraCommas++;
                        rowsWithExtraCommasPositions.Add(i + 1); // 行号从 1 开始
                    }
                }
                MessageBox.Show($"待生成表格总行数: {lines.Length}+总列数: {totalColumns}");
                if (rowsWithExtraCommasPositions.Count != 0)
                {
                    MessageBox.Show("异常行位置: " + string.Join(", ", rowsWithExtraCommasPositions));
                    return;
                }
                csvContents = lines;
                ////当对比xml内容有相同组合时，列出标题到combobox，内容到datagrid并绑定。更新ColumnPropertiesList
                try
                {
                    TableCollection test = XMLUtil.DeserializeFromXml<TableCollection>(filePath);
                    TableSingles = new ObservableCollection<TableSingle>();
                    foreach (TableSingle ts in test.tableSingles)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (TableEntity item in ts.tableEntities)
                        {
                            sb.Append(item.entityName + ",");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        if (sb.ToString() == firstLine)
                        {
                            TableSingles.Add(ts);
                        }
                    }
                    //// 在初始化时设置默认选中项
                    if (TableSingles != null && TableSingles.Count > 0)
                    {
                        SelectedTableSingle = TableSingles[0]; // 默认选中第一个
                    }
                    //当对比xml内容无相同组合时
                    if (tableSingles.Count() == 0)
                    {
                        //// 按逗号分割字段,统计字段数量
                        tableTitles = firstLine.Split(',').ToList();
                        fieldCount = tableTitles.Count;
                        foreach (var title in tableTitles)
                        {
                            ColumnPropertiesList.Add(new ColumnProperties { Title = title });
                        }
                    }
                    CanExportXML = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        private void UpdateDataGridFromSelectedTable()
        {
            if (SelectedTableSingle == null || SelectedTableSingle.tableEntities == null)
            {
                ColumnPropertiesList.Clear();
                return;
            }
            ColumnPropertiesList.Clear();
            // 将选中的 TableSingle 的 tableEntities 映射到 ColumnPropertiesList
            foreach (TableEntity item in SelectedTableSingle.tableEntities)
            {
                ColumnPropertiesList.Add(new ColumnProperties
                {
                    Title = item.entityName,
                    Width = item.entityWidth,
                    Alignment = item.entityAligh,
                    //RowCount = item.entityRow
                });
            }
        }
        private ObservableCollection<TableSingle> tableSingles = new ObservableCollection<TableSingle>();
        public ObservableCollection<TableSingle> TableSingles
        {
            get => tableSingles;
            set
            {
                tableSingles = value;
                OnPropertyChanged(nameof(TableSingles));
            }
        }
        private TableSingle selectedTableSingle;
        public TableSingle SelectedTableSingle
        {
            get => selectedTableSingle;
            set
            {
                selectedTableSingle = value;
                OnPropertyChanged(nameof(SelectedTableSingle));
                UpdateDataGridFromSelectedTable();
            }
        }
        public List<string> tableTitles = new List<string>();
        public int fieldCount = 0;
        public string tName;
        public string TName
        {
            get => tName;
            set
            {
                tName = value;
                OnPropertyChanged(nameof(tName));
            }
        }
    }
    public class ColumnProperties
    {
        public string Title { get; set; }
        public double Width { get; set; } = 1.0; // 默认值为 1
        public string Alignment { get; set; } = "居中";
        //public int RowCount { get; set; } = 1; // 默认值为 1
    }
    [XmlType(TypeName = "TableCollection")]
    public class TableCollection
    {
        [XmlArray("tableSingles")]
        public List<TableSingle> tableSingles { get; set; }
    }
    [XmlType(TypeName = "TableSingle")]
    public class TableSingle
    {
        [XmlAttribute]
        public string tableName { get; set; }
        [XmlArray("tableEntities")]
        public List<TableEntity> tableEntities { get; set; }
    }
    [XmlType(TypeName = "TableEntity")]
    public class TableEntity
    {
        [XmlAttribute]
        public string entityName { get; set; }
        [XmlAttribute]
        public double entityWidth { get; set; }
        [XmlAttribute]
        public string entityAligh { get; set; }
        //[XmlAttribute]
        //public int entityRow { get; set; }
    }
    public class TableDrawJig : DrawJig
    {
        private Point3d _insertionPoint;
        private readonly int _rowCount;
        private readonly int _columnCount;
        private readonly double _rowHeight;
        private readonly double _baseCellWidth;
        private readonly double[] _colWidthFactors;
        private readonly double _tableScale;

        public TableDrawJig(
            Point3d initialPoint,
            int rowCount, int columnCount,
            double rowHeight, double baseCellWidth,
            double[] colWidthFactors,
            double tableScale)
        {
            _insertionPoint = initialPoint;
            _rowCount = rowCount;
            _columnCount = columnCount;
            _rowHeight = rowHeight;
            _baseCellWidth = baseCellWidth;
            _colWidthFactors = colWidthFactors;
            _tableScale = tableScale;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            // 设置预览颜色为青色
            draw.SubEntityTraits.Color = 253; // 青色
            // 1. 绘制表格外框
            DrawTableOutline(draw);
            // 2. 绘制表格内框
            DrawTableInnerLines(draw);
            return true;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpts = new JigPromptPointOptions("\n指定表格位置: ");
            jigOpts.UserInputControls = UserInputControls.Accept3dCoordinates |
                                      UserInputControls.NoZeroResponseAccepted |
                                      UserInputControls.NoNegativeResponseAccepted;
            PromptPointResult result = prompts.AcquirePoint(jigOpts);

            if (_insertionPoint != result.Value)
            {
                _insertionPoint = result.Value;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }
        private void DrawTableOutline(WorldDraw draw)
        {
            double totalWidth = GetTotalWidth();
            double totalHeight = _rowCount * _rowHeight;
            // 绘制外框
            DrawRectangle(draw, _insertionPoint, totalWidth, totalHeight);
        }
        private void DrawTableInnerLines(WorldDraw draw)
        {
            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    double cellX = _insertionPoint.X + GetColumnWidthSum(col, _baseCellWidth, _colWidthFactors);
                    double cellY = _insertionPoint.Y - row * _rowHeight;
                    double cellWidth = _baseCellWidth * _colWidthFactors[col];

                    DrawRectangle(draw,
                        new Point3d(cellX, cellY, _insertionPoint.Z),
                        cellWidth, _rowHeight);
                }
            }
        }
        private void DrawRectangle(WorldDraw draw, Point3d topLeft, double width, double height)
        {
            Point3d topRight = new Point3d(topLeft.X + width, topLeft.Y, topLeft.Z);
            Point3d bottomLeft = new Point3d(topLeft.X, topLeft.Y - height, topLeft.Z);
            Point3d bottomRight = new Point3d(topRight.X, topRight.Y - height, topRight.Z);

            // 绘制四条边
            draw.Geometry.WorldLine(topLeft, topRight);
            draw.Geometry.WorldLine(topRight, bottomRight);
            draw.Geometry.WorldLine(bottomRight, bottomLeft);
            draw.Geometry.WorldLine(bottomLeft, topLeft);
        }
        private double GetTotalWidth()
        {
            double total = 0;
            for (int i = 0; i < _columnCount; i++)
            {
                total += _baseCellWidth * _colWidthFactors[i];
            }
            return total;
        }
        private double GetColumnWidthSum(int columnIndex, double baseWidth, double[] widthFactors)
        {
            double sum = 0;
            for (int i = 0; i < columnIndex; i++)
            {
                sum += baseWidth * widthFactors[i];
            }
            return sum;
        }
        public Point3d InsertionPoint => _insertionPoint;
    }

}
