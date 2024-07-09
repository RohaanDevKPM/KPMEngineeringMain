using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using static System.Windows.Forms.LinkLabel;
using Microsoft.Office.Interop.Excel;
using View = Autodesk.Revit.DB.View;
using Autodesk.Revit.DB.Architecture;

namespace KPMEngineeringB.R
{
    public partial class UpdateCircuitLoadName : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        UIDocument UiDoc;
        public List<Document> linkDocs = new List<Document>();
        public List<string> linkNames = new List<string>();
       
        public List<Element> collectCkt = new List<Element>();
        public UpdateCircuitLoadName(Document doc, ExternalCommandData commandData)
        {
            InitializeComponent();

            Doc = doc;
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            UiDoc = uiDoc;
        }

        private void UpdateCircuitLoadName_Load(object sender, EventArgs e)
        {
            FilteredElementCollector collectorLink = new FilteredElementCollector(Doc).OfClass(typeof(RevitLinkInstance));
            List<Element> collectLink = collectorLink.ToElements().ToList();

            // Need to Clear so no duplicate 
            listView1.Items.Clear();
            linkNames.Add("Current Project");
          
            foreach (Element link in collectLink)
            {               
                Document linkedDoc = (link as RevitLinkInstance).GetLinkDocument();
                linkDocs.Add(linkedDoc);

                ElementId typeId = (link as RevitLinkInstance).GetTypeId();
                ElementType linkType = Doc.GetElement(typeId) as ElementType;

                // Get the link file name without extension
                Autodesk.Revit.DB.Parameter linkFileNameParam = linkType.get_Parameter(BuiltInParameter.RVT_LINK_FILE_NAME_WITHOUT_EXT);
                
                string linkFileName = linkFileNameParam.AsString();
                linkNames.Add(linkFileName);
            }
            Autodesk.Revit.DB.View activeView = Doc.ActiveView;
            FilteredElementCollector collectorEleEq = new FilteredElementCollector(Doc, activeView.Id).OfCategory(BuiltInCategory.OST_ElectricalEquipment).WhereElementIsNotElementType();
            IList<Element> collectEleEq = collectorEleEq.ToElements();

            listView1.View = System.Windows.Forms.View.Details;
            listView1.HeaderStyle = ColumnHeaderStyle.None;
            ColumnHeader header = new ColumnHeader();

            listView1.Columns.Add(header);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

           
            foreach (Element eleEq in collectEleEq)
            {
                if (eleEq is FamilyInstance familyInstance)
                {
                    if ((eleEq as FamilyInstance).MEPModel.ElectricalSystems != null && !eleEq.Category.Name.Contains(".dwg"))
                    {
                        var getCkt = (eleEq as FamilyInstance).MEPModel.ElectricalSystems;

                        foreach (Element system in getCkt)
                        {
                            // Check if the system is a power circuit
                            if (system is ElectricalSystem electricalSystem && electricalSystem.SystemType == ElectricalSystemType.PowerCircuit)
                            {
                                string panelName = electricalSystem.PanelName ?? "No Panel Name";

                                collectCkt.Add(electricalSystem);

                                listView1.Items.Add(panelName);
                            }
                        }
                    }
                }
            }

            comboBox1.DataSource = linkNames;
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        // Update 
        private void button1_Click(object sender, EventArgs e)
        {

            var selectedIndex = comboBox1.SelectedIndex;
            IList <Element> collectRooms = new List<Element>(); 
            if (selectedIndex > 0)
            {
                var selectedElementLink = linkDocs[selectedIndex - 1];
                // Get the active view
                View activeView = Doc.ActiveView;

                // Get the elevation of the active view's level
                double getLvl = Math.Round(activeView.GenLevel.Elevation, 4);

                // Collect levels from the linked document
                Level linkLvl = null;
                FilteredElementCollector collectLvl = new FilteredElementCollector(selectedElementLink)
                    .OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType();
                foreach (Level lvl in collectLvl)
                {
                    if (Math.Round(lvl.Elevation, 4) == getLvl)
                    {
                        linkLvl = lvl;
                        break;
                    }
                }

                // Create a filter to get elements at the linked level
                ElementLevelFilter eleLvlFilter = new ElementLevelFilter(linkLvl.Id);
                // Collect rooms at the linked level
                collectRooms = new FilteredElementCollector(selectedElementLink)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WherePasses(eleLvlFilter)
                    .WhereElementIsNotElementType().ToElements();
            }
            else if(selectedIndex == 0)
            {
                // Get the active view
                View activeView = Doc.ActiveView;

                var Lev = activeView.GenLevel;

                // Create a filter to get elements at the linked level
                ElementLevelFilter eleLvlFilter = new ElementLevelFilter(Lev.Id);

                // Collect rooms at the linked level
                 collectRooms = new FilteredElementCollector(Doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WherePasses(eleLvlFilter)
                    .WhereElementIsNotElementType().ToElements();

            }
            ISet<ElectricalSystem> setElecSysList = new HashSet<ElectricalSystem>();
               
               
            foreach (int item in listView1.CheckedIndices)
            {
                Element element = collectCkt[item]; 

                // Explicitly cast the Element to an ElectricalSystem
                ElectricalSystem electricalSystem = element as ElectricalSystem;

                // Check if the cast was successful
                if (electricalSystem != null)
                {
                // Add the ElectricalSystem to the set
                setElecSysList.Add(electricalSystem);
                }
                   
            }
           
            using (Transaction transaction = new Transaction(Doc, "Update Circuit Load Name"))
            {
                transaction.Start();

                // Create a filter for multiple categories
                List<BuiltInCategory> categories = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_ElectricalEquipment,
                    BuiltInCategory.OST_LightingFixtures,
                    BuiltInCategory.OST_ElectricalFixtures
                };
                ElementMulticategoryFilter eleMultiCatFilter = new ElementMulticategoryFilter(categories);

                // Initialize lists for collected data
                List<object> checkData = new List<object>();

                // Process each selected electrical circuit
                foreach (var ckt in setElecSysList) 
                {
                    // Collect elements associated with the current circuit
                    ICollection<Element> elements = ckt.Elements.Cast<Element>().ToList();
                 
                    string loadName = "";
                    List<string> uniqueRoom = new List<string>();
                    List<string> uniqueDesc = new List<string>();
                    List<ElementId> collectedElementsIds = elements.Select(el => el.Id).ToList();
                    

                    // Process each room at the linked level
                    foreach (Room room in collectRooms)
                    {
                        if (room.Area > 0)
                        {
                            // Get the geometry of the room
                            GeometryElement geom = room.get_Geometry(new Options());
                            Solid roomSolid = null;
                            foreach (GeometryObject obj in geom)
                            {
                                roomSolid = obj as Solid;
                            }

                            // Create filters to find elements intersecting with the room
                            ElementIntersectsSolidFilter eleInterEleFilter = new ElementIntersectsSolidFilter(roomSolid);
                            BoundingBoxXYZ bbox = room.get_BoundingBox(null);
                            Autodesk.Revit.DB.Outline outline = new Autodesk.Revit.DB.Outline(bbox.Min, bbox.Max);
                            BoundingBoxIntersectsFilter bbIntersectFilter = new BoundingBoxIntersectsFilter(outline);

                            // Collect intersecting elements
                            var intersectingElements = new FilteredElementCollector(Doc)
                                .WherePasses(eleMultiCatFilter)
                                .WherePasses(bbIntersectFilter)
                                .WherePasses(eleInterEleFilter).ToElements();

                            if (intersectingElements.Count > 0)
                            {
                                // Process each intersecting element
                                foreach (Element intersect in intersectingElements)
                                {
                                    if (collectedElementsIds.Contains(intersect.Id))
                                    {
                                        ElementType intersectType = Doc.GetElement(intersect.GetTypeId()) as ElementType;
                                        string eleDescription = intersectType?.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)?.AsString();
                                        if (!string.IsNullOrEmpty(eleDescription) && !uniqueDesc.Contains(eleDescription))
                                        {
                                            uniqueDesc.Add(eleDescription);
                                        }
                                        string eleRoom = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
                                        if (!string.IsNullOrEmpty(eleRoom) && !uniqueRoom.Contains(eleRoom))
                                        {
                                            uniqueRoom.Add(eleRoom);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (uniqueDesc.Count > 0)
                    {
                        // Construct the load name from unique descriptions and rooms
                        foreach (string desc in uniqueDesc)
                        {
                            loadName += desc;
                            if (uniqueDesc.Last() != desc)
                            {
                                loadName += " + ";
                            }
                        }
                        loadName += "_";
                        foreach (string room in uniqueRoom)
                        {
                            loadName += room;
                            if (uniqueRoom.Last() != room)
                            {
                                loadName += ", ";
                            }
                        }
                        // Set the load name parameter of the circuit
                        Autodesk.Revit.DB.Parameter loadNameParam = ckt.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_NAME);
                        if (loadNameParam != null)
                        {
                            loadNameParam.Set(loadName);
                            checkData.Add(loadNameParam);
                        }

                    }
                }
                transaction.Commit();
                if (checkData.Count > 0)
                {
                    TaskDialog.Show("Output", $"{checkData.Count} Circuits Updated!");
                    Close();
                }
                else
                {
                    TaskDialog.Show("Not Found", "No Elements were found inside the Rooms, Please check the Room Boundary.");
                    Close();
                }
            }
        }
        // Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
        // Select All 
        private void button3_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = true;
            }
        }
        // Select None
        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = false;
            }

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = true;
            }
        }
    }
}
