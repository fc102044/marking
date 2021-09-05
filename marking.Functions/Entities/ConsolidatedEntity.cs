using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace marking.Functions.Entities
{
    public class ConsolidatedEntity : TableEntity
    {
        public int IdWork { get; set; }
        public DateTime DateConsolidate { get; set; }
        public double MinutsWorked { get; set; }
    }
}
