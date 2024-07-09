using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using KPMEngineeringB.R;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;

namespace KPMEngineeringB.R._6._SixthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class SixthButtonCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            
            SupportDatA.btnName = "Panel Schedules";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                using (System.Windows.Forms.Form formS = new Form6(doc))
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

        public static void CreateBtn6(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name, "Panel" + Environment.NewLine + "Schedules",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Update all Panel Schedule names as per Panel Name",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Panel_Schedule_32x32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Panel_Schedule_16x16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),

            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/EXsmRretzKBGuMKdFrV5EH0BCSdkozExIxNyU6BzTmGhZw?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=JlySb3");
            pushBtn.SetContextualHelp(contexHelp);
        }        
    }
}
