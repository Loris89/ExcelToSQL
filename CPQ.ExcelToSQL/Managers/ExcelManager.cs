using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CPQ.ExcelToSQL.Models;
using System.Data.SqlClient;

namespace CPQ.ExcelToSQL.Managers
{
    public class ExcelManager : IExcelManager
    {
        public ExcelManager() {}

        #region Import Excel

        // Sheet name excel
        private const string _SheetMapping = "Mapping _INPUT_OUTPUT";

        // Column name excel
        private const string _COD_AUC = "COD_AUC";
        private const string _CIAE = "CIAE";
        private const string _SAE = "SAE";
        private const string _ID_DOC_LIST = "idDocList";
        private const string _MATRIX_DOCS = "Regola_documento";

        public async Task<List<string>> ImportExcelCPQDocs(Stream stream)
        {
            // PER ANTONIO T.

            // Non ho fatto molti progressi oggi ma ti riassumo quanto ho fatto:
            // 1) In questo progetto ho creato una cartella SQL in cui inseriremo gli script
            // 2) Il codice che vedi sotto l'ho fatto per testare la connessione a SQL Express locale
            // 3) In "temp_sp.sql" trovi una bozza di stored procedure in cui ho fatto degli esperimenti da cui puoi riprendere le attività.
            // 4) In "final_query.sql" ci sarà tutto il necessario da rilasciare al cliente:
            // -- Aggiunta colonna e suo popolamento
            // -- Modifica stored procedures
            // -- Statements generati da questo tool

            // Mi sono concentrato su 3 aspetti:
            // 1- Passaggio delle tuple idDoc,Mandatory,Sostituto in input alla stored
            // 2- Mandare in output quello che prima facevi in PRINT (che per la scrittura su file non ci è utile)
            // 3- Scrittura su file di ciò che manda in output la stored p.

            // Per il punto 1 vedi sotto come ho fatto, poi lo adegui ai modelli che hai già creato.

            // Per il punto 2 l'unico modo che ho trovato è fare una serie di insert (laddove ora ci sono le print) dentro una tabella temporanea (#tmp_result)
            // su cui verrà eseguita una select finale che di conseguenza manderà in output tutte le righe (ogni riga sarebbe un pezzo dello script da eseguire).
            // Questo però significa che NON devono essere presenti delle "select" nel mezzo che andrebbero a sfalsare l'output.
            // Ovviamente tu hai messo delle select nel mezzo, ma finché scrivono in tabelle temporanee non mandano in output il loro risultato.

            // Comunque in linea di massima come procedura generale farei:
            // - Ricostruzione db di sviluppo in LOCALE
            // - Installazione della stored procedure in LOCALE che ci consente di produrre il file .sql
            // - Esecuzione di questo tool
            // Così facendo non sporchiamo i veri ambienti con la nostra stored procedure

            // Test SQL connection
            using (var conn = new SqlConnection(@"data source=localhost\SQLEXPRESS;initial catalog=DBFEUNICO;Integrated Security=SSPI;"))
            using (var command = new SqlCommand("test", conn) {  CommandType = CommandType.StoredProcedure })
            {
                conn.Open();

                #region Passaggio dei documenti con obbligatorietà e sostituti alla SP
                var docs = new List<string>() { "20,O,21", "11,F" };

                var table = new DataTable();
                table.Columns.Add("IdDoc", typeof(string));
                table.Columns.Add("Mandatory", typeof(bool));
                table.Columns.Add("SubstituteId", typeof(string));

                foreach (string s in docs)
                {
                    string[] parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    DataRow row = table.NewRow();
                    row[0] = parts[0];
                    row[1] = parts[1] == "O";
                    if (parts.Length == 3)
                        row[2] = parts[2];

                    table.Rows.Add(row);
                }

                var docsParameter = new SqlParameter("@docs", SqlDbType.Structured)
                {
                    TypeName = "dbo.Docs",
                    Value = table
                };

                command.Parameters.Add(docsParameter);
                #endregion

                // esecuzione stored procedure e scrittura su file
                using SqlDataReader rdr = command.ExecuteReader();
                // itera il risultato e scrivi su file
                while (rdr.Read())
                {
                    // Stampa di prova per testare il passaggio di parametri complessi.
                    // Da sostituire con la stampa singola di ogni query generata dalla sp (SELECT * FROM #tmp_result come ultimo statement nella stored)
                    // Esempio: File.AppendAllText(@"query.sql", $"{rdr["query"]}{Environment.NewLine}");
                    File.AppendAllText(@"query.sql", $"{rdr["IdDoc"]},{rdr["Mandatory"]},{rdr["SubstituteId"]}{Environment.NewLine}");
                }
            }

            { }

            List<String> messages = new List<String>();
            try
            {
                // if (string.IsNullOrEmpty(_user)) return null;

                /*
                 ID_DOC	
                 ID_AUC	
                 * COD_AUC	
                 * CIAE	
                 * SAE	
                 KEY	
                 DocumentName	
                 * ID Doc 	O/F	Sostituto	
                 Meta dati:
                   Numero_documento	Data_Rilascio	Luogo_Rilascio	Provincia	Ente_Rilascio

                Per inserire un nuovo documento occorre sapere: 
                     [DocumentId]
                    ,[Mandatory]
                    ,[IsProduct]
                    ,[Description]
                    + Metadati
                */
                //Lists of entities
                //var profiles = await _registriesRepository.GetProfiles().ConfigureAwait(false);
                //var workInitiatives = await _workInitiativesRepository.GetExportExcelWorkInitiative(_user).ConfigureAwait(false);
                //var divisions = await _divisionRepository.GetAll().ToListAsync().ConfigureAwait(false);
                //var subProcess = await _subProcessRepository.GetAll().ToListAsync().ConfigureAwait(false);
                //var disciplines = await _disciplineRepository.GetAll().ToListAsync().ConfigureAwait(false);
                //var process = await _processRepository.GetAll().ToListAsync().ConfigureAwait(false);

                //var companies = await _companyRepository.GetAll().ToListAsync().ConfigureAwait(false);
                //var departments = await _departmentRepository.GetAll().ToListAsync().ConfigureAwait(false);
                //var professionalRoles = await _professionalRoleRepository.GetAll().ToListAsync().ConfigureAwait(false);

                //var listUsersProfiles = new List<UserProfile>();
                //var listUsers = new List<User>();
                //var listDeputies = new Dictionary<User, string>();

                var listSheet = new List<string>() { _SheetMapping };
                var listDocAcquisitionFilter = new List<ExcelDocumentAcquisitionFilter>();

                XSSFWorkbook workbook = new XSSFWorkbook(stream);

                foreach (var sheetName in listSheet)
                {
                    DataTable table = new DataTable();
                    ISheet sheet = workbook.GetSheet(sheetName);
                    if (sheet == null)
                    {
                        messages.Add($"Error occured, check excel file: Sheet ({sheetName}) not found");
                        continue;
                    }

                    // Creazione colonne
                    IRow headerRow = sheet.GetRow(1);
                    int cellCount = headerRow.LastCellNum;
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                        table.Columns.Add(column);
                    }

                    // Lettura delle righe
                    int rowCount = sheet.LastRowNum;
                    for (int i = 2; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        DataRow dataRow = table.NewRow();

                        if (row != null)
                        {
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                var cell = GetFirstCellInMergedRegionContainingCell(row.GetCell(j));
                                if (cell != null)
                                {
                                    dataRow[j] = cell.ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }
                    }

                    if (table.Rows.Count > 0)
                    {
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];

                            var docAcquisitionFilter = IsValidExcelRow(row, i + 3, sheetName, listDocAcquisitionFilter, out List<string> currentMessage);

                            if (currentMessage.Any())
                                messages.AddRange(currentMessage);

                            if (docAcquisitionFilter == null)
                                continue; // validazione fallita -> passo al prossimo record

                            listDocAcquisitionFilter.Add(docAcquisitionFilter);
                        }
                    }
                }

