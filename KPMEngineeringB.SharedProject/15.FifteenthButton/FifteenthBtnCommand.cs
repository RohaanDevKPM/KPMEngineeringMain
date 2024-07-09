using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB.Plumbing;
using System.Linq;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;

namespace KPMEngineeringB.R._15._FifteenthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FifteenthBtnCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            SupportDatA.btnName = "Reconnect Elements";
            // Reconnect Pipe,Conduit,Cable Tray,Duct Plugin
            if (SupportDatA.CheckAuthorize(commandData))
            {
                List<Connector> connectors = new List<Connector>();
                List<XYZ> origins = new List<XYZ>();

                connectors.Clear();
                origins.Clear();

                int connectionCount = 0;
                var selectedElements = uiDoc.Selection.GetElementIds();
                using (Transaction transaction = new Transaction(doc, "Reconnect Element"))
                {
                    transaction.Start();
                    foreach (var element in selectedElements)
                    {
                        Element elementId = doc.GetElement(element);

                        if (elementId.Category.Name == "Pipe Fittings" || elementId.Category.Name == "Conduit Fittings" || elementId.Category.Name == "Duct Fittings" || elementId.Category.Name == "Cable Tray Fittings")
                        {
                            var connectorFitManager = (elementId as FamilyInstance).MEPModel.ConnectorManager;
                            if (connectorFitManager != null)
                            {
                                var unusedConnectors = connectorFitManager.UnusedConnectors;
                                foreach (var connector in unusedConnectors)
                                {
                                    var unused = connector as Connector;
                                    if (unused != null)
                                    {
                                        connectors.Add(unused);
                                        var origin = unused.Origin;
                                        origins.Add(origin);
                                    }
                                }
                            }
                        }

                        ConnectorManager connectorManager = null;

                        if (elementId.Category.Name == "Pipes")
                        {
                            var pipe = elementId as Pipe;
                            if (pipe != null)
                            {
                                connectorManager = pipe.ConnectorManager;
                            }
                        }
                        else if (elementId.Category.Name == "Conduits")
                        {
                            var conduit = elementId as Conduit;
                            if (conduit != null)
                            {
                                connectorManager = conduit.ConnectorManager;
                            }
                        }
                        else if (elementId.Category.Name == "Ducts")
                        {
                            var duct = elementId as Duct;
                            if (duct != null)
                            {
                                connectorManager = duct.ConnectorManager;
                            }
                        }
                        else if (elementId.Category.Name == "Cable Trays")
                        {
                            var cableTray = elementId as CableTray;
                            if (cableTray != null)
                            {
                                connectorManager = cableTray.ConnectorManager;
                            }
                        }
                        if (connectorManager != null)
                        {
                            var unusedConnectors = connectorManager.UnusedConnectors;
                            foreach (var connector in unusedConnectors)
                            {
                                var unused = connector as Connector;
                                if (unused != null)
                                {
                                    connectors.Add(unused);
                                    var origin = unused.Origin;
                                    origins.Add(origin);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < connectors.Count; i++)
                    {
                        var connector = connectors[i];
                        var origin = origins[i];

                        foreach (var con in connectors)
                        {
                            if (con.Origin.DistanceTo(origin) < (1 / 304.8))
                            {
                                if (con.Owner.Id != connector.Owner.Id)
                                {
                                    if (!con.IsConnected)
                                    {
                                        try
                                        {
                                            connector.ConnectTo(con);
                                            connectionCount++;
                                        }
                                        catch
                                        {
                                            connectionCount--;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                TaskDialog.Show("Output", connectionCount.ToString() + " Elements Connections Connected");
                SupportDatA.checkData(commandData);
                return Result.Succeeded;
            }
            else
            {
                return Result.Cancelled;
            }
        }
        public static void CreateBtn15(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Reconnect " + Environment.NewLine + "Elements",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                )
            {
                ToolTip = "Reconnect End of Elements which are aligned but not connected.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_connect_32.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_connect_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/ERgT7awBP2JAjVItOvztRjsBy-5W5ZKyEp1CyDMvJD896A?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=0gN5QF");
            pushBtn.SetContextualHelp(contexHelp);
        }
    }
}
