﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.IO;

public partial class Reports_EmployeeWiseBasicMonthlyAttendance : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            BindDropDown();


    }

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        txt_start_date.Text = Calendar1.SelectedDate.Date.ToString("d");
    }
    protected void Calendar2_SelectionChanged(object sender, EventArgs e)
    {
        txt_end_date.Text = Calendar2.SelectedDate.Date.ToString("d");
    }

    protected void BindDropDown()
    {
        ddlName.Items.Clear();
        ddlName.Items.Add("select");
        ManageEmployees objEmployee = new ManageEmployees();
        var x = objEmployee.GetAllEmployees();
        ddlName.DataSource = x;
        ddlName.DataTextField = "Name";
        ddlName.DataValueField = "Id";
        ddlName.DataBind();
        string empNames = "";
        foreach (var item in x)
        {
            empNames = empNames + "\"" + item.Name + "\",";
        }
        empNames = empNames.Remove(empNames.Length - 1);

        string data = @"<script>$(function () { var availableTags = [ " + empNames +
              @"];
            $(""#ContentPlaceHolder1_txtEmployeeId"").autocomplete({
                source: availableTags
            });
        });
   </script>";
        lit_autocomplete.Text = data;
    }

    protected void btn_report_Click(object sender, EventArgs e)
    {
        btnExport.Visible = true;
        ManageReports objManageReports = new ManageReports();
        TimeSpan relaxationTime = new TimeSpan();
        relaxationTime = TimeSpan.Parse(ddlRelaxation.SelectedValue.ToString());
        int EmployeeId = 0;
        Int32.TryParse(txtEmployeeId.Text, out EmployeeId);
        MonthlyReportOfEmployee objMonthlyReportOfEmployee = new MonthlyReportOfEmployee();
        var data = objManageReports.GetMonthlyAttendanceDetailedReport(EmployeeId, Calendar1.SelectedDate.Date, Calendar2.SelectedDate.Date, relaxationTime, out objMonthlyReportOfEmployee);
        grid_monthly_attendanceBasic.DataSource = data;
        grid_monthly_attendanceBasic.DataBind();
        if (data.Count != 0)
        {
            lblName.Text = "Name : " + data[0].Name;
            lblDepartment.Text = "Department : " + data[0].DepartmentName;
        }
        else
        {
            lblName.Text = "No Data";
        }
        lblTotalDuration.Text = "TotalDuration : " + objMonthlyReportOfEmployee.TotalDuration.ToString();
        lblPresentDays.Text = "PresentDays : " + objMonthlyReportOfEmployee.PresentDays.ToString();
        lblLeaves.Text = "Leaves : " + objMonthlyReportOfEmployee.Leaves.ToString();
        lblHolidays.Text = "Holidays : " + objMonthlyReportOfEmployee.Holidays.ToString();
        lblAbsentDays.Text = objMonthlyReportOfEmployee.AbsentDays.ToString();
        lblAbsentDays.Text = "AbsentDays : " + objMonthlyReportOfEmployee.AbsentDays.ToString();
        lblWeeklyOff.Text = "WeeklyOff : " + objMonthlyReportOfEmployee.WeeklyOff.ToString();
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {
        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                //To Export all pages
                grid_monthly_attendanceBasic.AllowPaging = false;
                //this.BindGrid();

                grid_monthly_attendanceBasic.RenderBeginTag(hw);
                grid_monthly_attendanceBasic.HeaderRow.RenderControl(hw);
                foreach (GridViewRow row in grid_monthly_attendanceBasic.Rows)
                {
                    row.RenderControl(hw);
                }
                grid_monthly_attendanceBasic.FooterRow.RenderControl(hw);
                grid_monthly_attendanceBasic.RenderEndTag(hw);
                StringReader sr = new StringReader(sw.ToString());
                Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();

                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Report.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Write(pdfDoc);
                Response.End();
            }
        }
    }
}