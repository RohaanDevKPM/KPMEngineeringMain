using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using KPMEngineeringB.R._7._SeventhButton;
using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace KPMEngineeringB.R
{
    public partial class Form7 : System.Windows.Forms.Form
    {
        public static IList<Element> collectedElements = new List<Element>();
        public static IList<Element> collectWallTypes = new List<Element>();

        public static int selectedFamIndex;
        
        Autodesk.Revit.DB.Document Doc;
        UIDocument UiDoc;
        public Form7(Autodesk.Revit.DB.Document doc, ExternalCommandData commandData, IList<Element> elements)
        {
            InitializeComponent();
            Doc = doc;
            var uiApp = commandData.Application;
            UiDoc = uiApp.ActiveUIDocument;
            collectedElements = elements;
            
        }

        public void Form7_Load(object sender, EventArgs e)
        {
            var wallTypeName = new List<string>();
            collectWallTypes = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
            foreach (var collectWallType in collectWallTypes)
            {
                var combineName = collectWallType.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString() +
                    " - " + collectWallType.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
                wallTypeName.Add(combineName);
            }

            cb_Wall_Type.DataSource = wallTypeName;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }

        private void bt_Done_Click(object sender, EventArgs e)
        {
            IList<Wall> convertedWalls = new List<Wall>();
            selectedFamIndex = cb_Wall_Type.SelectedIndex;
            var selectedFamily = collectWallTypes[selectedFamIndex] as WallType;
            var activeView = Doc.ActiveView;
            var lvl = activeView.GenLevel;
            using (Autodesk.Revit.DB.Transaction transactioN = new Autodesk.Revit.DB.Transaction(Doc, "Convert Column to Wall"))
            {
                transactioN.Start();

                var collectColumnTypes = new FilteredElementCollector(Doc)
                                                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                                                .WhereElementIsElementType().ToElements();

                foreach (var elem in collectColumnTypes)
                {
                    var bParam = elem.LookupParameter("b");
                    var hParam = elem.LookupParameter("h");

                    if (bParam != null)
                    {
                        var bValue = bParam.AsDouble();
                        var valueToSet = bValue;

                        if (hParam != null)
                        {
                            var hValue = hParam.AsDouble();
                            valueToSet = hValue;
                        }

                        var kpmWidthParam = elem.LookupParameter("KPM_Width");
                        var kpmDepthParam = elem.LookupParameter("KPM_Length");

                        if (kpmWidthParam != null)
                        {
                            kpmWidthParam.Set(bValue);
                        }
                        if (kpmDepthParam != null)
                        {
                            kpmDepthParam.Set(valueToSet);
                        }
                    }
                }

                foreach (var element in collectedElements)
                {
                    if (((Doc.GetElement(element.GetTypeId())).LookupParameter("b") != null) && ((Doc.GetElement(element.GetTypeId())).LookupParameter("h") != null))
                    {
                        var LengtH = ((Doc.GetElement(element.GetTypeId())).LookupParameter("b").AsDouble());
                        var WidtH = ((Doc.GetElement(element.GetTypeId())).LookupParameter("h").AsDouble());
                        var BaseLvl = Doc.GetElement(element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId());
                        var BaseOffseT = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();
                        double LongSide;
                        double ShortSide;
                        if (LengtH > WidtH)
                        {
                            LongSide = LengtH;
                            ShortSide = WidtH;
                        }
                        else if (LengtH == WidtH)
                        {
                            LongSide = LengtH;
                            ShortSide = WidtH;
                        }
                        else
                        {
                            LongSide = WidtH;
                            ShortSide = LengtH;
                        }
                        var DriveLine = (element as FamilyInstance).GetSweptProfile().GetDrivingCurve();
                        var locPoint = (element as FamilyInstance).Location as LocationPoint;
                        var PoinT = locPoint.Point;
                        var RotatioN = locPoint.Rotation;
                        var midHigh = PoinT.Add(XYZ.BasisZ);
                        var CurveS = (element as FamilyInstance).GetSweptProfile().GetSweptProfile().Curves;
                        XYZ StartpT = new XYZ(0, 0, 0);
                        XYZ EndpT = new XYZ(0, 0, 0);
                        if ( LengtH == WidtH )
                        {
                            
                            for( var i = 0; i < CurveS.Size; i++)
                            {
                                var linE = (CurveS.get_Item(i)) as Line;
                                if (i == 0)
                                {
                                    StartpT = linE.Evaluate(0.5, true);
                                }
                                else if (i == 2)
                                {
                                    EndpT = linE.Evaluate(0.5, true);
                                }
                            }
                        }
                        else
                        {
                            foreach (var liNE in CurveS)
                            {
                                var linE = liNE as Line;
                                if (Math.Round((linE.Length) * 304.8, 0) == Math.Round(ShortSide * 304.8, 0))
                                {
                                    if (StartpT.X == 0 && StartpT.Y == 0 && StartpT.Z == 0)
                                    {
                                        StartpT = linE.Evaluate(0.5, true);
                                    }
                                    EndpT = linE.Evaluate(0.5, true);
                                }

                            }
                        }
                        var NewCurve = Autodesk.Revit.DB.Line.CreateBound(StartpT + PoinT, EndpT + PoinT);
                        var axisLine = Autodesk.Revit.DB.Line.CreateBound(PoinT, midHigh);
                        var newLine = Doc.Create.NewDetailCurve(activeView, NewCurve);
                        var rotateLine = (newLine.Location).Rotate(axisLine, RotatioN);
                        var CurV = (newLine.Location as LocationCurve).Curve;
                        var wallWidth = Convert.ToInt32(Math.Round(ShortSide * 304.8, 0));
                        var CollectWallType = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
                        Element SelectedWallType = null;
                        var wallName = "KPM_ST_SHR_" + Convert.ToString(wallWidth);
                        foreach (var WallT in CollectWallType)
                        {
                            if (wallName == WallT.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString())
                            {
                                SelectedWallType = WallT;
                                break;
                            }
                        }

                        if (SelectedWallType == null)
                        {
                            SelectedWallType = selectedFamily.Duplicate(wallName);
                            var wallCompoundStr = (selectedFamily as HostObjAttributes).GetCompoundStructure();
                            wallCompoundStr.SetLayerWidth(0, (Convert.ToDouble(wallWidth)) / 304.8);
                            (SelectedWallType as HostObjAttributes).SetCompoundStructure(wallCompoundStr);
                        }
                        try
                        {

                            var WaLL = Autodesk.Revit.DB.Wall.Create(Doc, CurV, SelectedWallType.Id, BaseLvl.Id, DriveLine.Length, BaseOffseT, false, false);
                            if (WaLL.Id != null)
                            {
                                Doc.Delete(element.Id);
                            }

                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                        }
                        finally
                        {
                            Doc.Delete(newLine.Id);
                        }
                    }
                }
                transactioN.Commit();
            }
            this.DialogResult = DialogResult.OK;

            TaskDialog.Show("Successfully Converted into Walls","Total number of Columns converted into Walls: " + collectedElements.Count.ToString());

        }
    }
}
