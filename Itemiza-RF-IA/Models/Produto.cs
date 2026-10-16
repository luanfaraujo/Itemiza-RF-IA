
namespace Itemiza_RF_IA.Models;

public class Produto
{
    public int Id { get; set; }

    public string NomeNaNota { get; set; } = "";

    public string NomePersonalizado { get; set; } = "";

    public decimal? ValorUnitario { get; set; }

    public decimal? Quantidade { get; set; }

    public string? Unidade { get; set; }

    public decimal? ValorTotal { get; set; }

    public string Categoria { get; set; } = "";

    public int CompraId { get; set; }

    public Compra Compra { get; set; } = null!;
}