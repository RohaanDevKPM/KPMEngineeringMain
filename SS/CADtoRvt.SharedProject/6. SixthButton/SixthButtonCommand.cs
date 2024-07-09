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

namespace CADtoRvtPipe.R._6._SixthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class SixthButtonCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var ActiveV = doc.ActiveView;
            var allViews = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
            var PanelSchedule = string.Empty;
            IList<Autodesk.Revit.DB.View> PSlist = new List<Autodesk.Revit.DB.View>();
            var currentTime = DateTime.Now;
            DateTime.TryParse(setDate, out DateTime setTime);
            int compareResult = DateTime.Compare(setTime, currentTime);
            if (compareResult != -1)
            {
                var transaction = new Transaction(doc, "Updated Panel Schedule");
                transaction.Start();
                foreach (Autodesk.Revit.DB.View vieW in allViews)
                {
                    if (vieW.IsTemplate == false)
                    {
                        if (vieW.ViewType == ViewType.PanelSchedule)
                        {
                            Autodesk.Revit.DB.ElementId getPanelID = (vieW as Autodesk.Revit.DB.Electrical.PanelScheduleView).GetPanel();
                            Element getPanel = doc.GetElement(getPanelID);
                            string GetPanelName = getPanel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
                            string GetScheduleName = vieW.get_Parameter(BuiltInParameter.PANEL_SCHEDULE_NAME).AsString();
                            if (GetPanelName != GetScheduleName)
                            {
                                var setName = vieW.get_Parameter(BuiltInParameter.PANEL_SCHEDULE_NAME).Set(GetPanelName);
                                PanelSchedule += Convert.ToString(vieW.Name) + "\n";
                                PSlist.Add(vieW);
                            }
                        }
                    }
                }
                transaction.Commit();
                var count = PSlist.Count;
                if (count != 0)
                    TaskDialog.Show("Results", "Number of Panel Schedules Updated : " + count.ToString() + "\n" + "Panel Schedule Updated Names: " + "\n" + PanelSchedule);
                else
                    TaskDialog.Show("Results", "No Panel Schedules Updated. ");

                return Result.Succeeded;
            }
            else
            {
                TaskDialog.Show("Trial Period", "Your trial period is expired, please contact with KPM-Engineering Team.");
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
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://drive.google.com/file/d/1GdDRn1AdVstruocKZwriuZKAurO-Mgxx/view?usp=sharing");
            pushBtn.SetContextualHelp(contexHelp);
        }
        public string setDate = "2023-12-31";
    }
}
