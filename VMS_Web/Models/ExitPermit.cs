using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VMS.Library;

namespace VMS.Web.Models
{
    public class ExitPermit
    {
        DataTable dt;
        long num;
        public string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        public List<EPMaster2> GetExitPermitDatatables(string ExitPermitNo, string DateFrom, string DateTo)
        {
            string query = @"SELECT	EM.MasterId as Id, 
                                EM.SENo as No,
		                        EM.EPNo as ExitPermit,
		                        EM.UseDep as Departement,
		                        P.PlantName as Section,
		                        EM.Destination as Destination,
		                        Convert(nvarchar(10),EM.[Date],121) as [Date],
		                        EM.[Out] as [Out],
		                        EM.[In] as [In],
		                        EM.CompTrans as CompanyTransport,
		                        EM.CompTransTime as CompanyTransportTime,
		                        EM.[Status] as [Status]
                                from EPMASTER EM
                                INNER JOIN Plant P on EM.PlantID = P.plantId where EM.EPNo like '%" + ExitPermitNo + @"%' and  EM.[Status] = 'PENDING' and
                                                                                   EM.[ExpireTicket] = 'ACTIVE' and
                                                                                   EM.[Date] between '" + DateFrom + @"' and '" + DateTo + @"' Order by EM.[Date] DESC ";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _ExitPermit = (from rw in dt.AsEnumerable()
                               select new EPMaster2
                               {
                                   MasterId = rw["Id"].ToString(),
                                   SENo = int.Parse(rw["No"].ToString()),
                                   EPNo = rw["ExitPermit"].ToString(),
                                   UseDep = rw["Departement"].ToString(),
                                   PlantID = rw["Section"].ToString(),
                                   Destination = rw["Destination"].ToString(),
                                   Date = Convert.ToDateTime(rw["Date"].ToString()).ToString("yyyy-MM-dd"),
                                   In = TimeSpan.Parse(rw["In"].ToString()).ToString(),
                                   Out = TimeSpan.Parse(rw["Out"].ToString()).ToString(),
                                   CompTrans = bool.Parse(rw["CompanyTransport"].ToString()).ToString(),
                                   CompTransTime = TimeSpan.Parse(rw["CompanyTransportTime"].ToString()).ToString(),
                                   Status = rw["Status"].ToString(),
                               }).ToList();
            return _ExitPermit;
        }
        public List<EPMaster2> GetExitPermitDatatablesSecurity(string ExitPermitNo, string DateFrom, string DateTo)
        {
            string query = @"SELECT	EM.MasterId as Id, 
                                EM.SENo as No,
		                        EM.EPNo as ExitPermit,
		                        EM.UseDep as Departement,
		                        P.PlantName as Section,
		                        EM.Destination as Destination,
		                        Convert(nvarchar(10),EM.[Date],121) as [Date],
		                        EM.[Out] as [Out],
		                        EM.[In] as [In],
		                        EM.CompTrans as CompanyTransport,
		                        EM.CompTransTime as CompanyTransportTime,
		                        EM.[Status] as [Status]
                                from EPMASTER EM
                                INNER JOIN Plant P on EM.PlantID = P.plantId where EM.EPNo like '%" + ExitPermitNo + @"%' and  EM.[Status] = 'APPROVED' and 
                                                                                   EM.[ExpireTicket] = 'ACTIVE' and
                                                                                   EM.[Date] between '" + DateFrom + @"' and '" + DateTo + @"'Order by EM.[Date] DESC";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _ExitPermit = (from rw in dt.AsEnumerable()
                               select new EPMaster2
                               {
                                   MasterId = rw["Id"].ToString(),
                                   SENo = int.Parse(rw["No"].ToString()),
                                   EPNo = rw["ExitPermit"].ToString(),
                                   UseDep = rw["Departement"].ToString(),
                                   PlantID = rw["Section"].ToString(),
                                   Destination = rw["Destination"].ToString(),
                                   Date = Convert.ToDateTime(rw["Date"].ToString()).ToString("yyyy-MM-dd"),
                                   In = TimeSpan.Parse(rw["In"].ToString()).ToString(),
                                   Out = TimeSpan.Parse(rw["Out"].ToString()).ToString(),
                                   CompTrans = bool.Parse(rw["CompanyTransport"].ToString()).ToString(),
                                   CompTransTime = TimeSpan.Parse(rw["CompanyTransportTime"].ToString()).ToString(),
                                   Status = rw["Status"].ToString(),
                               }).ToList();
            return _ExitPermit;
        }
        public List<EPMaster2> GetExitPermitDatatablesHistory(string ExitPermitNo, string UseId, string DateFrom, string DateTo)
        {
            string query = @"SELECT	EM.MasterId as Id, 
                                EM.SENo as No,
		                        EM.EPNo as ExitPermit,
		                        EM.UseDep as Departement,
		                        P.PlantName as Section,
		                        EM.Destination as Destination,
								EM.CreatedBy as Requestor,
		                        Convert(nvarchar(10),EM.[Date],121) as [Date],
		                        EM.[Out] as [Out],
		                        EM.[In] as [In],
		                        EM.CompTrans as CompanyTransport,
		                        EM.CompTransTime as CompanyTransportTime,
		                        EM.[Status] as [Status]
                                from EPMASTER EM
                                INNER JOIN Plant P on EM.PlantID = P.plantId where EM.EPNo like '%" + ExitPermitNo + @"%' and  EM.[Status] = 'PENDING' and EM.CreatedBy = '" + UseId + @"' and
                                                                                   EM.[Date] between '" + DateFrom + @"' and '" + DateTo + @"'Order by EM.[Date] DESC";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _ExitPermit = (from rw in dt.AsEnumerable()
                               select new EPMaster2
                               {
                                   MasterId = rw["Id"].ToString(),
                                   SENo = int.Parse(rw["No"].ToString()),
                                   EPNo = rw["ExitPermit"].ToString(),
                                   UseDep = rw["Departement"].ToString(),
                                   PlantID = rw["Section"].ToString(),
                                   Destination = rw["Destination"].ToString(),
                                   Date = Convert.ToDateTime(rw["Date"].ToString()).ToString("yyyy-MM-dd"),
                                   In = TimeSpan.Parse(rw["In"].ToString()).ToString(),
                                   Out = TimeSpan.Parse(rw["Out"].ToString()).ToString(),
                                   CompTrans = bool.Parse(rw["CompanyTransport"].ToString()).ToString(),
                                   CompTransTime = TimeSpan.Parse(rw["CompanyTransportTime"].ToString()).ToString(),
                                   Status = rw["Status"].ToString(),
                                   CreatedBy = rw["Requestor"].ToString(),
                               }).ToList();
            return _ExitPermit;
        }

        public List<EPMasterDetail> GetExitPermitDatatables2(string ExitPermitNo, string DateFrom, string DateTo)
        {
            string query = @"SELECT EM.MasterId as MasterId, 
                                EM.EPNo as ExitPermit,
                                P.PlantName as Section,
                                EM.UseDep as Departement,
                                EM.Destination as Destination,
                                Convert(nvarchar(10),EM.[Date],121) as [Date],
                                ISNULL(EM.[Out], '00:00')  as [Out],
								ISNULL(EM.[In], '00:00')  as [In],
								ISNULL(EM.ActOut, '00:00')  as ActOut,
								ISNULL(EM.ActIn, '00:00')  as ActIn,
                                EM.CompTrans as CompanyTransport,
                                ISNULL(EM.CompTransTime, '00:00')  as CompanyTransportTime,
                                EM.[Status] as [Status],
								ISNULL(A.Employee, '-') as Employee,
                                U.UseNam as CreatedBy
                                into #Temp1 
                                from EPMaster EM
                                INNER JOIN Plant P on EM.PlantID = P.plantId
                                INNER JOIN Usr U on EM.CreatedBy = U.UseID
                                INNER JOIN (SELECT SS.MasterId,
                                   STUFF((SELECT ', ' + U.UseNam 
                                          FROM EPDetails US
		                                  INNER JOIN Usr U on US.UseId = U.UseID
                                          WHERE US.MasterId = SS.MasterId
                                          ORDER BY EPNo
                                          FOR XML PATH('')), 1, 1, '') AS Employee
                                FROM EPMaster SS) A on EM.MasterId = A.MasterId
                                where EM.MasterId = EM.MasterId
                                SELECT * FROM #Temp1 where ExitPermit like '%" + ExitPermitNo + @"%' AND Status = 'COMPLETED' AND  Date between '" + DateFrom + @"' and '" + DateTo + @"' ORDER BY DATE DESC
                                DROP TABLE #Temp1";
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            var _ExitPermit = (from rw in dt.AsEnumerable()
                               select new EPMasterDetail
                               {
                                   MasterId = rw["MasterId"].ToString(),
                                   EPNo = rw["ExitPermit"].ToString(),
                                   UseDep = rw["Departement"].ToString(),
                                   PlantID = rw["Section"].ToString(),
                                   CreateBy = rw["CreatedBy"].ToString(),
                                   Employee = rw["Employee"].ToString(),
                                   Destination = rw["Destination"].ToString(),
                                   Date = Convert.ToDateTime(rw["Date"].ToString()).ToString("yyyy-MM-dd"),
                                   Out = TimeSpan.Parse(rw["Out"].ToString()).ToString(),
                                   ActOut = TimeSpan.Parse(rw["ActOut"].ToString()).ToString(),
                                   In = TimeSpan.Parse(rw["In"].ToString()).ToString(),
                                   ActIn = TimeSpan.Parse(rw["ActIn"].ToString()).ToString(),
                                   CompTrans = bool.Parse(rw["CompanyTransport"].ToString()).ToString(),
                                   CompTransTime = TimeSpan.Parse(rw["CompanyTransportTime"].ToString()).ToString(),
                                   Status = rw["Status"].ToString(),
                               }).ToList();
            return _ExitPermit;
        }

        public DataTable ExportExitPermit(string DateStart, string DateEnd, string EPNo)
        {
            DataTable dt = new DataTable();
            string query = "EXEC [spGetExitPermit] @EPNo = '"+ EPNo + "', @DateStart= '"+ DateStart + "' ,@DateEnd= '" + DateEnd + "'";
            //"EXEC [spGetExitPermit] @EPNo = '', @DateStart= '2021-01-04' ,@DateEnd= '2021-01-05'"
            //var param = new List<ctSqlVariable>();
            //param.Add(new ctSqlVariable { Name = "DateStart", Type = "string", Value = DateStart });
            //param.Add(new ctSqlVariable { Name = "DateEnd", Type = "string", Value = DateEnd });
            //param.Add(new ctSqlVariable { Name = "EPNo", Type = "string", Value = EPNo });
            using (var sql = new MSSQL())
            {
                dt = sql.ExecDTQuery(ConnectionDB, query, null, null, false);
            }
            return dt;
        }
    }
}