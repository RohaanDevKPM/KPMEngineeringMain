using Autodesk.Revit.UI;
using CADtoRvtPipe.R._5._FifthButton;
using CADtoRvtPipe.R._6._SixthButton;
using CADtoRvtPipe.R.FourthButton;
using CADtoRvtPipe.R.ThirdButton;
using KPMEngineeringB.R.FirstButton;
using KPMEngineeringB.R.SecondButton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KPMEngineeringB.R
{
    public class ApplnCommand : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab("KPM-Engineering");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message.ToString());
            }

            var ribbonPanel1 = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "CAD to Revit") ??
                              application.CreateRibbonPanel("KPM-Engineering", "CAD to Revit");

            FirstButtonCommand.CreateBtn1(ribbonPanel1);
            SecondButtonCommand.CreateBtn2(ribbonPanel1);
            ThirdButtonCommand.CreateBtn3(ribbonPanel1);
            FourthButtonCommand.CreateBtn4(ribbonPanel1);
            FifthButtonCommand.CreateBtn5(ribbonPanel1);

            var ribbonPanel2 = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "Electrical") ??
                              application.CreateRibbonPanel("KPM-Engineering", "Electrical");

            SixthButtonCommand.CreateBtn6(ribbonPanel2);

            return Result.Succeeded;
        }
    }
}
