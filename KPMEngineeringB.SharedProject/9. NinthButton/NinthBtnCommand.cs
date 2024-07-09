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

namespace KPMEngineeringB.R._9._NinthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class NinthBtnCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            SupportDatA.btnName = "Export IFC Data";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                using (System.Windows.Forms.Form formS = new ExportToFamilyForIFC(doc, commandData))
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
        public static void CreateBtn9(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Export IFC" + Environment.NewLine + "Data",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                )
            {
                ToolTip = "Export Family Data for IFC Parameter.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_ifc_export_32.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_ifc_export_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/EZntmcgMjfRDiL4R_hLmoR8Bxr_BQSBlMsj0mGSRmeS1FQ?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=K2bJA2");
            pushBtn.SetContextualHelp(contexHelp);
        }
    }
}