                // Se tutto il documento excel è valido eseguo l'import
                if (!messages.Any())
                {
                    //loop create new users
                    //foreach (var user in listUsers)
                    //{
                    //    try
                    //    {
                    //        await _usersRepository.CreateUser(user, _user, true).ConfigureAwait(false);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        _logger.LogError(ex, System.Reflection.MethodBase.GetCurrentMethod()?.ReflectedType?.FullName);
                    //        messages.Add("501, An exception occurred while creating the user: " + user.Email);
                    //    }
                    //}

                    ////import user profiles
                    //var outMess = await _usersRepository.ImportExcelUsersProfiles(listUsersProfiles, _user).ConfigureAwait(false);
                    //if (outMess.Any())
                    //{
                    //    messages.AddRange(outMess);
                    //}

                    if (!messages.Any())
                    {
                        messages.Add("Success");
                    }
                }

                return messages;
            }
            catch (Exception ex)
            {
                messages.Add(ex.Message);
                return messages;
            }
        }

        private ExcelDocumentAcquisitionFilter IsValidExcelRow(
            DataRow r,
            int numRow,
            string sheetName,
            List<ExcelDocumentAcquisitionFilter> prevDocs,
            out List<string> message)
        {
            message = new List<string>();
            if (r == null || r.ItemArray.All(value => value == null || value is DBNull || string.IsNullOrWhiteSpace(value.ToString()))) return null;

            var item = new ExcelDocumentAcquisitionFilter();

            string COD_AUC = r[_COD_AUC]?.ToString().Trim();
            if (string.IsNullOrEmpty(COD_AUC))
            {
                message.Add($"An error occurred: Mandatory field missing or not valid, sheet {sheetName} row {numRow} column {_COD_AUC}");
            }
            else
            {
                item.COD_AUC = COD_AUC;
            }

            string CIAE = r[_CIAE]?.ToString().Trim();
            if (string.IsNullOrEmpty(CIAE))
            {
                message.Add($"An error occurred: Mandatory field missing or not valid, sheet {sheetName} row {numRow} column {_CIAE}");
            }
            else
            {
                item.CIAE = CIAE;
            }

            string SAE = r[_SAE]?.ToString().Trim();
            if (string.IsNullOrEmpty(SAE))
            {
                message.Add($"An error occurred: Mandatory field missing or not valid, sheet {sheetName} row {numRow} column {_SAE}");
            }
            else
            {
                item.SAE = SAE;
            }

            string ID_DOC_LIST = r[_ID_DOC_LIST]?.ToString().Trim();
            if (string.IsNullOrEmpty(ID_DOC_LIST))
            {
                message.Add($"An error occurred: Mandatory field missing or not valid, sheet {sheetName} row {numRow} column {_ID_DOC_LIST}");
            }
            else
            {
                item.ID_DOC_LIST = ID_DOC_LIST;
            }

            string inputMappingDocs = r[_MATRIX_DOCS]?.ToString().Trim();
            if (string.IsNullOrEmpty(inputMappingDocs))
            {
                message.Add($"An error occurred: Mandatory field missing or not valid, sheet {sheetName} row {numRow} column {_MATRIX_DOCS}");
            }
            else
            {
                item.InputMappingDocs = inputMappingDocs;
                try
                {
                    item.MappingDocs = ConvertMappingDocs(inputMappingDocs);
                }
                catch
                {
                    message.Add($"An error occurred: Mandatory field not valid, sheet {sheetName} row {numRow} column {_MATRIX_DOCS}");
                }
            }

            //controllo se nell'excel ci sono 2 tuple uguali
            if (prevDocs.Any(x => x.COD_AUC == COD_AUC && x.CIAE == CIAE && x.SAE == SAE && x.ID_DOC_LIST == ID_DOC_LIST))
            {
                message.Add($"An error occurred: Duplicated values, sheet {sheetName} row {numRow}");
            }

            if (message.Any())
            {
                return null;
            }

            return item;
        }

        private List<ExcelMappingDocumentItem> ConvertMappingDocs(string inputMappingDocs)
        {
            //'20,O,;21,O,22;23,F,;
            var result = inputMappingDocs.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new ExcelMappingDocumentItem(s)).ToList();

            return result;
        }

        public static ICell GetFirstCellInMergedRegionContainingCell(ICell cell)
        {
            if (cell != null && cell.IsMergedCell)
            {
                ISheet sheet = cell.Sheet;
                for (int i = 0; i < sheet.NumMergedRegions; i++)
                {
                    CellRangeAddress region = sheet.GetMergedRegion(i);
                    
                    if (region.IsInRange(cell.RowIndex, cell.ColumnIndex))
                    {
                        IRow row = sheet.GetRow(region.FirstRow);
                        ICell firstCell = row?.GetCell(region.FirstColumn);
                        return firstCell;
                    }
                }
                return null;
            }
            return cell;
        }

        #endregion
    }
}
