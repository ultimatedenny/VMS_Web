using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VMS.Library;

namespace VMS.Web.Class
{
    public static class Global
    {
        static string scalar;
        static DataTable dt;
        static long Num;
        static string ConnectionDB = System.Configuration.ConfigurationManager.ConnectionStrings["VMSConnection"].ToString();
        public static List<string> GetOUList()
        {
            string query = @"SELECT TOP 1 CodDes from CodLst where Cod = 'AD_OU_LIST'";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, query, null, null, false);
            }
            return scalar.Split(';').ToList();
        }
        public static string GetOUFilter()
        {
            string query = @"SELECT TOP 1 CodDes from CodLst where Cod = 'AD_OU_GROUP'";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, query, null, null, false);
            }
            return scalar;
        }
        public static string GetAdMemberOf(string Plant)
        {
            string query = $@"SELECT TOP 1 CodDes from CodLst where Cod = 'AD_FILTER_MEMBEROF' and CodAbb='{Plant}'";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, query, null, null, false);
            }
            return scalar;
        }
        public static bool GetVisitorConfigMandatory(string Cod)
        {
            string query = $@"SELECT TOP 1 CodDes from CodLst where Cod = '{Cod}'";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, query, null, null, false);
            }
            return scalar.ToBoolean();
        }
        public static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }
        public static string GetLocationPath(string GrpCod, string Cod, string CodAbb = "SPL")
        {
            string query = $@"SELECT TOP (1) [CodDes] FROM [dbo].[CodLst] where GrpCod='{GrpCod}' and Cod='{Cod}' and CodAbb like '%{CodAbb}%'";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, query, null, null, false);
            }
            return scalar;
        }
        public static bool IsAuth(this string BusFunc)
        {
            bool _isAuth;
            string queryCancel = $"select COUNT(*) from Codlst where GrpCod='VISITORCONFIG' and Cod='BUSFUNCCANCEL' and CodDes like '%{BusFunc}%'";
            using (var sql = new MSSQL())
            {
                scalar = sql.ExecuteScalar(ConnectionDB, queryCancel, null, null, false);
            }
            _isAuth = int.Parse(scalar) > 0 ? true : false;
            return _isAuth;
        }
    }

    public static class ADProp
    {
        public const string USEID = "userPrincipalName";
        public const string USENAM = "cn";
        public const string USEDEPT = "department";
        public const string USEMAIL = "mail";
        public const string USETEL   = "telephoneNumber";
        public const string MEMBEROF = "MEMBEROF";


    }

}