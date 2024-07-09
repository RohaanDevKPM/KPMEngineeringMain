using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using Category = Autodesk.Revit.DB.Category;
using Document = Autodesk.Revit.DB.Document;
using Form = System.Windows.Forms.Form;
using Level = Autodesk.Revit.DB.Level;
using Line = Autodesk.Revit.DB.Line;
using View = Autodesk.Revit.DB.View;
using View3D = Autodesk.Revit.DB.View3D;

namespace KPMEngineeringB.R
{
    public partial class Cad_to_Revit_Family_Placement : Form
    {
        Autodesk.Revit.DB.Document Doc;
        UIDocument UiDoc;
        public static List<Element> importInstances = new List<Element>();
        public static List<Category> CatSelect = new List<Category>();
        public static List<string> familyTypeNames = new List<string>();
        public static IList<Element> famElem = new List<Element>();

        public List<string> CatList = new List<string>();
       
        public static List<string> blockNames = new List<string>();
        
        private ExternalCommandData storedCommandData;
        public Cad_to_Revit_Family_Placement(Document doc, ExternalCommandData commandData)
        {
            InitializeComponent();
            Doc = doc;
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            UiDoc = uiDoc;
            storedCommandData = commandData;
            importInstances.Clear();
            CatSelect.Clear();
            familyTypeNames.Clear();
            famElem.Clear();
            CatList.Clear();
            blockNames.Clear();
        }

        private void Cad_to_Revit_Family_Placement_Load(object sender, EventArgs e)
        {
            // Get the active view
            Autodesk.Revit.DB.View activeView = Doc.ActiveView;

            // Collect all ImportInstance elements in the active view   
            importInstances = new FilteredElementCollector(Doc, activeView.Id).OfClass(typeof(ImportInstance)).WhereElementIsNotElementType().ToList();

            // Collect the names of the categories of the import instances
            var importNames = importInstances.Select(importInstance => importInstance.Category.Name).ToList();
             

            comboBox1.DataSource = importNames;
                
            List<Category> TempCat = new List<Category>();
            var all_categories = Doc.Settings.Categories;
            foreach (var category in all_categories)
            {
                if (((Autodesk.Revit.DB.Category)category).CategoryType == CategoryType.Model && !((Autodesk.Revit.DB.Category)category).Name.Contains(".dwg"))
                {
                    CatList.Add(((Autodesk.Revit.DB.Category)category).Name);
                    TempCat.Add((Autodesk.Revit.DB.Category)category); // for select
                }
            }

            List<int> sortedIndices = Enumerable.Range(0, CatList.Count).OrderBy(i => CatList[i]).ToList();
            var sortedListB = sortedIndices.Select(i => TempCat[i]).ToList();
            CatSelect = CatSelect.Concat(sortedListB).ToList();
            CatList.Sort();

            cb_Select_Category.DataSource = CatList;
            cb_Select_Category.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
            foreach (var importInstance in importInstances)
            {
                var geomElement = importInstance.get_Geometry(new Options());

                foreach (var geomObject in geomElement)
                {
                    if (geomObject is GeometryInstance geomInstance)
                    {
                        foreach (var innerGeomObject in geomInstance.SymbolGeometry)
                        {
                            if (innerGeomObject is GeometryInstance innerGeomInstance)
                            {
                                string blockName = innerGeomInstance.Symbol.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
                                string EndPart = blockName.Split('.').Last(); // Extract the end part of the block name
                                if (!blockNames.Contains(EndPart))
                                {
                                    blockNames.Add(EndPart);
                                }
                            }
                        }
                    }
                }
            }

            comboBox2.DataSource = blockNames;
       
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
      
        // Select Category name Combox 4
        private void cb_Select_Category_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cat = cb_Select_Category.SelectedIndex;
            var eleCat = CatSelect[cat].Id;
            famElem = new FilteredElementCollector(Doc).OfCategoryId(eleCat).WhereElementIsElementType().ToElements();
            
            if (famElem.Count > 0)
            {
                familyTypeNames = famElem
               .Select(element => element.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString() + " - " + element.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString())
                .ToList();
                // Temporarily remove the event handler
                cb_Family_Type.SelectedIndexChanged -= cb_Family_Type_SelectedIndexChanged;
              
                cb_Family_Type.DataSource = familyTypeNames;
                
                // Reattach the event handler
                cb_Family_Type.SelectedIndexChanged += cb_Family_Type_SelectedIndexChanged;
            }
            else
            {
                // Temporarily remove the event handler
                cb_Family_Type.SelectedIndexChanged -= cb_Family_Type_SelectedIndexChanged;
                cb_Family_Type.DataSource = null;
                // Reattach the event handler
                cb_Family_Type.SelectedIndexChanged += cb_Family_Type_SelectedIndexChanged;
            }
        }
        // Family Type 
        private void cb_Family_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedFam = cb_Family_Type.SelectedIndex;
            var familySym = famElem[selectedFam] as FamilySymbol;
            var colPara = familySym.Family.get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR);
            
