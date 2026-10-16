
namespace Itemiza_RF_IA.Models;

public class Compra
{
    public int Id { get; set; }

    public string Estabelecimento { get; set; } = "";

    public DateOnly? DataCompra { get; set; }

    public List<Produto> Produtos { get; set; } = new();
}