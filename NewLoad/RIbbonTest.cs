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
            RibbonControl ribbonControl = ComponentManager.Ribbon;
            RibbonTab tab = new RibbonTab();
            tab.Title = "Test";
            tab.Id = "ACAD.My_RibbonTab";
            ribbonControl.Tabs.Add(tab);
            tab.IsActive = true;
        }
    }
}
