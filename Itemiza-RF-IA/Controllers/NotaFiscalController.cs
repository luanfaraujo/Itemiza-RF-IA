using Itemiza_RF_IA.Services;
using Microsoft.AspNetCore.Mvc;

namespace Itemiza_RF_IA.Controllers;

public class NotaFiscalController : Controller
{
    private readonly GeminiService _geminiService;

    public NotaFiscalController(GeminiService geminiService)
    {
        _geminiService = geminiService;
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

        return Json(resposta);
    }
}
