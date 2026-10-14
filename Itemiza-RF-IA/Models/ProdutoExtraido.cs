
namespace Itemiza_RF_IA.Models;

public class ProdutoExtraido
{
    public string? NomeNaNota { get; set; }

    public string NomePersonalizado { get; set; } = "";

    public decimal? ValorUnitario { get; set; }

    public decimal? Quantidade { get; set; }

    public string? Unidade { get; set; }

    public decimal? ValorTotal { get; set; }

    public string? Categoria { get; set; }
}
