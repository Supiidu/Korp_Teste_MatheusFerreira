using ServiceFaturamento.Enums;

namespace ServiceFaturamento.Models;

public class Nota
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public DateTime DataEmissao { get; set; }
    public StatusNota Status { get; set; } //aberta ou fechada

    public List<ItemNota> Itens { get; set; }
}
