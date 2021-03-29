using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VMS.Web.Class;
using System.ComponentModel.DataAnnotations;

namespace VMS.Web.Models
{
    public class ImportExcel
    {
        [Required(ErrorMessage = "Please select file")]
        [FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file")]
        public HttpPostedFileBase file { get; set; }
    }
}