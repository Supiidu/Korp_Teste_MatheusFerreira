using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceEstoque.DTOS
{
    public class BaixaEstoqueDto
    {
        public string Codigo { get; set; }
        public int Quantidade { get; set; }
    }
}