            var value = colPara.AsInteger();
            var valString = colPara.AsValueString();
            var workP = familySym.Family.get_Parameter(BuiltInParameter.FAMILY_WORK_PLANE_BASED);
            var wP = workP.AsValueString();
            if (value == 0)
            {
                if (wP == "Yes")
                {
                    label4.Visible = true;
                    textBox1.Visible = true;
                    label6.Text = "Family Host: WorkPlane Based";
                }
                else
                {
                    label4.Visible = true;
                    textBox1.Visible = true;
                    label6.Text = "Family Host: Face Based";
                }
            }
           
            else if (valString == "Ceiling")
            {
                label4.Hide();
                textBox1.Hide();
                label6.Text = "Family Host: Ceiling Based";
            }
            else if (valString == "Floor")
            {

                label4.Hide();
                textBox1.Hide();
                label6.Text = "Family Host: Floor Based";
            }
            else if (valString == "Face")
            {
                label6.Text = "Family Host: Face Based";
            }
        }

        // Done
        private void button1_Click(object sender, EventArgs e)
        {
            var selectedFamilyType = cb_Family_Type.SelectedItem.ToString();
            var familyName = selectedFamilyType.Split('-')[0].Trim();
            var typeName = selectedFamilyType.Split('-')[1].Trim();

            var selectedFam = cb_Family_Type.SelectedIndex;
            var familySym = famElem[selectedFam] as FamilySymbol;
            double symbolAngle = 0;
            int totalAttempts = 0;
            if (familySym == null)
            {
                TaskDialog.Show("Error", "Family Symbol not found.");
                return;
            }

            if (radioButton1.Checked)
            {
                symbolAngle = 0;
            }
            else
            {
                symbolAngle = (90 * (Math.PI / 180.0));
            }

            // Validate offset input
            double offset = 0;
            if (textBox1.Visible && !double.TryParse(textBox1.Text, out offset))
            {
                TaskDialog.Show("Error", "Invalid offset value.");
                return;
            }

            // Ensure family symbol is active
            if (!familySym.IsActive)
            {
                using (Transaction t = new Transaction(Doc, "Activate Family Symbol"))
                {
                    t.Start();
                    familySym.Activate();
                    Doc.Regenerate();
                    t.Commit();
                }
            }

            string selectedBlockName = comboBox2.SelectedItem.ToString();
            string CADFileName = importInstances[comboBox1.SelectedIndex].Category.Name;
            string RevitName = CADFileName + "." + selectedBlockName;

            Options geomOptions = new Options();
            ImportInstance selectedImportInstance = importInstances[comboBox1.SelectedIndex] as ImportInstance;
            GeometryElement geomElement = selectedImportInstance.get_Geometry(geomOptions);

            Autodesk.Revit.DB.View activeView = Doc.ActiveView;
            Autodesk.Revit.DB.Level level = activeView.GenLevel;
            XYZ elevation = new XYZ(0, 0, (offset / 304.8));

            XYZ transformOrigin = new XYZ(selectedImportInstance.GetTransform().Origin.X, selectedImportInstance.GetTransform().Origin.Y, 0);
            var activeLvl = Doc.ActiveView.GenLevel;
            var lvlElev = new XYZ(0, 0, activeLvl.ProjectElevation);
            StructuralType structuralType = StructuralType.NonStructural;

            List<GeometryElement> geomList = new List<GeometryElement>();
            foreach (GeometryObject geomObject in geomElement)
            {
                if (geomObject is GeometryInstance geomInstance)
                {
                    geomList.Add(geomInstance.SymbolGeometry);
                }
            }

            List<FamilyInstance> placedFamilies = new List<FamilyInstance>();

            using (Transaction trans = new Transaction(Doc, "Place Revit Families"))
            {
                trans.Start();

                // Get the 3D view family type (default 3D view)
                var viewThreeD = new FilteredElementCollector(Doc)
                    .OfClass(typeof(View3D))
                   .ToList();
                var viewFamilyType = new FilteredElementCollector(Doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

                bool found = true;
                Element select3D = null;
                foreach (var n in viewThreeD)
                {
                    if (n.Name == "Reference Three-D View")
                    {
                        found = true;
                        select3D = n;
                        break;
                    }
                    else
                    {
                        found = false;
                    }
                }
                View3D new3DView = null;
                if (found)
                {
                    Doc.Delete(select3D.Id);
                    new3DView = View3D.CreateIsometric(Doc, viewFamilyType.Id);
                    new3DView.Name = "Reference Three-D View";
                }
                else
                {
                    new3DView = View3D.CreateIsometric(Doc, viewFamilyType.Id);
                    new3DView.Name = "Reference Three-D View";
                    new3DView.DetailLevel = ViewDetailLevel.Fine;
                    // Ensure Ceilings and Floors are visible
                    var ceilingsCategory = Doc.Settings.Categories.get_Item(BuiltInCategory.OST_Ceilings);
                    if (ceilingsCategory != null)
                    {
                        new3DView.SetCategoryHidden(ceilingsCategory.Id, false);
                    }

                    var floorsCategory = Doc.Settings.Categories.get_Item(BuiltInCategory.OST_Floors);
                    if (floorsCategory != null)
                    {
                        new3DView.SetCategoryHidden(floorsCategory.Id, false);
                    }
                }
                List<List<XYZ>> polylineCoordinates = new List<List<XYZ>>();
                IList<XYZ> polylineSCord = new List<XYZ>();
                List<Line> li = new List<Line>();
                var max = new Object();
                
                double angle = 0;
                var aVp = Doc.ActiveView as ViewPlan;

                foreach (GeometryElement geoElem in geomList)
                {
                    foreach (GeometryObject geoObj in geoElem)
                    {
                        if (geoObj is GeometryInstance geoInstance)
                        {
                            if (geoInstance.Symbol.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == RevitName)
                            {
                                Line longestLine = null;
                                //XYZ vectorLine;
                                var symbolGeometry = geoInstance.GetInstanceGeometry();
                                bool isPolyline = true;
                                foreach (var trySymbObj in symbolGeometry)
                                {
                                    if (trySymbObj.GetType().Name != "PolyLine")
                                    {
                                        isPolyline = false;
                                        break;
                                    }
                                }
                                if (isPolyline)
                                {
                                    foreach (var symbObj in symbolGeometry)
                                    {
                                        var polyline = symbObj as PolyLine;
                                        var coordinatesList = polyline.GetCoordinates();
                                        var ac1 = Doc.ActiveView;
                                        var lvl1 = ac1.GenLevel;
                                        var elevatioN1 = lvl1.ProjectElevation;
                                        li.Clear();
                                        for (int i = 0; i < coordinatesList.Count - 1; i++)
                                        {
                                            XYZ startPoint = coordinatesList[i];
                                            XYZ endPoint = coordinatesList[i + 1];

                                            Line geomLine = Line.CreateBound(startPoint, endPoint);
                                            li.Add(geomLine);
/*
                                            var planE = Plane.CreateByNormalAndOrigin(Doc.ActiveView.ViewDirection, new XYZ(0, 0, startPoint.Z));
                                            var sketcH = SketchPlane.Create(Doc, planE);
                                            var newLin3 = Doc.Create.NewModelCurve(geomLine, sketcH);*/

                                        }
                                        // Identify the longest line
                                        foreach (Line line in li)
                                        {
                                            if (longestLine == null || line.Length > longestLine.Length)
                                            {

                                                //longestLine = line;

                                                if (longestLine != null)
                                                {
                                                    XYZ directionVector = line.Direction;
                                                    if (directionVector.Y >= 0)
                                                    {
                                                        longestLine = line;
                                                    }
                                                }
                                                else
                                                {
                                                    longestLine = line;
                                                }
                                            }
                                        }

                                        if (longestLine == null)
                                        {
                                            TaskDialog.Show("Error", "No lines found.");
                                            return;
                                        }

                                        var ac = Doc.ActiveView;
                                        var lvl = ac.GenLevel;
                                        var elevatioN = lvl.ProjectElevation;

                                        if (longestLine != null)
                                        {
/*
                                            var planE = Plane.CreateByNormalAndOrigin(Doc.ActiveView.ViewDirection, new XYZ(0, 0, longestLine.GetEndPoint(0).Z));
                                            var sketcH = SketchPlane.Create(Doc, planE);
                                            var newLin3 = Doc.Create.NewModelCurve(longestLine, sketcH);*/
                                            var vectorLine = longestLine.GetEndPoint(1) - longestLine.GetEndPoint(0);
                                            angle = vectorLine.AngleTo(XYZ.BasisX) + symbolAngle;
                                            var dir = longestLine.Direction.ToString();
                                            ///TaskDialog.Show("Angle & Dir", dir + " "+ (angle * (180 / Math.PI)).ToString());
                                        }
                                        else
                                        {
                                            TaskDialog.Show("Error", "No lines found.");
                                        }
                                    }
                                }
                                else
                                {
                                    angle = 0 + symbolAngle;
                                }
                                if (geoInstance.Symbol.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == RevitName)
                                {
                                    totalAttempts++;

                                    XYZ point = geoInstance.Transform.Origin + transformOrigin + lvlElev;
                                    FamilyInstance familyInstance = null;
                                    var axisLine = Line.CreateBound(new XYZ(point.X, point.Y, point.Z), new XYZ(point.X, point.Y, point.Z + 1));

                                    var colPara = familySym.Family.get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR);
                                    var value = colPara.AsInteger();
                                    var valString = colPara.AsValueString();

                                    var workP = familySym.Family.get_Parameter(BuiltInParameter.FAMILY_WORK_PLANE_BASED);
                                    var wP = workP.AsValueString();

                                    if (value == 0)
                                    {
                                        if (wP == "Yes")
                                        {
                                            var host = Doc.ActiveView.GenLevel;
                                            familyInstance = Doc.Create.NewFamilyInstance(point, familySym, host, level, structuralType);
                                            familyInstance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(offset / 304.8);

                                            ElementTransformUtils.RotateElement(Doc, familyInstance.Id, axisLine, angle);
                                            placedFamilies.Add(familyInstance);
                                        }
                                        else
                                        {
                                            familyInstance = Doc.Create.NewFamilyInstance(point, familySym, level, structuralType);
                                            familyInstance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(offset / 304.8);
                                            ElementTransformUtils.RotateElement(Doc, familyInstance.Id, axisLine, angle);
                                            placedFamilies.Add(familyInstance);

                                        }
                                    }
                                    else if (valString == "Floor")
                                    {
                                        var categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
                                        var findRefTarget = FindReferenceTarget.Element;
                                        var riFirstCatElem = new ReferenceIntersector(categoryFilter, findRefTarget, new3DView);
                                        var dir = new XYZ(0, 0, -1);

                                        var singleRef = riFirstCatElem.FindNearest(point + new XYZ(0, 0, 500 / 304.8), dir);
                                        if (singleRef != null)
                                        {
                                            var singleGetRef = singleRef.GetReference();
                                            var singleElem = Doc.GetElement(singleGetRef.ElementId);

                                            var hostEle = Doc.GetElement(singleElem.Id);

                                            PlanViewRange viewRange = aVp.GetViewRange();
                                            var topClipPlaneLevel = Doc.GetElement(viewRange.GetLevelId(PlanViewPlane.TopClipPlane)) as Level;
                                            var bottomClipPlaneLevel = Doc.ActiveView.GenLevel;

                                            if (singleGetRef.GlobalPoint.Z >= bottomClipPlaneLevel.Elevation)
                                            {
                                                familyInstance = Doc.Create.NewFamilyInstance(point, familySym, hostEle, level, structuralType);
                                                ElementTransformUtils.RotateElement(Doc, familyInstance.Id, axisLine, angle);
                                                placedFamilies.Add(familyInstance);
                                            }
                                        }

                                    }
                                    else if (valString == "Ceiling")
                                    {
                                        var categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Ceilings);
                                        var findRefTarget = FindReferenceTarget.Element;
                                        var riFirstCatElem = new ReferenceIntersector(categoryFilter, findRefTarget, new3DView);
                                        var dir = new XYZ(0, 0, 1);
                                        var singleRef = riFirstCatElem.FindNearest(point, dir);
                                        if (singleRef != null)
                                        {
                                            var singleGetRef = singleRef.GetReference();
                                            var singleElem = Doc.GetElement(singleGetRef.ElementId);

                                            var hostEle = Doc.GetElement(singleElem.Id);

                                            PlanViewRange viewRange = aVp.GetViewRange();
                                            var topClipPlaneLevel = Doc.GetElement(viewRange.GetLevelId(PlanViewPlane.TopClipPlane)) as Level;

                                            if (singleGetRef.GlobalPoint.Z < topClipPlaneLevel.ProjectElevation)
                                            {
                                                familyInstance = Doc.Create.NewFamilyInstance(point, familySym, hostEle, level, structuralType);
                                                ElementTransformUtils.RotateElement(Doc, familyInstance.Id, axisLine, angle);
                                                placedFamilies.Add(familyInstance);
                                            }
                                        }
                                    }
                                    else if (valString == "Face")
                                    {
                                        var hostEle = Doc.ActiveView.GenLevel;
                                        familyInstance = Doc.Create.NewFamilyInstance(point, familySym, hostEle, level, structuralType);
                                        ElementTransformUtils.RotateElement(Doc, familyInstance.Id, axisLine, angle);

                                        ElementTransformUtils.MoveElement(Doc, familyInstance.Id, new XYZ(0, 0, (offset / 304.8)));
                                        if (offset.ToString() != (familyInstance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).AsDouble() * 304.8).ToString())
                                        {
                                            TaskDialog.Show("Plugin Output", "Selected Family is not supported.");
                                            break;
                                        }
                                        else
                                        {
                                            placedFamilies.Add(familyInstance);
                                        }
                                    }
                                    else
                                    {
                                        TaskDialog.Show("Plugin Output", "Currently Unsupported Revit Family Selected.");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                int familyCount = placedFamilies.Count;
                int notPlacedCount = totalAttempts - familyCount;
                if (familyCount > 0)
                {
                    if (notPlacedCount > 0)
                    {
                        TaskDialog.Show("Plugin Output", $"Total {familyCount} Number of Revit Families Placed. \n{notPlacedCount} Revit Families Not Placed.");
                    }
                    else
                    {
                        TaskDialog.Show("Plugin Output", $"Total {familyCount} Number of Revit Families Placed.");
                    }
                    trans.Commit();
                }
                else
                {
                    trans.RollBack();
                }

            }
        }
      

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
            }
        }
    }
}
