using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CPQ.ExcelToSQL.Models
{
    //TBL_Stub_MapAcquisitionDocumentItemFilter
    //TBL_Stub_MapAcquisitionDocumentItemSubstituteFilter
    public class ExcelMappingDocumentItem
    {
        // ID Doc   O/F Sostituto
        public string DocumentId { get; set; }
        public string Mandatory { get; set; }
        public string SubstituteId { get; set; }

        public ExcelMappingDocumentItem()
        {

        }
        public ExcelMappingDocumentItem(string inputMappingDoc)
        {
            //non rimuovere campi vuoti
            //21,O,22 - DocumentId,Mandatory,SubstituteId
            var inputValues = inputMappingDoc.Split(',', StringSplitOptions.None);
            if (inputValues.Length != 3)
                throw new Exception("excel file not valid");

            DocumentId = inputValues[0]?.Trim();
            Mandatory = inputValues[1]?.Trim();
            SubstituteId = inputValues[2]?.Trim();

            if (string.IsNullOrEmpty(DocumentId) || string.IsNullOrEmpty(Mandatory))
                throw new Exception("excel file not valid");

        }
    }
}
