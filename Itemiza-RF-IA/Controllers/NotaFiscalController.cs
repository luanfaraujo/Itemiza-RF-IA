using Itemiza_RF_IA.Data;
using Itemiza_RF_IA.Models;
using Itemiza_RF_IA.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Itemiza_RF_IA.Controllers;

public class NotaFiscalController : Controller
{
    private readonly GeminiService _geminiService;

    private readonly ItemizaDbContext _context;

    public NotaFiscalController(
        GeminiService geminiService,
        ItemizaDbContext context)
    {
        _geminiService = geminiService;
        _context = context;
    }

    [HttpGet]
    public IActionResult TestarImagemFormulario()
    {
        return View("TestarImagem");
    }

    [HttpPost]
    public async Task<IActionResult> TestarImagem(IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("Selecione uma imagem.");

        if (arquivo.Length > 10 * 1024 * 1024)
            return BadRequest("A imagem deve ter no máximo 10 MB.");

        var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/webp" };

        if (!tiposPermitidos.Contains(arquivo.ContentType))
            return BadRequest("Envie uma imagem JPEG, PNG ou WebP.");

        await using var stream = new MemoryStream();

        await arquivo.CopyToAsync(stream);


        var resposta = await _geminiService.AnalisarImagemAsync(
            stream.ToArray(),
            arquivo.ContentType
        );

        var compra = new Compra
        {
            Estabelecimento = resposta.Estabelecimento!,
            DataCompra = resposta.DataCompra,
            Produtos = resposta.Produtos.Select(p => new Produto
            {
                NomeNaNota = p.NomeNaNota!,
                NomePersonalizado = p.NomePersonalizado,
                ValorUnitario = p.ValorUnitario,
                Quantidade = p.Quantidade,
                Unidade = p.Unidade,
                ValorTotal = p.ValorTotal,
                Categoria = p.Categoria!
            }).ToList()
        };

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();

        return Json(new
        {
            mensagem = "Compra salva com sucesso!",
            compraId = compra.Id,
            estabelecimento = compra.Estabelecimento,
            dataCompra = compra.DataCompra,
            quantidadeProdutos = compra.Produtos.Count
        });
    }
}
