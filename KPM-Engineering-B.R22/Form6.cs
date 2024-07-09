using Autodesk.Revit.Creation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Net;
using Autodesk.Revit.DB.Electrical;
using System.Xml.Linq;
using System.Windows.Controls;

namespace KPMEngineeringB.R
{
    public partial class Form6 : System.Windows.Forms.Form
    {
        
        private IList<Autodesk.Revit.DB.View> scheduleEleList = new List<Autodesk.Revit.DB.View>();
        private IList<String> scheduleNameList = new List<String>();

        Autodesk.Revit.DB.Document Doc;
        public Form6(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            Doc = doc;

        }

        private void Form6_Load(object sender, EventArgs e)
        {
            var allViews = new FilteredElementCollector(Doc).OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
            var PanelSchedule = string.Empty;
            checkedListBox1.Items.Clear();
            foreach (Autodesk.Revit.DB.View vieW in allViews)
            {
                if (vieW.IsTemplate == false)
                {
                    if (vieW.ViewType == ViewType.PanelSchedule)
                    {

                        Autodesk.Revit.DB.ElementId getPanelID = (vieW as Autodesk.Revit.DB.Electrical.PanelScheduleView).GetPanel();
                        Element getPanel = Doc.GetElement(getPanelID);
                        string GetPanelName = getPanel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
                        string GetScheduleName = vieW.get_Parameter(BuiltInParameter.PANEL_SCHEDULE_NAME).AsString();
                        if (GetPanelName != GetScheduleName)
                        {
                            var scheduleName = vieW.get_Parameter(BuiltInParameter.PANEL_SCHEDULE_NAME).AsString();
                            scheduleEleList.Add(vieW);
                            scheduleNameList.Add(scheduleName);

                        }
                    }
                }
            }

            checkedListBox1.DataSource = scheduleNameList;
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {

            var transaction = new Transaction(Doc, "Updated Panel Schedule");
            transaction.Start();
            foreach (int item in checkedListBox1.CheckedIndices)
            {
                Autodesk.Revit.DB.ElementId getPanelID = (scheduleEleList[item] as Autodesk.Revit.DB.Electrical.PanelScheduleView).GetPanel();
                Element getPanel = Doc.GetElement(getPanelID);
                string GetPanelName = getPanel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
                var SetScheduleName = (scheduleEleList[item] as Autodesk.Revit.DB.Element).get_Parameter(BuiltInParameter.PANEL_SCHEDULE_NAME).Set(GetPanelName);
            }
            transaction.Commit();
            var count = checkedListBox1.CheckedIndices.Count;
            if (count != 0)
                TaskDialog.Show("Results", "Number of Panel Schedules Updated : " + count.ToString());
            else
                TaskDialog.Show("Results", "No Panel Schedules Updated. ");
            this.DialogResult = DialogResult.OK;
            Close();
                      
        }
        

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }


        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btn_SelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < scheduleEleList.Count; i++ )
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void btn_SelectNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < scheduleEleList.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }
    }
}
