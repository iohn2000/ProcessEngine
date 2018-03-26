using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    public sealed class TransactionHandler
    {
        private static volatile TransactionHandler instance;
        private static object syncRoot = new Object();

        private TransactionHandler() {

        }

        public static TransactionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TransactionHandler();
                    }
                }

                return instance;
            }
        }

        public CoreTransaction CreateTransaction() {
            //return new CoreTransaction();
            CoreTransaction ct = new CoreTransaction();
            //ct.Begin();
            return ct;
        }
    }

}



