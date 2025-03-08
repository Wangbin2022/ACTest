using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        Database Database;
        public Window1(Database database)
        {
            InitializeComponent();
            Database= database; 
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
            //0306 用户交互
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions ppo = new PromptPointOptions("input point");
            ppo.AllowNone = true;
            PromptPointResult ppr = GetPoint(ppo);
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d();
            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.OK) p1 = ppr.Value;
            ppo.Message = "2nd Point input.";
            ppo.BasePoint = p1;
            ppo.UseBasePoint = true;
            ppr = GetPoint(ppo);
            if (ppr.Status == PromptStatus.Cancel) return;
            if (ppr.Status == PromptStatus.None) return;
            if (ppr.Status == PromptStatus.OK) p2 = ppr.Value;
            AddLineToModelSpace(Database, p1, p2);
            //例程结束
        }
        public PromptPointResult GetPoint(PromptPointOptions ppo)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            ppo.AllowNone = true;
            return ed.GetPoint(ppo);
        }
        public ObjectId AddEntityToModelSpace(Database db, Entity ent)
        {
            ObjectId objectId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                objectId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }
            return objectId;
        }
        public ObjectId AddLineToModelSpace(Database db, Point3d startPoint, Point3d endPoint)
        {
            return AddEntityToModelSpace(db, new Autodesk.AutoCAD.DatabaseServices.Line(startPoint, endPoint));
        }
    }
}
