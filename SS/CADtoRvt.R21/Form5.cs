using Autodesk.Revit.Creation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Net;
using Autodesk.Revit.DB.Electrical;
using System.Xml.Linq;
using System.Windows.Controls;

namespace KPMEngineeringB.R
{
    public partial class Form5 : System.Windows.Forms.Form
    {
        private IList<Line> lineList = new List<Line>();
        
        private IList<string> lineType = new List<string> { "Detail Line", "Model Line"};

        private IList<string> lineStyle = new List<string>();

        private IList<PolyLine> polyList = new List<PolyLine>();       
        private static Category eleLineStyle {  get; set;}
        private static XYZ CADOrigin { get; set; }

        Autodesk.Revit.DB.Document Doc;
        public Form5(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            // CAD Files
            IList<Element> cadFiles = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            cb_Line_Style.Items.Clear();
            try
            {
                IList<string> cadFileList = new List<string>();
                foreach (ImportInstance cadFile in cadFiles)
                {
                    cadFileList.Add(cadFile.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString());
                }
                cb_CAD_Name.DataSource = cadFileList;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_CAD_Name.SelectedIndex = 0;

            try
            {
                cb_Line_Type.DataSource = lineType;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Line_Type.SelectedIndex = 0;

            Category lineCategory = Doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            CategoryNameMap subCategory = lineCategory.SubCategories;
            foreach ( Category lineS in subCategory)
            {
                if (lineS.Id != new ElementId(BuiltInCategory.OST_AreaSchemeLines) && lineS.Id != new ElementId(BuiltInCategory.OST_FabricAreaSketchEnvelopeLines)
                    && lineS.Id != new ElementId(BuiltInCategory.OST_FabricAreaSketchSheetsLines) && lineS.Id != new ElementId(BuiltInCategory.OST_RoomSeparationLines)
                    && lineS.Id != new ElementId(BuiltInCategory.OST_SketchLines) && lineS.Id != new ElementId(BuiltInCategory.OST_MEPSpaceSeparationLines)
                    && lineS.Id != new ElementId(BuiltInCategory.OST_AxisOfRotation) && lineS.Id != new ElementId(BuiltInCategory.OST_InsulationLines))
                {
                    lineStyle.Add(lineS.Name.ToString());
                }

                else
                {
                    ///Ignored
                }

            }
            var arrayStyle = lineStyle.ToArray();
            Array.Sort(arrayStyle);
            IList<string> uniqList = new HashSet<string>(arrayStyle).ToList();

            try
            {
                cb_Line_Style.DataSource = uniqList;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Line_Style.SelectedItem = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {

            Autodesk.Revit.DB.View activeView = Doc.ActiveView;
            IList<ElementId> finalList = new List<ElementId>();
            string selectedLayer = cb_CAD_Layer.SelectedItem.ToString();
            string selectedLineType = cb_Line_Type.SelectedItem.ToString();
            string selectedLineStyle = cb_Line_Style.SelectedItem.ToString();
            var planeRef = activeView.GenLevel.GetPlaneReference();

            Transaction t = new Transaction(Doc, "CADtoRvtLine");
            t.Start();
            var planE = Plane.CreateByNormalAndOrigin(Doc.ActiveView.ViewDirection, new XYZ(5,0,0));
            var sketcH = SketchPlane.Create(Doc, planE);
            Doc.ActiveView.SketchPlane = sketcH;
            foreach (Line selectedline in lineList)
            {
                ElementId graphicId = selectedline.GraphicsStyleId;
                GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                Category categorY = graphicInstance.GraphicsStyleCategory;
                string categoryName = categorY.Name;
                if (selectedLayer == categoryName)
                {
                    var bothPts = selectedline.Tessellate();
                    var startPoint = bothPts[0] + CADOrigin;
                    var endPoint = bothPts[1] + CADOrigin;
                    Curve curvE = Line.CreateBound(startPoint, endPoint) as Curve;
                    if (selectedLineType == "Detail Line")
                    {
                        try
                        {
                            DetailCurve createLines = Doc.Create.NewDetailCurve(activeView, curvE);
                            foreach (ElementId lnStyle in createLines.GetLineStyleIds())
                            {
                                GraphicsStyle gStyle = Doc.GetElement(lnStyle) as GraphicsStyle;
                                if (gStyle.Name == selectedLineStyle)
                                {
                                    createLines.LineStyle = gStyle;
                                    break;
                                }
                            }
                            finalList.Add(createLines.Id);
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message.ToString());
                        }
                    }
                        
                    else if (selectedLineType == "Model Line")
                    {
                        try
                        {
                            ModelCurve createLines = Doc.Create.NewModelCurve(curvE, sketcH);
                            foreach (ElementId lnStyle in createLines.GetLineStyleIds())
                            {
                                GraphicsStyle gStyle = Doc.GetElement(lnStyle) as GraphicsStyle;
                                if (gStyle.Name == selectedLineStyle)
                                {
                                    createLines.LineStyle = gStyle;
                                    break;
                                }
                            }
                            finalList.Add(createLines.Id);
                        }
                        catch (Exception ex)     
                        {
                            TaskDialog.Show("Error", ex.Message.ToString());
                        }
                    }
                    
                }
            }
            foreach (PolyLine selectedPoly in polyList)
            {
                ElementId graphicId = selectedPoly.GraphicsStyleId;
                GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                Category categorY = graphicInstance.GraphicsStyleCategory;
                string categoryName = categorY.Name;
                if (selectedLayer == categoryName)
                {
                    var Pts = selectedPoly.GetCoordinates();
                    for (int i = 0; i < Pts.Count - 1; i++)
                    {
                        Curve curvE = Line.CreateBound(Pts[i] + CADOrigin, Pts[i + 1] + CADOrigin) as Curve;
                        if (selectedLineType == "Detail Line")
                            try
                            {
                                DetailCurve createLines = Doc.Create.NewDetailCurve(activeView, curvE);
                                foreach (ElementId lnStyle in createLines.GetLineStyleIds())
                                {
                                    GraphicsStyle gStyle = Doc.GetElement(lnStyle) as GraphicsStyle;
                                    if (gStyle.Name == selectedLineStyle)
                                    {
                                        createLines.LineStyle = gStyle;
                                        break;
                                    }
                                }
                                finalList.Add(createLines.Id);
                            }
                            catch (Exception ex)
                            {
                                TaskDialog.Show("Error", ex.Message.ToString());
                            }
                        else if (selectedLineType == "Model Line")
                        {
                            try
                            {
                                ModelCurve createLines = Doc.Create.NewModelCurve(curvE, sketcH);
                                foreach (ElementId lnStyle in createLines.GetLineStyleIds())
                                {
                                    GraphicsStyle gStyle = Doc.GetElement(lnStyle) as GraphicsStyle;
                                    if (gStyle.Name == selectedLineStyle)
                                    {
                                        createLines.LineStyle = gStyle;
                                        break;
                                    }
                                }
                                finalList.Add(createLines.Id);
                            }
                            catch (Exception ex)
                            {
                                TaskDialog.Show("Error", ex.Message.ToString());
                            }
                        }
                    }
                }
            }
            t.Commit();
            TaskDialog.Show("Output", "Sucess!\n" + finalList.Count.ToString()
                + " number of Lines Created.");
        }
        private void cb_CAD_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            IList<Element> cadFiles = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            string selectedCadFile = cb_CAD_Name.SelectedItem.ToString();
            ImportInstance selectedCadInstance = null;
            foreach (ImportInstance cadF in cadFiles)
            {
                if (selectedCadFile == cadF.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString())
                {
                    selectedCadInstance = cadF;
                    break;
                }
            }


            IList<GeometryElement> geoList = new List<GeometryElement>();

            IList<string> layerfromCAD = new List<string>();

            Options op = Doc.Application.Create.NewGeometryOptions();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;

            GeometryElement geometrY = selectedCadInstance.get_Geometry(op);
            if (geometrY != null)
            {
                foreach (GeometryObject geoObj in geometrY)
                {
                    GeometryInstance geoInstance = geoObj as GeometryInstance;
                    if (geoInstance != null)
                    {
                        CADOrigin = geoInstance.Transform.Origin;
                        GeometryElement geoEle = geoInstance.GetSymbolGeometry() as GeometryElement;
                        if (geoEle != null)
                        {
                            foreach (GeometryObject geoObj1 in geoEle)
                            {
                                if (geoObj1 is Line)
                                {
                                    lineList.Add(geoObj1 as Line);
                                    Line lineData = geoObj1 as Line;
                                    ElementId graphicId = lineData.GraphicsStyleId;
                                    GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                                    Category categorY = graphicInstance.GraphicsStyleCategory;
                                    string categoryName = categorY.Name;
                                    layerfromCAD.Add(categoryName);

                                }
                                else if (geoObj1 is PolyLine)
                                {
                                    polyList.Add(geoObj1 as PolyLine);
                                    PolyLine polyData = geoObj1 as PolyLine;
                                    ElementId graphicId = polyData.GraphicsStyleId;
                                    GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                                    Category categorY = graphicInstance.GraphicsStyleCategory;
                                    string categoryName = categorY.Name;
                                    layerfromCAD.Add(categoryName);
                                }
                            }
                        }
                    }
                }
            }


            IList<string> uniq = new HashSet<string>(layerfromCAD).ToList();
            cb_CAD_Layer.DataSource = uniq;
            cb_CAD_Layer.SelectedIndex = 0;

        }

        private void cb_Line_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLineType = cb_Line_Type.SelectedItem.ToString();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }

        private void bt_Finish_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
