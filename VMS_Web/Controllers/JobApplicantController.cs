using ClosedXML.Excel;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VMS.Web.Class;
using VMS.Web.Models;

namespace VMS.Web.Controllers
{
    public class JobApplicantController : Controller
    {
        JobApplicants _JApp = new JobApplicants();
        public string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        public ActionResult Task()
        {
            return View();
        }
        public ActionResult Report()
        {
            return View();
        }
        public JsonResult GetBatchListTask(string BatchId, string DateFrom, string DateTo)
        {
            try 
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var _Task = _JApp.GetBatchListTask(BatchId, DateFrom, DateTo);
                recordsTotal = _Task.Count();
                var data = _Task.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null; 
            }
        }
        public JsonResult GetBatchDetailsTask(string BatchComp, string NameEmp)
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var _Task = _JApp.GetBatchDetailsTask(BatchComp, NameEmp);
                recordsTotal = _Task.Count();
                var data = _Task.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        public JsonResult GetBatchDetailsCount(string BatchComp)
        {
            try
            {
                return Json(_JApp.GetBatchDetailsCount(BatchComp), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        [HttpPost]
        public ActionResult Task(VMS.Web.Models.ImportExcel importExcel)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/Uploads/JobApplicant/" + importExcel.file.FileName);
                importExcel.file.SaveAs(path);
                string excelConnectionString = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                excelConnection.Open();
                string tableName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
                excelConnection.Close();
                OleDbCommand cmd = new OleDbCommand("Select * from [" + tableName + "]", excelConnection);
                excelConnection.Open();
                OleDbDataReader dReader;
                dReader = cmd.ExecuteReader();
                SqlBulkCopy sqlBulk = new SqlBulkCopy(ConfigurationManager.ConnectionStrings["VMSConnection"].ConnectionString);
                sqlBulk.DestinationTableName = "JobApplicant";
                sqlBulk.ColumnMappings.Add("Batch Comp", "BatchComp");
                sqlBulk.ColumnMappings.Add("Invitation Date", "InvitationDate");
                sqlBulk.ColumnMappings.Add("Request Date", "RequestDate");
                sqlBulk.ColumnMappings.Add("Total Candidate", "TotalCandidate");
                sqlBulk.ColumnMappings.Add("Status Batch", "StatusBatch");
                sqlBulk.ColumnMappings.Add("Batch Emp", "BatchEmp");
                sqlBulk.ColumnMappings.Add("Name Emp", "NameEmp");
                sqlBulk.ColumnMappings.Add("Phone Number", "PhoneNumber");
                sqlBulk.ColumnMappings.Add("Date Of Birth Emp", "DateOfBirthEmp");
                //sqlBulk.ColumnMappings.Add("Invitation Code Emp", "InvitationCodeEmp");
                sqlBulk.ColumnMappings.Add("Status Emp", "StatusEmp");

                sqlBulk.WriteToServer(dReader);
                excelConnection.Close();
                ViewBag.Result = "Successfully Imported";
            }
            return View();
        }
        [HttpPost]
        public ActionResult Reset()
        {
            Session["ExcelData"] = null;
            return RedirectToAction("Index");
        }
        public ActionResult BatchDetails(string BatchId)
        {
            ViewBag.Token = BatchId;
            ViewBag.Token2 = BatchId;
            return View(); 
        }
        public JsonResult SaveState(string BatchEmp)
        {
            string State;
            if (BatchEmp.Contains("YES"))
            {
                State = "ARRIVED";
                string _BatchEmp = BatchEmp.Replace("YES", "");
                return Json(_JApp.PostChangeUserStatus(State, _BatchEmp), JsonRequestBehavior.AllowGet);
            }
            else if (BatchEmp.Contains("NO"))
            {
                State = "PENDING";
                string _BatchEmp = BatchEmp.Replace("NO", "");
                return Json(_JApp.PostChangeUserStatus(State, _BatchEmp), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }
        public FileResult ExportJobApplicant(string BatchComp)
        {
            DataTable table = new DataTable("JobApplicantReport");
            table = _JApp.ExportJobApplicant(BatchComp);
            table.TableName = "JobApplicantReport";
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(table);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       DateTime.Now.ToString("yyMMdd") + "_JALog" + BatchComp + ".xlsx");
                }
            }
        }
    }
}