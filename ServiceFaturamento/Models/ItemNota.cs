using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceFaturamento.Models
{
    public class ItemNota
    {
        public int Id { get; set; }

        public int NotaFiscalId { get; set; }

        [JsonIgnore]
        public Nota Nota { get; set; }

        public string CodigoProduto { get; set; }
        public int Quantidade { get; set; }
    }
}