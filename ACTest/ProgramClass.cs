using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace ACTest
{
    public class ProgramClass
    {
        [CommandMethod("RibbonDemo")]
        public void RibbonDemo()
        {
            RibbonControl ribbonCtrl = ComponentManager.Ribbon; //获取cad的Ribbon界面
            RibbonTab tab = ribbonCtrl.AddTab("选项卡1", "Acad.RibbonId1", true); //给Ribbon界面添加一个选项卡
            CurPath.curPath = Path.GetDirectoryName(this.GetType().Assembly.Location) + "\\"; //获取程序集的加载路径
            RibbonPanelSource panelSource = tab.AddPanel("绘图"); //给选项卡添加面板
            panelSource.Items.Add(RibbonButtonInfos.LineBtn); //添加直线命令按钮
            //panelSource.Items.Add(RibbonButtonInfos.PolylineBtn); //添加多段线命令按钮
            //tab.AddPanel("修改");
            //tab.AddPanel("注释");
            //tab.AddPanel("图层");
        }
    }
    public static class RibbonButtonInfos
    {
        //直线按钮
        private static RibbonButtonEX lineBtn;
        public static RibbonButtonEX LineBtn
        {
            get
            {
                lineBtn = new RibbonButtonEX("直线", RibbonItemSize.Large, Orientation.Vertical, "Line");
                lineBtn.SetImg(CurPath.curPath + "Images\\直线放样32.PNG");//设置按钮图片
                //添加提示对象
                RibbonToolTip toolTip = new RibbonToolTip();
                toolTip.Title = "直线";
                toolTip.Content = "创建直线段";
                toolTip.Command = "LINE";
                toolTip.ExpandedContent = "是用LINE命令，可以创建一些列连续的直线段。每条线段都是可以单独进行编辑的直线对象。";
                string imgToolTipFileName = CurPath.curPath + "Images\\直线放样32.PNG";
                Uri toolTipUri = new Uri(imgToolTipFileName);
                BitmapImage toolTipBitmapImge = new BitmapImage(toolTipUri);
                toolTip.ExpandedImage = toolTipBitmapImge;
                lineBtn.ToolTip = toolTip;
                //鼠标进入时的图片
                lineBtn.ImgHoverFileName = CurPath.curPath + "Images\\直线放样16.PNG";
                return lineBtn;
            }
        }
        //多段线按钮
        private static RibbonButtonEX polylineBtn;
        public static RibbonButtonEX PolylineBtn
        {
            get
            {
                polylineBtn = new RibbonButtonEX("多段线", RibbonItemSize.Large, Orientation.Vertical, "Pline");
                polylineBtn.SetImg(CurPath.curPath + "Images\\Polyline.PNG");//设置按钮图片
                //添加提示对象
                RibbonToolTip toolTip = new RibbonToolTip();
                toolTip.Title = "多段线";
                toolTip.Content = "创建二维多段线";
                toolTip.Command = "PLINE";
                toolTip.ExpandedContent = "二维多段线是作为单个平面对象创建的相互连接的线段序列。可以创建直线段、圆弧段或者两者的组合线段。";
                string imgToolTipFileName = CurPath.curPath + "Images\\PolylineToolTip.PNG";
                Uri toolTipUri = new Uri(imgToolTipFileName);
                BitmapImage toolTipBitmapImge = new BitmapImage(toolTipUri);
                toolTip.ExpandedImage = toolTipBitmapImge;
                polylineBtn.ToolTip = toolTip;
                //鼠标进入时的图片
                polylineBtn.ImgHoverFileName = CurPath.curPath + "Images\\PolylineHover.PNG";
                return polylineBtn;
            }
        }
        //圆心半径按钮
        private static RibbonButtonEX circleCRBtn;
        public static RibbonButtonEX CircleCRBtn
        {
            get
            {
                circleCRBtn = new RibbonButtonEX("圆心,半径", RibbonItemSize.Large, Orientation.Horizontal, "Circle");
                circleCRBtn.SetImg(CurPath.curPath + "Images\\CircleCR.PNG");//设置按钮图片
                circleCRBtn.ShowText = false;
                //添加提示对象
                RibbonToolTip toolTip = new RibbonToolTip();
                toolTip.Title = "圆心,半径";
                toolTip.Content = "用圆心和半径创建圆";
                toolTip.Command = "CIRCLE";
                toolTip.ExpandedContent = "用圆心和半径创建圆。\n\n示例:";
                string imgToolTipFileName = CurPath.curPath + "Images\\CircleCDHover.PNG";
                Uri toolTipUri = new Uri(imgToolTipFileName);
                BitmapImage toolTipBitmapImge = new BitmapImage(toolTipUri);
                toolTip.ExpandedImage = toolTipBitmapImge;
                circleCRBtn.ToolTip = toolTip;
                //鼠标进入时的图片
                circleCRBtn.ImgHoverFileName = CurPath.curPath + "Images\\CircleToolTip.PNG";
                return circleCRBtn;
            }
        }
    }
    public static partial class RibbonTool
    {
        /// <summary>
        /// 添加Ribbon选项卡
        /// </summary>
        /// <param name="ribbonCtrl">Ribbon控制器</param>
        /// <param name="title">选项卡标题</param>
        /// <param name="ID">选项卡ID</param>
        /// <param name="isActive">是否置为当前</param>
        /// <returns>RibbonTab</returns>
        public static RibbonTab AddTab(this RibbonControl ribbonCtrl, string title, string ID, bool isActive)
        {
            RibbonTab tab = new RibbonTab();
            tab.Title = title;
            tab.Id = ID;
            ribbonCtrl.Tabs.Add(tab);
            tab.IsActive = isActive;
            return tab;
        }
        /// <summary>
        /// 添加面板
        /// </summary>
        /// <param name="tab">Ribbon选项卡</param>
        /// <param name="title">面板标题</param>
        /// <returns>RibbonPanelSource</returns>
        public static RibbonPanelSource AddPanel(this RibbonTab tab, string title)
        {
            RibbonPanelSource panelSource = new RibbonPanelSource();
            panelSource.Title = title;
            RibbonPanel ribbonPanel = new RibbonPanel();
            ribbonPanel.Source = panelSource;
            tab.Panels.Add(ribbonPanel);
            return panelSource;
        }
        /// <summary>
        /// 给面板添加下拉组合按钮
        /// </summary>
        /// <param name="panelSource"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="orient"></param>
        /// <returns></returns>
        public static RibbonSplitButton AddSplitButton(this RibbonPanelSource panelSource, string text, RibbonItemSize size, Orientation orient)
        {
            RibbonSplitButton splitBtn = new RibbonSplitButton();
            splitBtn.Text = text;
            splitBtn.ShowText = true;
            splitBtn.Size = size;
            splitBtn.ShowImage = true;
            splitBtn.Orientation = orient;
            panelSource.Items.Add(splitBtn);
            return splitBtn;
        }
    }
    public class RibbonCommandHandler : System.Windows.Input.ICommand
    {
        //定义用于确定此命令是否可以在其当前状态下执行的方法。
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
        // 定义在调用此命令时调用的方法。
        public void Execute(object parameter)
        {
            if (parameter is RibbonButton)
            {
                RibbonButton btn = (RibbonButton)parameter;
                if (btn.CommandParameter != null)
                {
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    doc.SendStringToExecute(btn.CommandParameter.ToString(), true, false, false);
                }
            }
        }
    }
    public class RibbonButtonEX : RibbonButton
    {
        //正常显示的图片
        private string imgFileName = "";
        public string ImgFileName
        {
            get { return imgFileName; }
            set { imgFileName = value; }
        }
        //鼠标进入时的图片
        private string imgHoverFileName = "";
        public string ImgHoverFileName
        {
            get { return imgHoverFileName; }
            set { imgHoverFileName = value; }
        }

        public RibbonButtonEX(string name, RibbonItemSize size, Orientation orient, string cmd)
            : base()
        {
            this.Name = name;//按钮的名称
            this.Text = name;
            this.ShowText = true; //显示文字
            this.MouseEntered += this_MouseEntered;
            this.MouseLeft += this_MouseLeft;
            this.Size = size; //按钮尺寸
            this.Orientation = orient; //按钮排列方式
            this.CommandHandler = new RibbonCommandHandler(); //给按钮关联命令
            this.CommandParameter = cmd + " ";
            this.ShowImage = true; //显示图片
        }
        public void SetImg(string imgFileName)
        {
            Uri uri = new Uri(imgFileName);
            BitmapImage bitmapImge = new BitmapImage(uri);
            this.Image = bitmapImge; //按钮图片
            this.LargeImage = bitmapImge; //按钮大图片
            this.imgFileName = imgFileName;
        }
        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void this_MouseLeft(object sender, EventArgs e)
        {
            if (this.ImgFileName != "")
            {
                RibbonButton btn = (RibbonButton)sender;
                string imgFileName = this.ImgFileName;
                Uri uri = new Uri(imgFileName);
                BitmapImage bitmapImge = new BitmapImage(uri);
                btn.Image = bitmapImge; //按钮图片
                btn.LargeImage = bitmapImge; //按钮大图片
            }

        }
        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void this_MouseEntered(object sender, EventArgs e)
        {
            if (this.ImgHoverFileName != "")
            {
                RibbonButton btn = (RibbonButton)sender;
                string imgFileName = this.ImgHoverFileName;
                Uri uri = new Uri(imgFileName);
                BitmapImage bitmapImge = new BitmapImage(uri);
                btn.Image = bitmapImge; //按钮图片
                btn.LargeImage = bitmapImge; //按钮大图片
            }

        }
    }
    public static class CurPath
    {
        public static string curPath = "";
    }
}
