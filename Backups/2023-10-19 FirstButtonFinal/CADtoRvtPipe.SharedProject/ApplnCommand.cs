using Autodesk.Revit.UI;
using CADtoRvtPipe.R.FirstButton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CADtoRvtPipe.R
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

            var ribbonPanel = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "CAD to Revit") ??
                              application.CreateRibbonPanel("KPM-Engineering", "CAD to Revit");

            FirstButtonCommand.CreateBtn(ribbonPanel);

            return Result.Succeeded;
        }
    }
}
