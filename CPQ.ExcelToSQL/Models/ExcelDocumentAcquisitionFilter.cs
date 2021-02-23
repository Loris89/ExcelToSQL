using System;
using System.Collections.Generic;
using System.Text;

namespace CPQ.ExcelToSQL.Models
{
    //DocumentAcquisitionFilter 
    public class ExcelDocumentAcquisitionFilter
    {
        public string COD_AUC { get; set; }
        public string CIAE { get; set; }
        public string SAE { get; set; }
        public string ID_DOC_LIST { get; set; }//new parameter
        /// <summary>
        /// string input dell'excel da trasformare in List<ExcelMappingDocumentItem>
        /// </summary>
        public string InputMappingDocs { get; set; }//ID Doc O/F Sostituto
        public List<ExcelMappingDocumentItem> MappingDocs { get; set; }

    }

}
