using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VMS.Library;

namespace VMS.Web.Models
{
    public class Approval
    {
    }
    public class ApprovalAction
    {
        DataTable dt;
        long num;
        public string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        public VMSRes<string> ApproveAction(string Username, string LogId, string Status, string ApprovedRemark = "")
        {
            List<ctSqlVariable> paramss = new List<ctSqlVariable>();
            paramss.Add(new ctSqlVariable { Name = "LogId", Value = LogId, Type = "String" });
            paramss.Add(new ctSqlVariable { Name = "ApprovedBy", Value = Username, Type = "String" });
            paramss.Add(new ctSqlVariable { Name = "Status", Value = Status, Type = "String" });
            paramss.Add(new ctSqlVariable { Name = "ApprovedRemark", Value = ApprovedRemark, Type = "String" });
            using (var sql = new MSSQL())
            {
                dt = sql.ExecuteStoProcDT(ConnectionDB, "ApprovalAction", paramss);
            }
            return dt.OneRowdtToResult();
        }

        public VMSRes<string> ChangeDelegate(string isDelegate, string UserName)
        {
            string scalar;
            string query = $@"DECLARE @UseID varchar(50) = '{UserName}'
DECLARE @isDelegate varchar(5) = '{isDelegate}'
UPDATE Usr set isDelegate=@isDelegate where UseID=@UseID
DECLARE @UseLev varchar(1)
DECLARE @Approval varchar(10)
DECLARE @UseNam varchar(200)
select @UseLev=UseLev, @Approval=ApprovalGroup from Usr where UseID=@UseID
if EXISTS(select top 1 1 from Usr where ApprovalGroup=@Approval and UseLev=CAST(@UseLev as int)+1)
select @UseNam = COALESCE(@UseNam + '; ', '') + UseNam from Usr where ApprovalGroup=@Approval and UseLev=CAST(@UseLev as int)+1
else
set @UseNam='None'
select @UseNam as Usenam";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, query, null, null, false);
            }
            return scalar.ScalarResult();
        }

        public VMSRes<string> CheckAuthorize(string ApprovalGroup, string Uselev)
        {
            DataTable dt = new DataTable();
            string query = $@"DECLARE 
                            @UseLev nvarchar(1) = '{Uselev}',
                            @ApprovalGroup nvarchar (4) = '{ApprovalGroup}',
                            @UserPlant varchar(4) = '{Sessions.GetUsePlant()}'
				IF @UseLev = '1'
                begin
	                select 1 as isSuccess,'You Have Access to Approve' as [Message]
                end
				else if (@UseLev = '2' AND NOT EXISTS(SELECT * FROM Usr where UseLev = cast (@UseLev as INT) - 1 and ApprovalGroup=@ApprovalGroup and isDelegate=0 and UsePlant=@UserPlant))
				begin
	                select 1 as isSuccess,'You Have Access to Approve' as [Message]
                end
				else if (@UseLev = '3' AND NOT EXISTS(SELECT * FROM Usr where UseLev = cast (@UseLev as INT) - 1 and ApprovalGroup=@ApprovalGroup and isDelegate=0 and UsePlant=@UserPlant))
				begin
	                select 1 as isSuccess,'You Have Access to Approve' as [Message]
                end
				else
				BEGIN
				select 0 as isSuccess,'You do not Have Access to Approve' as [Message]
				END";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            return dt.OneRowdtToResult();

        }

        public VMSRes<string> CheckDelegate(string Username)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT top 1 isDelegate, CASE WHEN 
                            isDelegate = 0 then 'FALSE' else 'TRUE' end as [message]
                            FROM Usr where UseID='" + Username + "'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            return dt.OneRowdtToResult();
        }

        public List<LogHistory> ShowHistoryApproval(string Username)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT 
            t1.LogId,
            t1.Area,
			UseNam,
            STUFF(
                    (SELECT '; ' + FullName + ' (' + Company + ')'
                    FROM VisitLog V1
                    INNER JOIN Visitor  V2
                    ON V2.id = V1.VisitorId  where logid=t1.logid
                    ORDER BY V2.Company
                    FOR XML PATH('')), 1, 1, ''
            ) 
            [VisitorName], Remark, datestart, dateend, timeStart, TimeEnd, t1.status
            FROM VisitLog t1 
			INNER JOIN Usr on t1.hostId = UseId where t1.ApprovedBy='" + Username + @"'
and [ApprovedAt] between DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) and DATEADD(MONTH, DATEDIFF(MONTH, -1, GETDATE()), -1)            
GROUP BY 
            t1.LogId, t1.Area, UseNam, datestart, dateend, Remark, timeStart, TimeEnd, t1.status";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _visitLog = (from rw in dt.AsEnumerable()
                             select new LogHistory
                             {
                                 Area = rw["Area"].ToString(),
                                 HostName = rw["UseNam"].ToString(),
                                 FullName = rw["VisitorName"].ToString(),
                                 Remark = rw["Remark"].ToString(),
                                 DateStart = Convert.ToDateTime(rw["datestart"].ToString()).ToString("yyyy-MM-dd"),
                                 DateEnd = Convert.ToDateTime(rw["dateend"].ToString()).ToString("yyyy-MM-dd"),
                                 TimeStart = rw["timeStart"].ToString(),
                                 TimeEnd = rw["TimeEnd"].ToString(),
                                 Status = rw["status"].ToString(),
                             }).ToList();
            return _visitLog;
        }
        public List<LogHistory> ShowRequestApproval()
        {

            DataTable dt = new DataTable();
            string query = $@"DECLARE @ApprovalGroup varchar(20) = '{Sessions.GetApprovalGroup() ?? ""}'
SELECT 
            t1.LogId,
            t1.Area,
			U.UseNam,
			U.UseDep,
            STUFF(
                    (SELECT '; ' + FullName + ' (' + Company + ')<br> '
                    FROM VisitLog VL2
                    INNER JOIN Visitor  V2
                    ON V2.id = VL2.VisitorId  where logid=t1.logid
                    ORDER BY V2.Company
                    FOR XML PATH('')), 1, 1, ''
            ) 
            [VisitorName], Remark, datestart, dateend, timeStart, TimeEnd, t1.status
            FROM VisitLog t1 
			INNER JOIN Usr U on t1.hostId = UseId 
			INNER JOIN ApprovalGroup AG on U.UseDep = AG.Dept
			where t1.status='PENDING' and t1.Plant='{Sessions.GetUsePlant()}' and AG.ApprovalGroup=@ApprovalGroup
            GROUP BY 
            t1.LogId, t1.Area, UseNam, U.UseDep,datestart, dateend, Remark, timeStart, TimeEnd, t1.status";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }

            var _visitLog = (from rw in dt.AsEnumerable()
                             select new LogHistory
                             {
                                 LogId = rw["LogId"].ToString(),
                                 Area = rw["Area"].ToString(),
                                 HostName = rw["UseNam"].ToString(),
                                 FullName = rw["VisitorName"].ToString(),
                                 Remark = rw["Remark"].ToString(),
                                 DateStart = Convert.ToDateTime(rw["datestart"].ToString()).ToString("yyyy-MM-dd"),
                                 DateEnd = Convert.ToDateTime(rw["dateend"].ToString()).ToString("yyyy-MM-dd"),
                                 TimeStart = rw["timeStart"].ToString(),
                                 TimeEnd = rw["TimeEnd"].ToString(),
                                 Status = rw["status"].ToString(),

                             }).ToList();
            return _visitLog;
        }
    }
}