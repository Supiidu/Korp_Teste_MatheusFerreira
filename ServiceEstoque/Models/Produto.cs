namespace ServiceEstoque.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public int Saldo { get; set; }
    }
}