using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using VMS.Library;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace VMS.Web.Models
{
    public class MasterDataAction
    {
        DataTable dt;
        long num;
        public string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        public VMSRes<string> ChangeStatusMasterData(string TableName, string ColumnName, string id, string Username)
        {
            string query = @"
                IF EXISTS(SELECT 1 FROM " + TableName + @" WHERE " + ColumnName + @" ='" + id + @"')
                BEGIN
                    UPDATE " + TableName + @" set isActive = case
                        when isActive = 1 then 0 else 1 end
	                    where  " + ColumnName + @" = '" + id + @"'
                END";
            using (var sql = new MSSQL())
            {
                num = sql.ExecNonQuery(ConnectionDB, query, null, null, false);
            }
            return num.NonQueryResults();
        }

        //DataTables
        public List<AreaVisitor> GetAreaDatatables(string AreaName)
        {
            string query = @"SELECT PlantName, areaId, areaName, AV.VisitorType, VT.VisitorType as VisitorTypeName
                            from AreaVisitor AV
                            INNER JOIN Plant P on AV.Plant = P.plantId
                            INNER JOIN VisitorType VT on AV.VisitorType = VT.id where AV.Plant like '%" + AreaName + @"%' or AV.areaId like '%" + AreaName + @"%' or AV.AreaName like '%" + AreaName + @"%' or P.plantName like '%" + AreaName + @"%'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _visitorArea = (from rw in dt.AsEnumerable()
                                select new AreaVisitor
                                {
                                    PlantName = rw["PlantName"].ToString(),
                                    VisitorType = int.Parse(rw["VisitorType"].ToString()),
                                    VisitorTypeName = rw["VisitorTypeName"].ToString(),
                                    areaId = rw["areaId"].ToString(),
                                    areaName = rw["areaName"].ToString(),
                                }).ToList();
            return _visitorArea;
        }
       
        public List<VisitType> GetVisitorTypeDatatables()
        {
            string query = @"SELECT [Id]
              ,[VisitorType]
              ,[NeedApprove] FROM [VisitorType]";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _visitorType = (from rw in dt.AsEnumerable()
                                select new VisitType
                                {
                                    Id = int.Parse(rw["Id"].ToString()),
                                    VisitorType = rw["VisitorType"].ToString(),
                                    NeedApprove = rw["NeedApprove"].ToString(),
                                }).ToList();
            return _visitorType;
        }

        public AreaVisitor GetAreaDetail(string AreaId, string VisitorType)
        {
            string query = @"SELECT Plant, areaId, areaName, AV.VisitorType
            from AreaVisitor AV
            INNER JOIN Plant P on AV.Plant = P.plantId
            INNER JOIN VisitorType VT on AV.VisitorType = VT.id
            WHERE areaId='" + AreaId + @"' and AV.VisitorType='" + VisitorType + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _visitorArea = (from rw in dt.AsEnumerable()
                                select new AreaVisitor
                                {
                                    Plant = int.Parse(rw["Plant"].ToString()),
                                    VisitorType = int.Parse(rw["VisitorType"].ToString()),
                                    areaId = rw["areaId"].ToString(),
                                    areaName = rw["areaName"].ToString(),
                                }).FirstOrDefault();
            return _visitorArea;
        }
        //public ExitPermit2 GetExitPermitDetail(string Id)
        //{
        //    string query = @"SELECT [Id], EPNo, U.UseDep as UserDep, U.UseID as UserID, U.UseNam as Username, P.PlantName as PlantName, Destination, EP.[Date] as PermitDate, EP.[Out] as PermitTimeOut, EP.[In] as PermitTimeIn, ActOut, ActIn, CompTrans
        //                    from ExitPermit EP
        //                    INNER JOIN Plant P on EP.plantID = P.plantId
        //                    INNER JOIN Usr U on EP.UseID = U.UseID where Id like '%" + Id + @"%'";
        //    using (var sql = new MSSQL())
        //    {
        //        dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
        //    }
        //    var _ExitPermit = (from rw in dt.AsEnumerable()
        //                       select new ExitPermit2
        //                       {
        //                           Id = int.Parse(rw["Id"].ToString()),
        //                           EPNo = rw["EPNo"].ToString(),
        //                           UseDep = rw["UserDep"].ToString(),
        //                           UseID = rw["UserID"].ToString(),
        //                           Username = rw["Username"].ToString(),
        //                           plantID = rw["PlantName"].ToString(),
        //                           Destination = rw["Destination"].ToString(),
        //                           Date = Convert.ToDateTime(rw["PermitDate"].ToString()).ToString("yyyy-MM-dd"),
        //                           In = TimeSpan.Parse(rw["PermitTimeIn"].ToString()).ToString(),
        //                           Out = TimeSpan.Parse(rw["PermitTimeOut"].ToString()).ToString(),
        //                           CompTrans = bool.Parse(rw["CompTrans"].ToString()).ToString(),
        //                           ActIn = TimeSpan.Parse(rw["ActIn"].ToString()).ToString(),
        //                           ActOut = TimeSpan.Parse(rw["ActOut"].ToString()).ToString(),
        //                       }).FirstOrDefault();
        //    return _ExitPermit;
        //}
        public VisitType GetVisitorTypeDetail(string Id)
        {
            string query = @"SELECT [Id]
      ,[VisitorType]
      ,[NeedApprove] FROM [VisitorType] where Id='" + Id + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _visitorType = (from rw in dt.AsEnumerable()
                                select new VisitType
                                {
                                    Id = int.Parse(rw["Id"].ToString()),
                                    VisitorType = rw["VisitorType"].ToString(),
                                    NeedApprove = rw["NeedApprove"].ToString(),
                                }).FirstOrDefault();
            return _visitorType;
        }

        //Save
        public VMSRes<string> SaveArea(AreaVisitor _area, string oldArea, string oldVisitorType)
        {
            string query = @"
            IF EXISTS(SELECT * FROM AreaVisitor where areaId='" + oldArea + "' and VisitorType='" + oldVisitorType + @"')
            BEGIN
            Update AreaVisitor set 
            Plant='" + _area.Plant + @"', 
            areaId='" + _area.areaId + @"', 
            areaName='" + _area.areaName + @"', 
            VisitorType='" + _area.VisitorType + @"', 
            UpdateBy='" + _area.UpdateBy + @"', 
            UpdateAt='" + DateTime.Now.ToString("yyyy-MM-dd") + @"' where areaId='" + oldArea + @"' and VisitorType = '" + oldVisitorType + @"' 
            END
                    ELSE
            BEGIN
            INSERT INTO AreaVisitor(Plant, AreaId, AreaName, VisitorType, UpdateAt, UpdateBy)
            VALUES('" + _area.Plant + @"','" + _area.areaId + @"','" + _area.areaName + @"','" + _area.VisitorType + @"','" + DateTime.Now.ToString("yyyy-MM-dd") + @"','" + _area.UpdateBy + @"')
            END";
            using (var sql = new MSSQL())
            {
                num = sql.ExecNonQuery(ConnectionDB, query, null, null, false);
            }
            return num.NonQueryResults();
        }
        //public VMSRes<string> SaveExitPermit(ExitPermit2 ExitPermit)
        //{
        //    string query = @"
        //    IF EXISTS(SELECT * FROM ExitPermit where Id='" + ExitPermit.Id + @"')
        //    BEGIN
        //    UPDATE [dbo].[ExitPermit]
        //       SET [UseDep] = '" + ExitPermit.UseDep + @"'
        //          ,[UseID] = '" + ExitPermit.UseID + @"'
        //          ,[plantID] = '" + ExitPermit.plantID + @"'
        //          ,[Destination] = '" + ExitPermit.Destination + @"'
        //          ,[Date] = '" + ExitPermit.Date + @"'
        //          ,[Out] = '" + ExitPermit.Out + @"'
        //          ,[ActOut] = '" + ExitPermit.ActOut + @"'
        //          ,[In] = '" + ExitPermit.In + @"'
        //          ,[ActIn] = '" + ExitPermit.ActIn + @"'
        //          ,[CompTrans] = '" + ExitPermit.CompTrans + @"'
        //    WHERE Id='" + ExitPermit.Id + @"'
        //    END
        //    ELSE
        //    BEGIN
        //    DECLARE 
        //    @maxId int
        //    set @MaxId = (select isnull (max(cast(id as int))+1,1) from ExitPermit)
        //    INSERT INTO [dbo].[ExitPermit]
        //               ([Id]
					   //,[EPNo]
					   //,[UseDep]
        //               ,[UseID]
        //               ,[plantID]
        //               ,[Destination]
					   //,[Date]
					   //,[Out]
        //               ,[ActOut]
					   //,[In]
        //               ,[ActIn]
        //               ,[CompTrans]
					   //,[CreatedBy]
					   //,[CreatedDate]
					   //)
        //         VALUES
        //               (
        //                   @MaxId
        //                   ,'EP'+ CONVERT([NVARCHAR](50),FORMAT(GETDATE(), 'yyMMdd')) + '-' + REPLICATE('0',(3) - LEN(CONVERT([NVARCHAR](50),@MaxId)))+ CONVERT([NVARCHAR](50),@MaxId)
        //                   ,'" + ExitPermit.UseDep + @"'
        //                   ,'" + ExitPermit.UseID + @"'
        //                   ,'" + ExitPermit.plantID + @"'
        //                   ,'" + ExitPermit.Destination + @"'
        //                   ,'" + ExitPermit.Date + @"'
						  // ,'" + ExitPermit.Out + @"'
        //                   ,'" + ExitPermit.ActOut + @"'
        //                   ,'" + ExitPermit.In + @"'
        //                   ,'" + ExitPermit.ActIn + @"'
        //                   ,'" + ExitPermit.CompTrans + @"'
        //                   ,'" + ExitPermit.CreatedBy + @"'
        //                   ,'" + DateTime.Now.ToString("yyyy-MM-dd") + @"'
        //               )
        //    END
        //    ";
        //    using (MSSQL sql = new MSSQL())
        //    {
        //        num = sql.ExecNonQuery(ConnectionDB, query, null, null, false);
        //    }
        //    return num.NonQueryResults();
        //}
        public VMSRes<string> SaveVisitorType(VisitType VisitType)
        {
            string query = @"
            IF EXISTS(SELECT * FROM VISITORTYPE where Id='" + VisitType.Id + @"')
            BEGIN
            UPDATE [dbo].[VisitorType]
               SET [VisitorType] = '" + VisitType.VisitorType + @"'
                  ,[NeedApprove] = '" + VisitType.NeedApprove + @"'
                  ,[UpdateAt] = '" + DateTime.Now.ToString("yyyy-MM-dd") + @"'
                  ,[UpdateOn] = '" + VisitType.UpdateBy + @"'
             WHERE Id='" + VisitType.Id + @"'
            END
            ELSE
            BEGIN
            DECLARE 
            @maxId int
            set @MaxId = (select isnull (max(cast(id as int))+1,1) from VisitorType)
            INSERT INTO [dbo].[VisitorType]
                       ([Id]
                       ,[VisitorType]
                       ,[NeedApprove]
                       ,[UpdateAt]
                       ,[UpdateOn])
                 VALUES
            (
                       @MaxId
                       ,'" + VisitType.VisitorType + @"'
                       ,'" + VisitType.NeedApprove + @"'
                       ,'" + DateTime.Now.ToString("yyyy-MM-dd") + @"'
                       ,'" + VisitType.UpdateBy + @"')
            END
            ";
            using (var sql = new MSSQL())
            {
                num = sql.ExecNonQuery(ConnectionDB, query, null, null, false);
            }
            return num.NonQueryResults();
        }

        //
        public List<AreaVisitor> GetAreaforDDList(string Plant, string VisitType)
        {
            DataTable dt = new DataTable();
            string query = @"select areaId, areaName from AreaVisitor where plant='" + Plant + "' and VisitorType = '" + VisitType + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _visitorArea = (from rw in dt.AsEnumerable()
                                select new AreaVisitor
                                {
                                    areaId = rw["areaId"].ToString(),
                                    areaName = rw["areaName"].ToString(),
                                }).ToList();
            return _visitorArea;
        }
        //GetApproverforDDList
        public List<Approver> GetApproverforDDList(string PlantName = "")
        {
            DataTable dt = new DataTable();
            string query = @"SELECT  UseID, UseNam into #Temp1 FROM Usr WHERE BusFunc = 'SYSTEM-DIRECTOR'
                            INSERT INTO #Temp1 (UseID, UseNam) VALUES ('MGT','Dept. Manager')
                            SELECT * FROM #Temp1
                            DROP TABLE #Temp1";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _Approver = (from rw in dt.AsEnumerable()
                           select new Approver
                           {
                               UseId = rw["UseId"].ToString(),
                               UseNam = rw["UseNam"].ToString(),
                           }).ToList();
            return _Approver;
        }
        public List<Approver> GetApproverforDDList2(string PlantName = "")
        {
            DataTable dt = new DataTable();
            string query = @"SELECT UseId, UseNam FROM USR WHERE BUSFUNC = 'SYSTEM-DIRECTOR'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _Approver = (from rw in dt.AsEnumerable()
                             select new Approver
                             {
                                 UseId = rw["UseId"].ToString(),
                                 UseNam = rw["UseNam"].ToString(),
                             }).ToList();
            return _Approver;
        }

        public List<Approver> GetApproverforDDList3(string Dept, string Plant)
        {
            DataTable dt = new DataTable();
            string query = @"EXEC [spGetExitPermitApproval2] '"+ Dept + "', '"+ Plant + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _Approver = (from rw in dt.AsEnumerable()
                             select new Approver
                             {
                                 UseId = rw["UseId"].ToString(),
                                 UseNam = rw["UseNam"].ToString(),
                             }).ToList();
            return _Approver;
        }

        public List<Plant> GetPlantforDDList(string PlantName = "")
        {
            DataTable dt = new DataTable();
            string query = @"SELECT plantId, plantName FROM Plant where plantName like '%" + PlantName + @"%'
                            ";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _plants = (from rw in dt.AsEnumerable()
                           select new Plant
                           {
                               plantId = Convert.ToInt16(rw["plantId"].ToString()),
                               plantName = rw["plantName"].ToString(),
                           }).ToList();
            return _plants;
        }
        public List<VisitType> GetVisitTypeforDDList(string VisitType = "")
        {
            DataTable dt = new DataTable();
            string query = @"SELECT id, VisitorType FROM VisitorType
                            where visitorType is not null";
            if (VisitType == "1")
            {
                query += @" and NeedApprove != 1";
            }
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _visitorType = (from rw in dt.AsEnumerable()
                                select new VisitType
                                {
                                    Id = Convert.ToInt16(rw["Id"].ToString()),
                                    VisitorType = rw["VisitorType"].ToString(),
                                }).ToList();
            return _visitorType;
        }

        //
        public List<Visitor> PopulateVisitorCompany(string text)
        {
            DataTable dt = new DataTable();
            string query = $@"DECLARE @USEMDM varchar(50)
set  @USEMDM = (select CodDes from CodLst where GrpCod='VISITORCONFIG' and Cod='USINGMDM')if (@USEMDM is null or @USEMDM = 'true')
BEGIN
SELECT DISTINCT [Description] from MDMDB.dbo.TVENDOR where [Description] Like'%{text}%' UNION
SELECT DISTINCT Company as [Description] from Visitor where Company Like '%{text}%'
END
else
BEGIN
SELECT DISTINCT Company as [Description] from Visitor where Company Like '%{text}%'
END
";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _data = (from rw in dt.AsEnumerable()
                         select new Visitor
                         {
                             Company = rw["Description"].ToString()
                         }).ToList();
            return _data;
        }
        public List<Department> GetDepartmentforDDList(string plant)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT Dept, DeptName from Dept where plant = '" + plant + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _data = (from rw in dt.AsEnumerable()
                         select new Department
                         {
                             Dept = rw["Dept"].ToString(),
                             DeptName = rw["DeptName"].ToString()
                         }).ToList();
            return _data;
        }
        public List<CodLst> GetCodLst(string GrpCod)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT Cod, CodAbb from CodLst where GrpCod = '" + GrpCod + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _data = (from rw in dt.AsEnumerable()
                         select new CodLst
                         {
                             Cod = rw["Cod"].ToString(),
                             CodAbb = rw["CodAbb"].ToString()
                         }).ToList();
            return _data;
        }
        public List<Plant> GetPlantMDMList(string PlantName = "")
        {
            DataTable dt = new DataTable();
            string query = @"SELECT [Plant], [Description] FROM [MDMDB].[dbo].[TPLANT] where [Description] like '%" + PlantName + @"%' ";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _plants = (from rw in dt.AsEnumerable()
                           select new Plant
                           {
                               plantId = Convert.ToInt16(rw["Plant"].ToString()),
                               plantName = rw["Description"].ToString(),
                           }).ToList();
            return _plants;
        }

    }

    public enum Status
    {
        PENDING = 1,
        APPROVED = 2,
        REJECTED = 3,
        CHECKIN = 4,
        BREAK = 5,
        CHECKOUT = 6,
        DELETE = 7

    }
    public enum Method
    {
        Security = 1,
        User = 2
    }
    class MasterDataCommon
    {
    }
    public class Plant
    {
        public int plantId { get; set; }
        public string plantName { get; set; }
        public DateTime UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
    public class Approver
    {
        public string UseNam { get; set; }
        public string UseId { get; set; }
    }
    public class VisitType
    {
        public int Id { get; set; }
        public string VisitorType { get; set; }
        public string NeedApprove { get; set; }
        public string UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
    public class AreaVisitor
    {
        public int Plant { get; set; }
        public string PlantName { get; set; }
        public string areaGroupId { get; set; }
        public string areaId { get; set; }
        public string areaName { get; set; }
        public int VisitorType { get; set; }
        public string VisitorTypeName { get; set; }
        public DateTime UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
    public class SqlVariable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
    public class HostAppointment
    {
        public string LogId { get; set; }
        public string UseID { get; set; }
        public string UseNam { get; set; }
        public string UseTel { get; set; }
        public string UseDep { get; set; }
        public string Area { get; set; }
        public string VisitorType { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string NeedApprove { get; set; }
    }
    public class VisitLogArea
    {
        public string LogId { get; set; }
        public string AreaId { get; set; }

    }
    public class Chart
    {
        public int Data { get; set; }
        public string Name { get; set; }
    }
    public class MenulistDto
    {
        public string MenuName { get; set; }
        public string MenuCode { get; set; }
        public string MenuAction { get; set; }
        public string MenuParent { get; set; }
        public string MenuController { get; set; }
        public string MenuIcon { get; set; }
    }
    public class BusFunc
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class Menulist
    {
        public string MenuName { get; set; }
        public string MenuCode { get; set; }
        public string MenuAction { get; set; }
        public string MenuParent { get; set; }
        public string MenuController { get; set; }
        public string MenuIcon { get; set; }
        public string MenuSeq { get; set; }
        public string isView { get; set; }
    }
    public class LogBook
    {
        public int LogId { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string InfoGrant { get; set; }
        public string NameGrant { get; set; }
        public string DateGrant { get; set; }
        public string TimeGrant { get; set; }
        public string PhotoGrant { get; set; }
        public string NameReceive { get; set; }
        public string DateReceive { get; set; }
        public string TimeReceive { get; set; }
        public string PhotoReceive { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string Plant { get; set; }
    }
    public class Department
    {
        public string Plant { get; set; }
        public string Dept { get; set; }
        public string DeptName { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateAt { get; set; }
    }
    public class CodLst
    {
        public string GrpCod { get; set; }
        public string Cod { get; set; }
        public string CodAbb { get; set; }
        public string CodDesc { get; set; }

    }
    public class PhotoLogBook
    {
        public string PhotoItem { get; set; }
        public string PhotoUser { get; set; }
    }


    public class ExitPermit2
    {
        public int Id { get; set; }
        public string EPNo { get; set; }
        public string UseDep { get; set; }
        public string UseID { get; set; }
        public string Username { get; set; }
        public string plantID { get; set; }
        public string Destination { get; set; }
        public string Date { get; set; }
        public string Out { get; set; }
        public string In { get; set; }
        public string ActOut { get; set; }
        public string ActIn { get; set; }
        public string CompTrans { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
    }
    public class EPMaster2
    {
        public string MasterId { get; set; }
        public int SENo { get; set; }
        public string EPNo { get; set; }
        public string UseDep { get; set; }
        public string PlantID { get; set; }
        public string Destination { get; set; }
        public string Date { get; set; }
        public string Out { get; set; }
        public string In { get; set; }
        public string CompTrans { get; set; }
        public string CompTransTime { get; set; }
        public string Status { get; set; }
        public string ExpireTicket { get; set; }
        public string Approver { get; set; }
        public string CreatedBy { get; set; }
        public string Remarks { get; set; }
    }
    public class JABatch
    {
        public string BatchComp { get; set; }
        public string InvitationDate { get; set; }
        public string RequestDate { get; set; }
        public string TotalCandidate { get; set; }
        public string StatusBatch { get; set; }
        public string BatchEmp { get; set; }
        public string NameEmp { get; set; }
        public string DateOfBirthEmp { get; set; }
        public string PhoneNumber { get; set; }
        public string InvitationCodeEmp { get; set; }
        public string StatusEmp { get; set; }
        public string UploadedDate { get; set; }
        public string UploadedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string Coming { get; set; }
        public string Absent { get; set; }
        public string Percentage { get; set; }

    }
    public class EPMasterDetail
    {
        public string MasterId { get; set; }
        public int SENo { get; set; }
        public string EPNo { get; set; }
        public string UseDep { get; set; }
        public string PlantID { get; set; }
        public string Destination { get; set; }
        public string Date { get; set; }
        public string Out { get; set; }
        public string In { get; set; }
        public string CompTrans { get; set; }
        public string CompTransTime { get; set; }
        public string Status { get; set; }
        public string DetailId { get; set; }
        public string Employee { get; set; }
        public string CreateBy { get; set; }
        public string ActOut { get; set; }
        public string ActIn { get; set; }
        public int Id { get; set; }
    }


    public class MDeliveryOrder
    {
        public string MasterId { get; set; }
        public int SENo { get; set; }
        public string DONo { get; set; }
        public string UseDep { get; set; }
        public string UseID { get; set; }
        public string ReqDate { get; set; }
        public string Address { get; set; }
        public string DelVia { get; set; }
        public string DriName { get; set; }
        public string VechNo { get; set; }
        public string TimeOut { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string ReceiverName { get; set; }
        public string ReceivedDate { get; set; }
        public string ReceivedPic { get; set; }
        public string SecurityCheck { get; set; }
        public string SecurityPic { get; set; }
        public string ManagerApprove { get; set; }
        public string Status { get; set; }
    }
    public class DDeliveryOrder
    {
        public string DetailId { get; set; }
        public string MasterId { get; set; }
        public int Id { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public string Remark { get; set; }
        public string IsReturned { get; set; }
        public string ReturnedBy { get; set; }
        public string ReturnedDate { get; set; }
        public string Photo { get; set; }
    }
    public class MDDeliveryOrder
    {
        public string MasterId { get; set; }
        public int SENo { get; set; }
        public string DONo { get; set; }
        public string UseDep { get; set; }
        public string UseID { get; set; }
        public string ReqDate { get; set; }
        public string Address { get; set; }
        public string DelVia { get; set; }
        public string DriName { get; set; }
        public string VechNo { get; set; }
        public string TimeOut { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string ReceiverName { get; set; }
        public string ReceivedDate { get; set; }
        public string ReceivedPic { get; set; }
        public string SecurityCheck { get; set; }
        public string SecurityPic { get; set; }
        public string ManagerApprove { get; set; }
        public string Status { get; set; }
        public string DetailId { get; set; }
        public int Id { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public string Remark { get; set; }
        public string IsReturned { get; set; }
        public string ReturnedBy { get; set; }
        public string ReturnedDate { get; set; }
        public string Photo { get; set; }
    }

    
    public class Attendance
    {
        public int LogId { get; set; }
        public int TSVisitorId { get; set; }
        public string HostName { get; set; }
        public string HostDepartment { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string DateVisit { get; set; }
        public string NeedLunch { get; set; }
        public string Status { get; set; }
        public string Plant { get; set; }
        public string Remark { get; set; }
        public string NeedStay { get; set; }
        public string DateofEnd { get; set; }
        public string Premises { get; set; }
    }
}
