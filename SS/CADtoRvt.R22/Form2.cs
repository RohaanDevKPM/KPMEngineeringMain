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
using Autodesk.Revit.DB.Mechanical;

namespace KPMEngineeringB.R
{
    public partial class Form2 : System.Windows.Forms.Form
    {
        private IList<Line> lineList = new List<Line>();

        private IList<PolyLine> polyList = new List<PolyLine>();
        private static double middleEle { get; set; }

        private static double ductWidth {  get; set; }

        private static double ductHeight { get; set; }

        private static double ductDiameter { get; set; }

        private static Element eleDuctSys { get; set;}
        private static DuctType eleDuctType {  get; set;}

        private static XYZ CADOrigin { get; set; }

        Autodesk.Revit.DB.Document Doc;
        public Form2(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // CAD Files
            IList<Element> cadFiles = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
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

            //Duct Type
            IList<Element> ductType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsElementType()
                .ToElements();
            try
            {
                IList<string> ductTypeName = new List<string>();
                foreach(DuctType pt in ductType)
                {
                    ductTypeName.Add((pt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                        + " - " + (pt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()));
                }
                cb_Duct_Type.DataSource = ductTypeName;
                if (ductTypeName.Contains("Round Duct"))
                {
                    label6.Visible = false;
                    label7.Visible = false;
                    tb_Duct_Height.Visible = false;
                    tb_Duct_Width.Visible = false;
                    tb_Duct_Diameter.Visible = true;
                    label8.Visible = true;
                }
                else
                {
                    label6.Visible = true;
                    label7.Visible = true;
                    tb_Duct_Height.Visible = true;
                    tb_Duct_Width.Visible = true;
                    tb_Duct_Diameter.Visible = false;
                    label8.Visible = false;
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Duct_Type.SelectedIndex = 0;

            //Duct System Type
            IList<Element> ductSysType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_DuctSystem).WhereElementIsElementType()
                .ToElements();
            try
            {
                IList<string> ductSysTypeName = new List<string>();
                foreach(Element pst in ductSysType)
                {
                    ductSysTypeName.Add(pst.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString());
                }
                cb_Duct_System.DataSource = ductSysTypeName;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Duct_System.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            double defalutEle;
            double.TryParse(tb_MiddleEle.Text, out defalutEle);
            middleEle = defalutEle/304.8;
            double defalutDuctWidth;
            double.TryParse(tb_Duct_Width.Text, out defalutDuctWidth);
            double tempDuctW = defalutDuctWidth;
            if (tempDuctW == 0)
            {
                ductWidth = 300/304.8;
            }
            else
            {
                ductWidth = defalutDuctWidth / 304.8;
            }
            double defalutDuctHeight;
            double.TryParse(tb_Duct_Height.Text, out defalutDuctHeight);
            double tempDuctH = defalutDuctHeight;
            if (tempDuctH == 0)
            {
                ductHeight = 300 / 304.8;
            }
            else
            {
                ductHeight = defalutDuctHeight / 304.8;
            }
            double defalutDuctDiameter;
            double.TryParse(tb_Duct_Diameter.Text, out defalutDuctDiameter);
            double tempDuctD = defalutDuctDiameter;
            if (tempDuctD == 0)
            {
                ductDiameter = 200 / 304.8;
            }
            else
            {
                ductDiameter = defalutDuctDiameter / 304.8;
            }
            Level activeLevel = Doc.ActiveView.GenLevel;
            IList<ElementId> finalList = new List<ElementId>();
            IList<ElementId> secondList = new List<ElementId>();
            string selectedLayer = cb_CAD_Layer.SelectedItem.ToString();
            string selectedDuctType = cb_Duct_Type.SelectedItem.ToString();
            string selectedDuctSys = cb_Duct_System.SelectedItem.ToString();

            IList<Element> ductType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsElementType()
                .ToElements();
            foreach (DuctType pt in ductType)
            {
                if ((pt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (pt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedDuctType)
                {
                    eleDuctType = pt;
                    break;
                }
            }

            IList<Element> ductSysType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_DuctSystem).WhereElementIsElementType()
                .ToElements();
            foreach (Element pst in ductSysType)
            {
                if ((pst.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedDuctSys)
                {
                    eleDuctSys = pst;
                    break;
                }
            }

            Transaction t = new Transaction(Doc, "CADtoRvtDuct");
            t.Start();
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

                    try
                    {
                        var createPlaceholder = Autodesk.Revit.DB.Mechanical.Duct.CreatePlaceholder(Doc, eleDuctSys.Id,
                            eleDuctType.Id, activeLevel.Id, startPoint, endPoint);
                        if (selectedDuctType.Contains("Round Duct"))
                        {
                            var dSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).Set(ductDiameter);
                        }
                        else
                        {
                            var wSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).Set(ductWidth);
                            var hSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).Set(ductHeight);

                        }
                        var mEle = createPlaceholder.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                        finalList.Add(createPlaceholder.Id);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", ex.Message.ToString());
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
                        try
                        {
                            var createPlaceholder = Autodesk.Revit.DB.Mechanical.Duct.CreatePlaceholder(Doc, eleDuctSys.Id,
                                eleDuctType.Id, activeLevel.Id, Pts[i] + CADOrigin, Pts[i + 1] + CADOrigin);
                            if (selectedDuctType.Contains("Round Duct"))
                            {
                                var dSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).Set(ductDiameter);
                            }
                            else
                            {
                                var wSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).Set(ductWidth);
                                var hSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).Set(ductHeight);

                            }
                            var mEle = createPlaceholder.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                            finalList.Add(createPlaceholder.Id);
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message.ToString());
                        }
                    }
                }
            }
            
            try
            {
                var ConvertPH = Autodesk.Revit.DB.Mechanical.MechanicalUtils.ConvertDuctPlaceholders(Doc, finalList);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message.ToString());
            }
            Doc.Regenerate();
            t.Commit();
            
            Transaction t1 = new Transaction(Doc, "CADtoRvtDuct");
            t.Start();
            foreach(ElementId ph in finalList)
            {
                if (Doc.GetElement(ph) != null)
                {
                    secondList.Add(ph);
                }
            }
            if (secondList.Count > 0)
                try
                {
                    var ConvertPH = Autodesk.Revit.DB.Mechanical.MechanicalUtils.ConvertDuctPlaceholders(Doc, secondList);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message.ToString());
                }
            t.Commit() ;
            
            TaskDialog.Show("Output", "Sucess!\n" + finalList.Count.ToString()
                + " number of Ducts Created.");
            
            
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

        private void cb_Pipe_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDuctType = cb_Duct_Type.SelectedItem.ToString();
            if (selectedDuctType.Contains("Round Duct"))
            {
                label6.Visible = false;
                label7.Visible = false;
                tb_Duct_Height.Visible = false;
                tb_Duct_Width.Visible = false;
                tb_Duct_Diameter.Visible = true;
                label8.Visible = true;
            }
            else
            {
                label6.Visible = true;
                label7.Visible = true;
                tb_Duct_Height.Visible = true;
                tb_Duct_Width.Visible = true;
                tb_Duct_Diameter.Visible = false;
                label8.Visible = false;
            }

            ///TaskDialog.Show("Result", DuctTypeInstance.Id.ToString());

        }

        private void tb_MiddleEle_TextChanged(object sender, EventArgs e)
        {

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
