using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ACTest
{
    public class RIbbonTest
    {
        [CommandMethod("RibbonTest")]
        public void RibbonTest()
        {
            //新版本要用新方式声明RibbonControl么？
            //RibbonControl ribbonControl = Autodesk.AutoCAD.Ribbon.RibbonServices.RibbonPaletteSet.RibbonControl;
            //RibbonControl ribbonControl = Autodesk.Windows.ComponentManager.Ribbon;
            RibbonControl ribbonControl = ComponentManager.Ribbon;
            RibbonTab tab = new RibbonTab();
            tab.Title = "Test";
            tab.Id = "ACAD.My_RibbonTab";
            ribbonControl.Tabs.Add(tab);
            tab.IsActive = true;
        }
    }
}
