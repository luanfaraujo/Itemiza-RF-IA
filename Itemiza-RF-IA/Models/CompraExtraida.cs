namespace Itemiza_RF_IA.Models;

public class CompraExtraida
{
    public string? Estabelecimento { get; set; }

    public DateOnly? DataCompra { get; set; }

    public string? Erro { get; set; }

    public List<ProdutoExtraido> Produtos { get; set; } = new();
}