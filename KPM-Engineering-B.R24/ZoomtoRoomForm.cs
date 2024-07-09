using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using CheckBox = System.Windows.Forms.CheckBox;
using ComboBox = System.Windows.Forms.ComboBox;
using Document = Autodesk.Revit.DB.Document;
using Form = System.Windows.Forms.Form;
using Level = Autodesk.Revit.DB.Level;
using Point = System.Drawing.Point;
using View = Autodesk.Revit.DB.View;

namespace KPMEngineeringB.R
{
    // Task to Zoom to Room in Linked Model 
    public partial class ZoomtoRoomForm : Form
    {
        Autodesk.Revit.DB.Document Doc;
        UIDocument UiDoc;
        // for select list
        private List<RevitLinkInstance> linkInstances = new List<RevitLinkInstance>();
        private IList<Element> levelInstances = new List<Element>();
        private IList<Element> roomInstances = new List<Element>();
        public ZoomtoRoomForm(Document doc,ExternalCommandData commandData) // Document doc
        {
            InitializeComponent();
            Doc = doc;
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            UiDoc = uiDoc;
          
        }
        private void ZoomtoRoomForm_Load(object sender, EventArgs e)
        {
            //Position Form 
            int screenWidth = Screen.PrimaryScreen.Bounds.Size.Width;
            int formWidth = this.Width;
            this.Location = new Point(screenWidth-formWidth-20,0);

            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            List<RevitLinkInstance> linkedInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks)
                                                          .OfClass(typeof(RevitLinkInstance))
                                                           .Cast<RevitLinkInstance>()
                                                           .ToList();

            IList<string> linkList = new List<string>();

            foreach (var linkedInstance in linkedInstances)
            {
                if (linkedInstance != null)
                {
                    linkInstances.Add(linkedInstance); // for Select
                    string l = linkedInstance.Name;
                    linkList.Add(l);
                }
            }
            comboBox1.DataSource = linkList; // linkList  
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            var lin = comboBox1.SelectedIndex;

            var linkElement = linkInstances[lin];
            Document linkedDocument = linkElement.GetLinkDocument();
            IList<Element> level = new FilteredElementCollector(linkedDocument).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements(); // .ToElements()

            var levels = level.OfType<Level>().ToList();

            IList<string> levelList = new List<string>();
            for (int i = 0; i < levels.Count; i++)
            {
                string levelName = levels[i].Name;
                levelInstances.Add(level[i]);
                levelList.Add(levelName);
            }
            comboBox2.DataSource = levelList;            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Important to clear List or else it will Override
            roomInstances.Clear(); // need to clear list so correct room zoom whichever room you select

            var link = comboBox1.SelectedIndex;
            var lvl = comboBox2.SelectedIndex; // for level in combo2 selected
      
            var linkLevel = levelInstances[(lvl)];
            var linkEle = linkInstances[(link)];

            Document linkedDocument = linkEle.GetLinkDocument();

            var eleLvlFilter = new ElementLevelFilter(linkLevel.Id);

            IList<Element> linkRooms = new FilteredElementCollector(linkedDocument).OfCategory(BuiltInCategory.OST_Rooms).WherePasses(eleLvlFilter).WhereElementIsNotElementType().ToElements(); // .ToElements()
            IList<string> roomsList = new List<string>();

            foreach (var linkRoom in linkRooms)
            {
                roomsList.Add(linkRoom.Name);
                roomInstances.Add(linkRoom); // for selection
            }
            comboBox3.DataSource = roomsList;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            var rooM = comboBox3.SelectedIndex;
            var rE = roomInstances[rooM];

            // Zoom to Room 
            BoundingBoxXYZ bb = rE.get_BoundingBox(null) as BoundingBoxXYZ; 

            if (bb != null)
            {
                XYZ min = bb.Min;
                XYZ max = bb.Max;

                ElementId activeViewId = UiDoc.ActiveView.Id;

                // Get all open UI views
                List<UIView> uiViews = new List<UIView>();
                foreach (UIView uiView in UiDoc.GetOpenUIViews())
                {
                    if (activeViewId == uiView.ViewId)
                    {
                        uiViews.Add(uiView);
                    }
                }

                // Zoom and center the view on the element's bounding box
                if (uiViews.Count > 0)
                {
                    UIView viewToZoom = uiViews[0]; // Assuming there is only one open UI view for the active view

                    // Calculate the diagonally opposite corners of the rectangle to zoom and center
                    XYZ viewCorner1 = new XYZ(min.X, min.Y, 0);
                    XYZ viewCorner2 = new XYZ(max.X, max.Y, 0);

                    // Zoom and center the view on the specified rectangle
                    viewToZoom.ZoomAndCenterRectangle(viewCorner1, viewCorner2);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }
    }
}
