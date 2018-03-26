using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Transactiontester
{
    class Program
    {
        static void Main(string[] args)
        {

            CoreTransaction trans = new CoreTransaction();
            PersonHandler handler = new PersonHandler(trans);


            try
            {
                trans.Begin();
                EMDPerson perso = (EMDPerson)handler.GetPersonByP_Id(5);

                handler.UpdateObject(perso);

                perso.Language = "changed";
                Console.WriteLine("Blocking Transaction");

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                trans.Rollback();
            }
            
            Console.WriteLine("Transaction Rollback");
        }
    }
}
