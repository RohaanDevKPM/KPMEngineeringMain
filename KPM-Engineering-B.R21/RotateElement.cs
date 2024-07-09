using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Office.Interop.Excel;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Relational;
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
using Line = Autodesk.Revit.DB.Line;
using Point = Autodesk.Revit.DB.Point;

namespace KPMEngineeringB.R
{
    public partial class RotateElement : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        UIDocument UiDoc;
        public RotateElement(Document doc, ExternalCommandData commandData)
        {
            InitializeComponent();
            Doc = doc;
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            UiDoc = uiDoc;
        }

        private void RotateElement_Load(object sender, EventArgs e)
        {
            label1.Text = "Angle(°)";
            textBox1.Text = "45";
        }
        
        // Right Arrow
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var selectedElements = UiDoc.Selection.GetElementIds();

            double angleToRotate = new double();
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                angleToRotate = ConvertToRadians(45);
            }
            else
            {
                double don;
                if (double.TryParse(textBox1.Text, out don))
                {
                    angleToRotate = ConvertToRadians(don);
                }
            }

            Line axiS = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));

            // Start a new transaction
            using (Transaction transaction = new Transaction(Doc, "Rotate Elements"))
            {
                transaction.Start();
                foreach (var elementId in selectedElements)
                {
                    Element element = Doc.GetElement(elementId);

                    if (element.Category.Name == "Pipe Fittings" || element.Category.Name == "Conduit Fittings" || element.Category.Name == "Duct Fittings")
                    {
                        var connectorsSet = (element as FamilyInstance).MEPModel.ConnectorManager.Connectors;

                        foreach (var con in connectorsSet)
                        {
                            var owner = (con as Connector).AllRefs;
                            foreach (var connector in owner)
                            {
                                var ownerId = (connector as Connector).Owner.Id;
                                if (!selectedElements.Contains(ownerId))
                                {
                                    var eleAxis = Doc.GetElement(ownerId);
                                    var loc1 = eleAxis.Location as LocationCurve;
                                    var startPt = loc1.Curve.GetEndPoint(0);
                                    var EndPt = loc1.Curve.GetEndPoint(1);
                                    axiS = Line.CreateBound(startPt, EndPt);
                                }
                            }
                        }
                    }
                }

                var oldPt = axiS.GetEndPoint(1);
                var newPt = new XYZ(1, 0, 0);
                if (oldPt.X != newPt.X && oldPt.Y != newPt.Y && oldPt.Z != newPt.Z)
                {
                    ElementTransformUtils.RotateElements(Doc, selectedElements, axiS, angleToRotate);
                }
                else
                {
                    TaskDialog.Show("Error", "Selection is invalid.");
                }
                transaction.Commit();
            }
        }

        private double ConvertToRadians(double angle)
        { 
            return ((Math.PI * angle)/180);
        }
        // Left Rotating Arrow
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            var selectedElements = UiDoc.Selection.GetElementIds();
          
            double angleToRotate = new double();
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                angleToRotate = ConvertToRadians(-45);
            }
            else
            {
                double don;
                if (double.TryParse(textBox1.Text, out don))
                {
                    angleToRotate = ConvertToRadians(-don);
                }
            }

            Line axiS = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));

            // Start a new transaction
            using (Transaction transaction = new Transaction(Doc, "Rotate Elements"))
            {
                transaction.Start();
                foreach (var elementId in selectedElements)
                {
                    Element element = Doc.GetElement(elementId);
                    if (element.Category.Name == "Pipe Fittings" || element.Category.Name == "Conduit Fittings" || element.Category.Name == "Duct Fittings")
                    {
                        var connectorsSet = (element as FamilyInstance).MEPModel.ConnectorManager.Connectors;

                        foreach (var con in connectorsSet)
                        {
                            var owner = (con as Connector).AllRefs;
                            foreach (var connector in owner)
                            {
                                var ownerId = (connector as Connector).Owner.Id;
                                if (!selectedElements.Contains(ownerId))
                                {
                                    var eleAxis = Doc.GetElement(ownerId);
                                    var loc1 = eleAxis.Location as LocationCurve;
                                    var startPt = loc1.Curve.GetEndPoint(0);
                                    var EndPt = loc1.Curve.GetEndPoint(1);
                                    axiS = Line.CreateBound(startPt, EndPt);
                                }
                            }
                        }
                    }
                }

                var oldPt = axiS.GetEndPoint(1);
                var newPt = new XYZ(1, 0, 0);
                if (oldPt.X != newPt.X && oldPt.Y != newPt.Y && oldPt.Z != newPt.Z)
                {
                    ElementTransformUtils.RotateElements(Doc, selectedElements, axiS, angleToRotate);
                }
                else
                {
                    TaskDialog.Show("Error", "Selection is invalid.");
                }
                transaction.Commit();
            }
        }
        // Done 
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }
    }
}
