using Microsoft.WindowsAzure.Storage.Table;
using System;
namespace marking.Functions.Entities
{
    public class MarkingEntity : TableEntity
    {
        public int IdEmpleo { get; set; }
        public DateTime DateTimeInOrOut { get; set; }
        public int Tipo { get; set; }
        public bool consolidated { get; set; }
    }
}
