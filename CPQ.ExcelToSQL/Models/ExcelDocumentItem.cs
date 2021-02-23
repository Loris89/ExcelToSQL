using System;

namespace CPQ.ExcelToSQL.Models
{
    //TBL_Stub_AcquisitionDocumentItem
    public class ExcelDocumentItem
    {
        public Guid? Id { get; set; }
        public string DocumentId { get; set; }
        public bool Mandatory { get; set; }
        public bool IsProduct { get; set; }
        public string Description { get; set; }

        //Metadati
    }
}
