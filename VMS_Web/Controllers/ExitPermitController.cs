using ClosedXML.Excel;
using Kendo.Mvc.Extensions;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VMS.Web.Attribute;
using VMS.Web.Models;

namespace VMS.Web.Controllers
{

    public class ExitPermitController : Controller
    {
        long num;
        public string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        string _UseId, _token;
        //ExitPermit1 _exitpermit = new ExitPermit1();
        ExitPermit _epAction = new ExitPermit();
        Notifications _Nofif = new Notifications();
        private DBVisitorMSEntities db = new DBVisitorMSEntities();
        [ShimanoCustom]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Update(EPMaster model)
        {
            if (model.MasterId != null)
            {
                EPMaster or = db.EPMasters.SingleOrDefault(x => x.MasterId == model.MasterId);
                or.MasterId = model.MasterId;
                or.SENo = model.SENo;
                or.EPNo = model.EPNo;
                or.UseDep = model.UseDep;
                or.PlantID = model.PlantID;
                or.Destination = model.Destination;
                or.Date = model.Date;
                or.Out = model.Out;
                or.ActOut = model.ActOut;
                or.In = model.In;
                or.ActIn = model.ActIn;
                or.CompTrans = model.CompTrans;
                or.CompTransTime = model.CompTransTime;
                or.Status = model.Status;
                or.UpdateBy = Session["UseID"].ToString();
                or.UpdateDate = DateTime.Now;
                db.SaveChanges();
            }
            return View(model);
        }
        public ActionResult UpdateSecurity(EPMaster model)
        {
            if (model.MasterId != null)
            {
                EPMaster or = db.EPMasters.SingleOrDefault(x => x.MasterId == model.MasterId);
                or.MasterId = model.MasterId;
                or.SENo = model.SENo;
                or.EPNo = model.EPNo;
                or.UseDep = model.UseDep;
                or.PlantID = model.PlantID;
                or.Destination = model.Destination;
                or.Date = model.Date;
                or.Out = model.Out;
                or.ActOut = model.ActOut;
                or.In = model.In;
                or.ActIn = model.ActIn;
                or.CompTrans = model.CompTrans;
                or.CompTransTime = model.CompTransTime;
                if (model.ActIn.ToString() == "00:00" || model.ActIn.ToString() == "00:00:00")
                {
                    or.Status = model.Status;
                }
                else
                {
                    or.Status = "COMPLETED"; 
                }
                //or.Status = "COMPLETED";
                or.UpdateBy = Session["UseID"].ToString();
                or.UpdateDate = DateTime.Now;
                db.SaveChanges();
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult UpdateMobile(EPViewModel model)
        {
            HttpCookie cookie2 = HttpContext.Request.Cookies.Get("cookie_manager");
            _UseId = cookie2.Value.ToString();
            if (model.SENo > 0)
            {
                EPMaster or = db.EPMasters.SingleOrDefault(x => x.SENo == model.SENo);
                or.MasterId = model.MasterId;
                or.SENo = model.SENo;
                or.EPNo = model.EPNo;
                or.UseDep = model.UseDep;
                or.PlantID = model.PlantID;
                or.Destination = model.Destination;
                or.Date = model.Date;
                or.Out = model.Out;
                or.ActOut = model.ActOut;
                or.In = model.In;
                or.ActIn = model.ActIn;
                or.CompTrans = model.CompTrans;
                or.CompTransTime = model.CompTransTime;
                or.Status = model.Status;
                or.UpdateBy = _UseId;
                or.UpdateDate = DateTime.Now;
                db.SaveChanges();
            }
            return View(model);
        }
        public ActionResult Mobile(string UseId)
        {
            var query = "update EPMaster set ExpireTicket = 'EXPIRED' Where DATEDIFF(MINUTE,(CONVERT(DATETIME, CONVERT(CHAR(8), [date], 112)+ ' ' + CONVERT(CHAR(8), [Out], 108))), GETDATE())> 60 SELECT 'SUCCESSED' as isSuccess";
            db.Database.SqlQuery<string>(query).First();
            HttpCookie cookie = new HttpCookie("cookie_useid", UseId);
            cookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(cookie);
            return View();
        }
        public ActionResult MobileConfirm(Guid MasterId, string token, string UseId)
        {
            EPMaster or = db.EPMasters.SingleOrDefault(a => a.MasterId == MasterId);

            HttpCookie cookie = new HttpCookie("cookie_manager", UseId);
            cookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(cookie);

            ViewBag.Token = token;

            return View("MobileConfirm", or);
        }
        public ActionResult ThankYou()
        {
            return View();
        }
        public ActionResult SaveOrder(string UseDep, string PlantID, string Destination, string Out, string In, string Remarks,
                                            string ActOut, string ActIn, string Date, string CompTrans, string CompTransTime,
                                            string OTH, string Status, string Approver, EPDetail[] EPDetailss)
        {
            _UseId = Session["UseID"].ToString();
            string result = "ERROR";
            var query = "Declare @MaxNo varchar(50), @UniqNo varchar(10) select @MaxNo = isnull(MAX(convert(bigint, RIGHT(SENo, 3))), 0) + 1 from EPMaster where CONVERT(date, CreatedDate) = convert(date, getdate()) select @UniqNo = right(convert(varchar, 1000 + @MaxNo), 3) SELECT @UniqNo";
            var query_ = db.Database.SqlQuery<string>(query).First();
            var masterId = Guid.NewGuid();
            string EPNo_ = "EP" + DateTime.Now.ToString("yyMMdd") + "-" + query_.ToString();

            if (UseDep == "-1" || PlantID == "-1" || Approver == "-1" || Remarks == "" ||
                UseDep == "" || PlantID == "" || Approver == "" || Destination == "" || Out == "" || In == "" || Date == "" || CompTrans == "" || Status == "" || EPDetailss == null)
            {
                result = "Please complete all field";
            }
            else if (Remarks.Length > 50)
            {
                result = "Maximum number of remarks is 50 characters";
            }
            else
            {
                var Date1 = Convert.ToDateTime(Date);
                var TimeIn1 = TimeSpan.Parse(In);
                var TimeOut1 = TimeSpan.Parse(Out);
                var CalculatedTime = Date1 + TimeOut1;
                // var OTH1 = TimeSpan.Parse(OTH);
                var CalculatedTime1 = Date1 + TimeOut1;
                //var CalculatedTime2 = Date1 + OTH1;

                if (CalculatedTime1 < DateTime.Now)
                {
                    result = "Cannot create Exit Permit for " + CalculatedTime1.ToString("dd-MM-yyyy HH:mm") + "";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //else if (CalculatedTime2 < DateTime.Now)
                //{
                //    result = "Cannot select driver for " + CalculatedTime2.ToString("dd-MM-yyyy HH:mm") + "";
                //    return Json(result, JsonRequestBehavior.AllowGet);
                //}
                else
                {
                    EPMaster model = new EPMaster();
                    model.MasterId = masterId;
                    model.SENo = Convert.ToInt32(query_);
                    model.EPNo = EPNo_;
                    model.UseDep = UseDep;
                    model.PlantID = PlantID;
                    model.Destination = Destination;
                    model.Date = Convert.ToDateTime(Date);
                    model.Out = TimeSpan.Parse(Out);
                    model.ActOut = TimeSpan.Parse(ActOut);
                    model.In = TimeSpan.Parse(In);
                    model.ActIn = TimeSpan.Parse(ActIn);
                    if (CompTrans == "true")
                    {
                        if (CompTransTime == "")
                        {
                            result = "Please complete all field";
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        else if (CompTransTime == "OTH")
                        {
                            if (OTH == "")
                            {
                                result = "Please complete all field";
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                model.CompTrans = Convert.ToBoolean(CompTrans);
                                model.CompTransTime = TimeSpan.Parse(OTH);
                            }
                        }
                        else
                        {
                            model.CompTrans = Convert.ToBoolean(CompTrans);
                            model.CompTransTime = TimeSpan.Parse(CompTransTime);
                        }
                    }
                    else
                    {
                        model.CompTrans = Convert.ToBoolean(CompTrans);
                        model.CompTransTime = TimeSpan.Parse("00:00:00");
                    }
                    model.Status = Status;
                    model.CreatedBy = _UseId;
                    model.CreatedDate = DateTime.Now;
                    model.UpdateBy = _UseId;
                    model.UpdateDate = DateTime.Now;
                    model.ExpireTicket = "ACTIVE";

                    db.EPMasters.Add(model);
                    foreach (var item in EPDetailss)
                    {
                        var detailId = Guid.NewGuid();
                        var EPDetails = new EPDetail();
                        EPDetail O = new EPDetail();
                        O.DetailId = detailId;
                        O.MasterId = masterId;
                        O.UseID = item.UseID;
                        db.EPDetails.Add(O);
                    }
                    db.SaveChanges();
                    var PARAM5 = "UPDATE EPMaster SET Approver = '" + Approver + "' WHERE MasterId = '" + masterId + "' select 'SUCCESS'";
                    var PARAM6 = db.Database.SqlQuery<string>(PARAM5).First();
                    _Nofif.GetUserApprovalEP(masterId, EPNo_);
                    result = "SUCCESS";
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveOrderMobile(string UseDep, string PlantID, string Destination, string Out, string In, string Remarks,
                                            string ActOut, string ActIn, string Date, string CompTrans, string CompTransTime, 
                                            string OTH, string Status, string Approver, EPDetail[] EPDetailss)
        {
            HttpCookie cookie2 = HttpContext.Request.Cookies.Get("cookie_useid");
            _UseId = cookie2.Value.ToString();
            string result = "ERROR";
            var query = "Declare @MaxNo varchar(50), @UniqNo varchar(10) select @MaxNo = isnull(MAX(convert(bigint, RIGHT(SENo, 3))), 0) + 1 from EPMaster where CONVERT(date, CreatedDate) = convert(date, getdate()) select @UniqNo = right(convert(varchar, 1000 + @MaxNo), 3) SELECT @UniqNo";
            var query_ = db.Database.SqlQuery<string>(query).First();
            var masterId = Guid.NewGuid();
            string EPNo_ = "EP" + DateTime.Now.ToString("yyMMdd") + "-" + query_.ToString();

            if (UseDep == "-1" || PlantID == "-1" || Approver == "-1" || Remarks == "" ||
                UseDep == "" || PlantID == "" || Approver == "" || Destination == "" || Out == "" || In == "" || Date == "" || CompTrans == "" || CompTransTime == "" || Status == "" || EPDetailss == null)
            {
                result = "Please complete all field";
            }
            else if (Remarks.Length > 50)
            {
                result = "Maximum number of remarks is 50 characters";
            }
            else
            {
                var Date1 = Convert.ToDateTime(Date);
                var TimeIn1 = TimeSpan.Parse(In);
                var TimeOut1 = TimeSpan.Parse(Out);
                var CalculatedTime1 = Date1 + TimeOut1;
                if (CalculatedTime1 < DateTime.Now)
                {
                    result = "Cannot create Exit Permit for " + CalculatedTime1.ToString("dd-MM-yyyy HH:mm") + "";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    EPMaster model = new EPMaster();
                    model.MasterId = masterId;
                    model.SENo = Convert.ToInt32(query_);
                    model.EPNo = EPNo_;
                    model.UseDep = UseDep;
                    model.PlantID = PlantID;
                    model.Destination = Destination;
                    model.Date = Convert.ToDateTime(Date);
                    model.Out = TimeSpan.Parse(Out);
                    model.ActOut = TimeSpan.Parse(ActOut);
                    model.In = TimeSpan.Parse(In);
                    model.ActIn = TimeSpan.Parse(ActIn);
                    if (CompTrans == "true")
                    {
                        if (CompTransTime == "OTH")
                        {
                            if (OTH == "")
                            {
                                result = "Please complete all field";
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                model.CompTrans = Convert.ToBoolean(CompTrans);
                                model.CompTransTime = TimeSpan.Parse(OTH);
                            }
                        }
                        else
                        {
                            model.CompTrans = Convert.ToBoolean(CompTrans);
                            model.CompTransTime = TimeSpan.Parse(CompTransTime);
                        }
                    }
                    else
                    {
                        model.CompTrans = Convert.ToBoolean(CompTrans);
                        model.CompTransTime = TimeSpan.Parse("00:00:00");
                    }
                    model.Status = Status;
                    model.CreatedBy = _UseId;
                    model.CreatedDate = DateTime.Now;
                    model.UpdateBy = _UseId;
                    model.UpdateDate = DateTime.Now;
                    model.ExpireTicket = "ACTIVE";
                    model.Remarks = Remarks;
                    db.EPMasters.Add(model);
                    foreach (var item in EPDetailss)
                    {
                        var detailId = Guid.NewGuid();
                        var EPDetails = new EPDetail();
                        EPDetail O = new EPDetail();
                        O.DetailId = detailId;
                        O.MasterId = masterId;
                        O.UseID = item.UseID;
                        db.EPDetails.Add(O);
                    }
                    db.SaveChanges();
                    var PARAM5 = "UPDATE EPMaster SET Approver = '" + Approver + "' WHERE MasterId = '" + masterId + "' select 'SUCCESS'";
                    var PARAM6 = db.Database.SqlQuery<string>(PARAM5).First();
                    _Nofif.GetUserApprovalEP(masterId, EPNo_);
                    result = "SUCCESS";
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FindApproval(string UseDep)
        {
            return null;
        }
        public ActionResult EditMaster(string EPNo)
        {
            if (Session["BusFunc"].ToString() == "SYSTEM-MANAGER" || Session["BusFunc"].ToString() == "SYSTEM-ADMIN")
            {
                EPViewModel model = new EPViewModel();
                EPMaster or = db.EPMasters.SingleOrDefault(c => c.EPNo == EPNo);
                //model.MasterId = or.MasterId;
                //model.SENo = or.SENo;
                //model.EPNo = or.EPNo;
                //model.UseDep = or.UseDep;
                //model.PlantID = or.PlantID;
                //model.Destination = or.Destination;
                //model.Date = or.Date;
                //model.Out = or.Out;
                //model.ActOut = (TimeSpan)or.ActOut;
                //model.In = or.In;
                //model.ActIn = (TimeSpan)or.ActIn;
                //model.CompTrans = or.CompTrans;
                //model.CompTransTime = or.CompTransTime;
                //model.Status = or.Status;
                return PartialView("Partial1", or);
            }
            else
            {
                return PartialView("Error");
            }
        }
        public ActionResult EditTime(string EPNo)
        {
            if (Session["BusFunc"].ToString() == "SYSTEM-SECURITY" || Session["BusFunc"].ToString() == "SYSTEM-ADMIN" )
            {
                EPViewModel model = new EPViewModel();
                EPMaster or = db.EPMasters.SingleOrDefault(c => c.EPNo == EPNo);
                //model.MasterId = or.MasterId;
                //model.SENo = or.SENo;
                //model.EPNo = or.EPNo;
                //model.UseDep = or.UseDep;
                //model.PlantID = or.PlantID;
                //model.Destination = or.Destination;
                //model.Date = or.Date;
                //model.Out = or.Out;
                //model.ActOut = (TimeSpan)or.ActOut;
                //model.In = or.In;
                //model.ActIn = (TimeSpan)or.ActIn;
                //model.CompTrans = or.CompTrans;
                //model.CompTransTime = or.CompTransTime;
                //model.Status = or.Status;
                return PartialView("Partial2", or);
            }
            else
            {
                return PartialView("Error");
            }
        }
        public ActionResult EditApprover(string EPNo, string Approver)
        {
            try
            {
                string query = "update EPMaster SET APPROVER = '"+ Approver + "' WHERE EPNo = '" + EPNo + "'";
                var query_ = db.Database.SqlQuery<string>(query).First();

                string UseNam_ = "select usenam from usr where UseId = '"+ Approver + "'";
                var UseNam = db.Database.SqlQuery<string>(UseNam_).First();
                string No = EPNo;
                string UseId = Approver;

                var _User = _Nofif.GetUserToken(No, UseId, UseNam);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(e.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetExitPermitDatatables(string ExitPermitNo, string DateFrom, string DateTo)
        {
            try
            {
                var query = "update EPMaster set ExpireTicket = 'EXPIRED' Where DATEDIFF(MINUTE,(CONVERT(DATETIME, CONVERT(CHAR(8), [date], 112)+ ' ' + CONVERT(CHAR(8), [Out], 108))), GETDATE())> 60 SELECT 'SUCCESSED' as isSuccess";
                var query_ = db.Database.SqlQuery<string>(query).First();

                //Creating instance of DatabaseContext class
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                //Paging Size (10,20,50,100)  
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                if (Session["BusFunc"].ToString() == "SYSTEM-SECURITY")
                {
                    var _Permit = _epAction.GetExitPermitDatatablesSecurity(ExitPermitNo, DateFrom, DateTo);
                    recordsTotal = _Permit.Count();
                    var data = _Permit.Skip(skip).Take(pageSize).ToList();
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                }
                else
                {
                    var _Permit = _epAction.GetExitPermitDatatables(ExitPermitNo, DateFrom, DateTo);
                    recordsTotal = _Permit.Count();
                    var data = _Permit.Skip(skip).Take(pageSize).ToList();
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                }
                
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public JsonResult GetExitPermitDatatables2(string ExitPermitNo, string DateFrom, string DateTo)
        {
            try
            {
                var query = "update EPMaster set ExpireTicket = 'EXPIRED' Where DATEDIFF(MINUTE,(CONVERT(DATETIME, CONVERT(CHAR(8), [date], 112)+ ' ' + CONVERT(CHAR(8), [Out], 108))), GETDATE())> 60 SELECT 'SUCCESSED' as isSuccess";
                db.Database.SqlQuery<string>(query).First();
                //Creating instance of DatabaseContext class
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                //Paging Size (10,20,50,100)  
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var _Permit = _epAction.GetExitPermitDatatables2(ExitPermitNo, DateFrom, DateTo);

                //total number of rows count   
                recordsTotal = _Permit.Count();
                //Paging   
                var data = _Permit.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public FileResult ExportExitPermit(string start2, string end2, string EPNo)
        {
            DataTable table = new DataTable("ExitPermitReport");
            table = _epAction.ExportExitPermit(start2, end2, EPNo);
            table.TableName = "ExitPermitReport";
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(table);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", DateTime.Now.ToString("yyMMdd") + "_EPLog" + start2 + "_" + end2 + ".xlsx");
                }
            }
        }
        public ActionResult TriggerExport()
        {
            try
            {
                var result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
        }
    }
}