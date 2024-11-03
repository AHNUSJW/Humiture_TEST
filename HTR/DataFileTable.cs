using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTR
{
    public class DataFileTable
    {
        public DateTime dateTime;
        public string Name;

        public DataFileTable(DateTime dt, string name)
        {
            dateTime = dt;
            Name = name;
        }
    }
}
