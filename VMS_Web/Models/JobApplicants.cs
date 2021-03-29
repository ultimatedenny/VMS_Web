using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VMS.Library;
using VMS.Web.Models;

namespace VMS.Web.Models
{
    public class JobApplicants
    {
        DataTable dt;
        long num;
        public string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        public List<JABatch> GetBatchListTask(string BatchId, string DateFrom, string DateTo)
        {
            string query = @"SELECT 
	        BatchComp
	        ,InvitationDate
	        ,RequestDate
	        ,StatusBatch
	        ,COUNT(BatchComp) As TotalCandidate
	        ,SUM(CASE WHEN StatusEmp = 'COMING' THEN 1 ELSE 0 END) As COMING
	        ,SUM(CASE WHEN StatusEmp = 'ABSENT' THEN 1 ELSE 0 END) As ABSENT
	        ,(CONVERT(FLOAT,SUM(CASE WHEN StatusEmp = 'COMING' THEN 1 ELSE 0 END))/CONVERT(FLOAT,COUNT(BatchComp)))*100 As PERCENTAGE
            FROM JobApplicant
            WHERE RequestDate BETWEEN '" + DateFrom + "' AND '" + DateTo + "' GROUP BY BatchComp, InvitationDate, RequestDate, StatusBatch";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _BatchListTask = (from rw in dt.AsEnumerable()
                                  select new JABatch
                                  {
                                      BatchComp = rw["BatchComp"].ToString(),
                                      InvitationDate = Convert.ToDateTime(rw["InvitationDate"].ToString()).ToString("yyyy-MM-dd"),
                                      RequestDate = Convert.ToDateTime(rw["RequestDate"].ToString()).ToString("yyyy-MM-dd"),
                                      TotalCandidate = rw["TotalCandidate"].ToString(),
                                      Coming = rw["COMING"].ToString(),
                                      Absent = rw["ABSENT"].ToString(),
                                      Percentage = rw["PERCENTAGE"].ToString() + "%",
                                      StatusBatch = rw["StatusBatch"].ToString(),
                                  }).ToList();
            return _BatchListTask;
        }
        public List<JABatch> GetBatchDetailsTask(string BatchComp, string NameEmp)
        {
            string query = @"SELECT DISTINCT BatchEmp, 
                            NameEmp,
                            DateOfBirthEmp,
                            InvitationDate,
                            UpdatedBy, 
                            UpdatedDate, 
                            StatusEmp 
                            FROM JobApplicant WHERE BatchComp = '" + BatchComp + @"' and NameEmp like '%" + NameEmp + @"%'";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _BatchListTask = (from rw in dt.AsEnumerable()
                                  select new JABatch
                                  {
                                      BatchEmp = rw["BatchEmp"].ToString(),
                                      NameEmp = rw["NameEmp"].ToString(),
                                      DateOfBirthEmp = Convert.ToDateTime(rw["DateOfBirthEmp"].ToString()).ToString("yyyy-MM-dd"),
                                      InvitationDate = Convert.ToDateTime(rw["InvitationDate"].ToString()).ToString("yyyy-MM-dd"),
                                      UpdatedBy = rw["UpdatedBy"].ToString(),
                                      UpdatedDate = Convert.ToDateTime(rw["InvitationDate"].ToString()).ToString("yyyy-MM-dd"),
                                      StatusEmp = rw["StatusEmp"].ToString(),
                                  }).ToList();
            return _BatchListTask;
        }
        public List<JABatch> GetBatchDetailsCount(string BatchComp)
        {
            string query = @"SELECT 
                            COUNT(BatchComp) As TOTALCANDIDATE,
                            SUM(CASE WHEN StatusEmp = 'COMING' THEN 1 ELSE 0 END) As COMING,
                            SUM(CASE WHEN StatusEmp = 'ABSENT' THEN 1 ELSE 0 END) As ABSENT,
                            (CONVERT(FLOAT,SUM(CASE WHEN StatusEmp = 'COMING' THEN 1 ELSE 0 END))/CONVERT(FLOAT,COUNT(BatchComp)))*100 As PERCENTAGE
                            FROM JobApplicant
                            WHERE BatchComp = '"+ BatchComp + "' GROUP BY BatchComp, InvitationDate, RequestDate, StatusBatch";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _BatchListTask = (from rw in dt.AsEnumerable()
                                  select new JABatch
                                  {
                                      TotalCandidate = rw["TOTALCANDIDATE"].ToString(),
                                      Coming = rw["COMING"].ToString(),
                                      Absent = rw["ABSENT"].ToString(),
                                      Percentage = rw["PERCENTAGE"].ToString() + "%"
                                  }).ToList();
            return _BatchListTask;
        }
        public DataTable ExportJobApplicant(string BatchComp)
        {
            DataTable dt = new DataTable();
            string query = $@"[spGetJobApplicant]";
            var param = new List<ctSqlVariable>();
            param.Add(new ctSqlVariable { Name = "BatchComp", Type = "string", Value = BatchComp });
            using (var sql = new MSSQL())
            {
                dt = sql.ExecuteStoProcDT(ConnectionDB, query, param);
            }
            return dt;
        }
        public VMSRes<string> PostChangeUserStatus(string StatusEmp, string _BatchEmp)
        {
            string query = @"UPDATE JobApplicant SET StatusEmp = '" + StatusEmp + @"' WHERE BatchEmp = '" + _BatchEmp + @"'";
            using (var sql = new MSSQL())
            {
                num = sql.ExecNonQuery(ConnectionDB, query, null, null, false);
            }
            return num.NonQueryResults();
        }
    }
}