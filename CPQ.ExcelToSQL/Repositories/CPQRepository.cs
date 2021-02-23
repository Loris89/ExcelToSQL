
namespace CPQ.ExcelToSQL.Repositories
{
    public class CPQRepository : ICPQRepository
    {
        public readonly IConnectionsString Connections;

        public CPQRepository(IConnectionsString connections)
        {
            Connections = connections;
        }

        /*
        public async Task UpdateTimesheet(int idStepModel, DateTime date, int idUser, List<Timesheet> timesheet)
        {
            try
            {
                //todo fare validazione per bene
                if (timesheet == null) return;

                int idStepPlan = 0;
                int idStructureView = 0;

                using (SqlConnection conn = new SqlConnection(Connections.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("WSA_GetParamForTimesheet", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id_Step", SqlDbType.Int).Value = idStepModel;
                        cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = date;

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            while (await reader.ReadAsync())
                            {
                                idStepPlan = Convert.ToInt32(reader["Id_Step"]);
                                idStructureView = Convert.ToInt32(reader["Id_StructuresView"]);
                            }
                        }
                        await conn.CloseAsync();
                    }
                }

                using (SqlConnection conn = new SqlConnection(Connections.GetConnectionString()))
                {
                    await conn.OpenAsync();
                    var transaction = conn.BeginTransaction();

                    try
                    {
                        foreach (var row in timesheet)
                        {
                            using (SqlCommand cmd = new SqlCommand("WSA_UpdateTimesheetRow", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@IdUser", SqlDbType.Int).Value = idUser;
                                cmd.Parameters.Add("@IdStep", SqlDbType.Int).Value = idStepPlan;
                                cmd.Parameters.Add("@IdStr", SqlDbType.Int).Value = row.IdStr;
                                cmd.Parameters.Add("@EntrataAM", SqlDbType.VarChar).Value = row.EntrataAM;
                                cmd.Parameters.Add("@UscitaAM", SqlDbType.VarChar).Value = row.UscitaAM;
                                cmd.Parameters.Add("@EntrataPM", SqlDbType.VarChar).Value = row.EntrataPM;
                                cmd.Parameters.Add("@UscitaPM", SqlDbType.VarChar).Value = row.UscitaPM;
                                cmd.Parameters.Add("@Note", SqlDbType.VarChar).Value = row.Note;
                                cmd.Parameters.Add("@IdStructureView", SqlDbType.Int).Value = idStructureView;

                                cmd.Parameters.Add("@OrePermesso", SqlDbType.VarChar).Value = "";
                                cmd.Parameters.Add("@Malattia", SqlDbType.VarChar).Value = "";
                                cmd.Parameters.Add("@CodiceMalattia", SqlDbType.VarChar).Value = "";
                                cmd.Parameters.Add("@Ferie", SqlDbType.VarChar).Value = "";

                                cmd.ExecuteNonQuery();

                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        await conn.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task InsertInvoiceAruba(List<InvoiceAruba> invoicesAruba)
        {
            try
            {
                if (invoicesAruba == null || !invoicesAruba.Any()) return;

                //mapping InvoiceAruba to Invoice for json.stringfy the object
                var invoices = _mapper.Map<List<Invoice>>(invoicesAruba);

                using (SqlConnection conn = new SqlConnection(Connections.GetConnectionString()))
                {
                    await conn.OpenAsync();
                    var transaction = conn.BeginTransaction();

                    try
                    {
                        foreach (var i in invoices)
                        {
                            using (SqlCommand cmd = new SqlCommand("WSI_Invoices_Insert", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = i.Id;
                                cmd.Parameters.Add("@Filename", SqlDbType.NVarChar).Value = i.Filename;
                                cmd.Parameters.Add("@Invoices", SqlDbType.NVarChar).Value = i.Invoices;
                                cmd.Parameters.Add("@Sender", SqlDbType.NVarChar).Value = i.Sender;
                                cmd.Parameters.Add("@Receiver", SqlDbType.NVarChar).Value = i.Receiver;
                                cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime2).Value = i.CreationDate;
                                cmd.Parameters.Add("@LastUpdate", SqlDbType.DateTime2).Value = i.LastUpdate;
                                cmd.Parameters.Add("@IdSdi", SqlDbType.NVarChar).Value = i.IdSdi;
                                cmd.Parameters.Add("@Signed", SqlDbType.Bit).Value = i.Signed;
                                cmd.Parameters.Add("@PddAvailable", SqlDbType.Bit).Value = i.PddAvailable;
                                cmd.Parameters.Add("@InvoiceType", SqlDbType.NVarChar).Value = i.InvoiceType;
                                cmd.Parameters.Add("@DocType", SqlDbType.NVarChar).Value = i.DocType;
                                //IdFile solo in update,
                                //CreatedOn e ImportedOn direttamente dal db

                                cmd.ExecuteNonQuery();

                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        await conn.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception {0}", System.Reflection.MethodBase.GetCurrentMethod()?.ReflectedType?.FullName);
                throw ex;
            }
        }

        public async Task<int?> InsertFile(int idUser, string fileName, string path, string fileLength, string mimeType)
        {
            try
            {
                int? idFile = null;

                using (SqlConnection conn = new SqlConnection(Connections.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("InsertFile", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdUser", SqlDbType.Int).Value = idUser;
                        cmd.Parameters.Add("@IdRole", SqlDbType.Int).Value = _idRole;
                        cmd.Parameters.Add("@IdApplication", SqlDbType.Int).Value = _idApplication;
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = fileName;
                        cmd.Parameters.Add("@length", SqlDbType.Int).Value = fileLength;
                        cmd.Parameters.Add("@mimeType", SqlDbType.VarChar).Value = mimeType;
                        cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = path;
                        SqlParameter outputIdParam = new SqlParameter("@id_File", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputIdParam);

                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        idFile = (int)outputIdParam.Value;
                        await conn.CloseAsync();
                    }
                }

                return idFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception {0}", System.Reflection.MethodBase.GetCurrentMethod()?.ReflectedType?.FullName);
                throw ex;
            }
        }

        */
    }
}
