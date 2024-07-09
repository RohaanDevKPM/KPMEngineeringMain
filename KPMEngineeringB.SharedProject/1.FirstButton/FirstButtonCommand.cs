using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using MySql.Data.MySqlClient;

namespace KPMEngineeringB.R.FirstButton
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class FirstButtonCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            IList<Element> cadFiles = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            SupportDatA.btnName = "CAD to Pipe";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                if (doc.ActiveView.ViewType == ViewType.FloorPlan)
                {
                    if (cadFiles.Count > 0)
                    {
                        using (System.Windows.Forms.Form formS = new Form1(doc))
                        {
                            if (formS.ShowDialog() == DialogResult.OK)
                            {
                                SupportDatA.checkData(commandData);
                                return Result.Succeeded;
                            }
                            else
                            {
                                return Result.Cancelled;
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Error", "No CAD file found in Active View.");
                        return Result.Cancelled;
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "Active View is not a Floor Plan.\nPlease try again on 2D floor plan.");
                    return Result.Cancelled;
                }
            }
            else
            {
                return Result.Cancelled;
            }
        }
        public static void CreateBtn1(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name, "CAD to" + Environment.NewLine + "Pipe",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Convert Link or Import CAD Lines & Polylines into Revit Pipes & Fittings.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icon_plumbing_pipe_32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icon_plumbing_pipe_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                LongDescription = "This will convert all CAD lines & polylines into Pipes & Pipe Fittings.\n" +
                "Ensure Pipe Size is available in Routing Preference.",
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/EZkvqz7Vcd5HuJd54UyOOPUBhcvy8rORIy5L1_hIXDgDJQ?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=tOyTnV");
            pushBtn.SetContextualHelp(contexHelp);
        }
    }
}
