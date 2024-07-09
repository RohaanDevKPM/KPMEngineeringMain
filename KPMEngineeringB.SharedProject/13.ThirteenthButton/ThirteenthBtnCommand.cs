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
using System.Windows.Forms;

namespace KPMEngineeringB.R._13._ThirteenthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ThirteenthBtnCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            SupportDatA.btnName = "Circuit Load Name";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                using (System.Windows.Forms.Form formS = new UpdateCircuitLoadName(doc, commandData))
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
                return Result.Cancelled;
            }
        }
        public static void CreateBtn13(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Circuit" + Environment.NewLine + "Load Name",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                )
            {
                ToolTip = "Update Electrical Circuit Load Name.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_electrical_circuit_32.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                LongDescription = "Update Electrical Circuit Load Name as per Electrical Devices or Fixture Locations in Rooms.",
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_electrical_circuit_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/EWf-UANhQehEkhSjxZPe400B6GlAHRkAueDjT--jrItpWw?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=CHVp1W");
            pushBtn.SetContextualHelp(contexHelp);
        }
    }
}
