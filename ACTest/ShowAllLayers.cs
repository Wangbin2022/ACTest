using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using AcadApp = Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Runtime;
using ACTest;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(ShowAllLayers))]
namespace ACTest
{
    public class ShowAllLayers
    {
        [CommandMethod("ShowAllLayers", CommandFlags.UsePickSet)]
        public void ShowAllLayersMethod()
        {
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //ed.WriteMessage("New20250420,222");
            LayerManagerView layerManager =new LayerManagerView();
            layerManager.ShowDialog();
        }
    }
}
