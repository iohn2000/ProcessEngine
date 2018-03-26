using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.EDPExports.Entities;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.EDPExports.Entities
{
    public class EDPDataForITHandler : Kapsch.IS.Util.Sql.SqlData.BaseDataAccess
    {
        public EDPDataForITHandler(String connectionStringName) : base(connectionStringName)
        {

        }

        public EDPDataForIT CreateItem(EDPDataForIT item)
        {
            try
            {
                String queryString = "INSERT INTO [dbo].[EDPDataForIT] ([Status],[UserID],[UserType],[UserStatus],[FirstName],[FamilyName],[DisplayName],[ObjID],[CompanyShortName],[EmploymentTypeID],[Direct],[Mobile],[Phone],[EFax],[Room],[PersonalNr],[PersonID],[EmploymentID],[Gender],[created]) VALUES (@Status,@UserID,@UserType,@UserStatus,@FirstName,@FamilyName,@DisplayName,@ObjID,@CompanyShortName,@EmploymentTypeID,@Direct,@Mobile,@Phone,@EFax,@Room,@PersonalNr,@PersonID,@EmploymentID,@Gender,@created)";
                sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = queryString;
                sqlCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 25).Value = item.Status;
                sqlCommand.Parameters.Add("@UserID", SqlDbType.NVarChar, 50).Value = item.UserID;
                sqlCommand.Parameters.Add("@UserType", SqlDbType.NVarChar, 50).Value = item.UserType;
                sqlCommand.Parameters.Add("@UserStatus", SqlDbType.NVarChar, 50).Value = item.UserStatus;
                sqlCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = item.FirstName;
                sqlCommand.Parameters.Add("@FamilyName", SqlDbType.NVarChar, 255).Value = item.FamilyName;
                sqlCommand.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 510).Value = item.DisplayName;
                sqlCommand.Parameters.Add("@ObjID", SqlDbType.Int).Value = item.ObjID;
                sqlCommand.Parameters.Add("@CompanyShortName", SqlDbType.NVarChar, 255).Value = item.CompanyShortName;
                sqlCommand.Parameters.Add("@EmploymentTypeID", SqlDbType.Int).Value = item.EmploymentTypeID;
                sqlCommand.Parameters.Add("@Direct", SqlDbType.NVarChar, 255).Value = item.Direct;
                sqlCommand.Parameters.Add("@Mobile", SqlDbType.NVarChar, 255).Value = item.Mobile;
                sqlCommand.Parameters.Add("@Phone", SqlDbType.NVarChar, 255).Value = item.Phone;
                sqlCommand.Parameters.Add("@EFax", SqlDbType.NVarChar, 255).Value = item.EFax;
                sqlCommand.Parameters.Add("@Room", SqlDbType.NVarChar, 255).Value = item.Room;
                sqlCommand.Parameters.Add("@PersonalNr", SqlDbType.Char, 10).Value = item.PersonalNr;
                sqlCommand.Parameters.Add("@PersonID", SqlDbType.Int).Value = item.PersonID;
                sqlCommand.Parameters.Add("@EmploymentID", SqlDbType.Int).Value = item.EmploymentID;
                sqlCommand.Parameters.Add("@Gender", SqlDbType.Char, 1).Value = item.Gender;
                sqlCommand.Parameters.Add("@created", SqlDbType.DateTime2).Value = item.created;

                sqlConnection.Open();
                int rowsEffected = sqlCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "Could not insert new row in table EDPDataForIT");
            }
            if (sqlConnection != null)
            {
                sqlConnection.Close();
            }
            return item;
        }

        public EDPDataForIT UpdateItem(EDPDataForIT item)
        {
            try
            {
                String queryString = "UPDATE [dbo].[EDPDataForIT] SET [Status] = @Status, [UserType]=@UserType,[UserStatus]=@UserStatus,[FirstName]=@FirstName,[FamilyName]=@FamilyName,[DisplayName]=@DisplayName,[ObjID]=@ObjID,[CompanyShortName]=@CompanyShortName,[EmploymentTypeID]=@EmploymentTypeID,[Direct]=@Direct,[Mobile]=@Mobile,[Phone]=@Phone,[EFax]=@EFax,[Room]=@Room,[PersonalNr]=@PersonalNr,[PersonID]=@PersonID,[EmploymentID]=@EmploymentID,[Gender]=@Gender,[created]=@created WHERE [UserID]=@UserID";
                sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = queryString;
                sqlCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 25).Value = item.Status;
                sqlCommand.Parameters.Add("@UserType", SqlDbType.NVarChar, 50).Value = item.UserType;
                sqlCommand.Parameters.Add("@UserStatus", SqlDbType.NVarChar, 50).Value = item.UserStatus;
                sqlCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = item.FirstName;
                sqlCommand.Parameters.Add("@FamilyName", SqlDbType.NVarChar, 255).Value = item.FamilyName;
                sqlCommand.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 510).Value = item.DisplayName;
                sqlCommand.Parameters.Add("@ObjID", SqlDbType.Int).Value = item.ObjID;
                sqlCommand.Parameters.Add("@CompanyShortName", SqlDbType.NVarChar, 255).Value = item.CompanyShortName;
                sqlCommand.Parameters.Add("@EmploymentTypeID", SqlDbType.Int).Value = item.EmploymentTypeID;
                sqlCommand.Parameters.Add("@Direct", SqlDbType.NVarChar, 255).Value = item.Direct;
                sqlCommand.Parameters.Add("@Mobile", SqlDbType.NVarChar, 255).Value = item.Mobile;
                sqlCommand.Parameters.Add("@Phone", SqlDbType.NVarChar, 255).Value = item.Phone;
                sqlCommand.Parameters.Add("@EFax", SqlDbType.NVarChar, 255).Value = item.EFax;
                sqlCommand.Parameters.Add("@Room", SqlDbType.NVarChar, 255).Value = item.Room;
                sqlCommand.Parameters.Add("@PersonalNr", SqlDbType.Char, 10).Value = item.PersonalNr;
                sqlCommand.Parameters.Add("@PersonID", SqlDbType.Int).Value = item.PersonID;
                sqlCommand.Parameters.Add("@EmploymentID", SqlDbType.Int).Value = item.EmploymentID;
                sqlCommand.Parameters.Add("@Gender", SqlDbType.Char, 1).Value = item.Gender;
                sqlCommand.Parameters.Add("@created", SqlDbType.DateTime2).Value = item.created;
                sqlCommand.Parameters.Add("@UserID", SqlDbType.NVarChar, 50).Value = item.UserID;

                sqlConnection.Open();
                int rowsEffected = sqlCommand.ExecuteNonQuery();
                if (rowsEffected == 0)
                {
                    throw new BaseException(ErrorCodeHandler.E_DB_ITEM_NOT_FOUND, string.Format("An item with the UserID {0} was not found!", item.UserID));
                }
            }
            catch (Exception ex)
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "Could not insert new row in table EDPDataForIT");
            }
            if (sqlConnection != null)
            {
                sqlConnection.Close();
            }
            return item;
        }

        public EDPDataForIT CreateOrUpdateItem(EDPDataForIT item)
        {
            if (ItemExists(item.UserID))
            {
                UpdateItem(item);
            }
            else
            {
                CreateItem(item);
            }
            return item;
        }


        public bool ItemExists(string UserId)
        {
            bool returnVal = false;
            if (!string.IsNullOrWhiteSpace(UserId))
            {
                String queryString = "SELECT count(*) FROM EDPDataForIT where UserID = @UserID";

                try
                {

                    sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = queryString;

                    sqlCommand.Parameters.Add(new SqlParameter("@UserID", UserId));
                    sqlConnection.Open();

                    //                    int rowsEffected = sqlCommand.ExecuteNonQuery();
                    int rowsEffected = (int)sqlCommand.ExecuteScalar();

                    if (rowsEffected > 0)
                        returnVal = true;
                    else
                        returnVal = false;
                }
                catch (Exception ex)
                {
                    if (sqlConnection != null)
                    {
                        sqlConnection.Close();
                    }
                    throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
                }

                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The provided UserID is null or empty!");
            }

            return returnVal;
        }


    }
}
