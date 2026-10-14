using System.Text.Json;
using Itemiza_RF_IA.Models;
using Google.GenAI;

// Alterar o namespace depois - quando juntar tudo, "Itemiza" vai ser melhor
namespace Itemiza_RF_IA.Services;

public class GeminiService
{
    private static string? ValidarCompra(CompraExtraida compra)
    {
        if (!string.IsNullOrWhiteSpace(compra.Erro))
        {
            return compra.Erro;
        }

        if (string.IsNullOrWhiteSpace(compra.Estabelecimento))
        {
            return "Não foi possível identificar o estabelecimento.";
        }

        if (compra.Produtos.Count == 0)
        {
            return "Nenhum produto foi identificado na nota.";
        }

        string[] categoriasPermitidas =
        [
            "Alimentação",
        "Higiene pessoal",
        "Limpeza doméstica",
        "Utilidades domésticas",
        "Saúde e medicamentos",
        "Vestuário",
        "Papelaria",
        "Outros"
        ];

        foreach (var produto in compra.Produtos)
        {
            if (string.IsNullOrWhiteSpace(produto.NomeNaNota))
            {
                return "Um dos produtos não possui nome identificado.";
            }

            if (produto.Categoria is null ||
                !categoriasPermitidas.Contains(produto.Categoria))
            {
                return $"Categoria inválida para o produto: {produto.NomeNaNota}.";
            }
        }

        return null;
    }

    private readonly Client _client;

    public GeminiService()
    {
        _client = new Client();
    }

    public async Task<string> TestarConexaoAsync()
    {
        var response = await _client.Models.GenerateContentAsync(
            model: "gemini-flash-latest",
            contents: "Responda em português: diga olá e confirme que a conexão está funcionando."
        );

        return response.Candidates[0].Content.Parts[0].Text;
    }


    public async Task<CompraExtraida> AnalisarImagemAsync(
        byte[] imagem,
        string tipoMime)
    {
        var response = await _client.Models.GenerateContentAsync(
            model: "gemini-3.5-flash-lite",
            contents: new Google.GenAI.Types.Content
            {
                Parts =
                [
                    new Google.GenAI.Types.Part
                {
                    InlineData = new Google.GenAI.Types.Blob
                    {
                        MimeType = tipoMime,
                        Data = imagem
                    }
                },
                new Google.GenAI.Types.Part
                {
                    Text = """
                        Analise a imagem enviada e extraia informações de uma nota fiscal, cupom fiscal ou comprovante de compra.

                        Se a imagem não contiver um documento desse tipo, informe isso claramente usando o campo "erro".

                        Retorne exclusivamente um objeto JSON válido, sem blocos Markdown, comentários ou texto adicional, seguindo exatamente esta estrutura:

                        {
                        "estabelecimento": "nome do estabelecimento ou null",
                        "dataCompra": "data no formato AAAA-MM-DD ou null",
                        "erro": null,
                        "produtos": [
                        {
                        "nomeNaNota": "nome exatamente como impresso",
                        "nomePersonalizado": "",
                        "valorUnitario": 0.00,
                        "quantidade": 0.00,
                        "unidade": "unidade de medida ou null",
                        "valorTotal": 0.00,
                        "categoria": "categoria permitida"
                        }
                        ]
                        }

                        Regras para preencher os campos:

                        "estabelecimento": nome exatamente como aparece no documento. Use null se não for possível identificá-lo.

                        "dataCompra": data da compra no formato AAAA-MM-DD. Converta a data impressa para esse formato. Use null se a data estiver ausente ou ilegível.

                        "erro": use null se a imagem contiver um documento válido para análise. Caso contrário, descreva brevemente o problema e retorne uma lista vazia em "produtos".

                        "produtos": inclua cada produto identificável individualmente. Não inclua itens cuja existência não possa ser confirmada.

                        "nomeNaNota": preserve o nome ou a descrição exatamente como aparece no documento, incluindo abreviações.

                        "nomePersonalizado": sempre retorne uma string vazia (""). Esse campo será preenchido pelo usuário posteriormente.

                        "valorUnitario": preço por unidade ou por quilograma, conforme indicado no documento. Use null quando não puder identificá-lo com segurança.

                        "quantidade": quantidade comprada ou peso adquirido, conforme indicado no documento. Use null quando não puder identificá-lo com segurança.

                        "unidade": unidade de medida correspondente, como "UN", "PT", "kg", "g", "L" ou "mL", quando identificável. Preserve a sigla impressa quando aplicável. Use null se não for possível identificá-la.

                        "valorTotal": valor total efetivamente cobrado pelo produto, conforme impresso no documento. Use null quando não puder identificá-lo com segurança.

                        "categoria": escolha exatamente uma destas categorias:

                        "Alimentação"

                        "Higiene pessoal"

                        "Limpeza doméstica"

                        "Utilidades domésticas"

                        "Saúde e medicamentos"

                        "Vestuário"

                        "Papelaria"

                        "Outros"

                        Regras adicionais:

                        Não invente nem estime informações ausentes ou ilegíveis.

                        Para campos numéricos ausentes, use null, nunca zero como substituto.

                        Valores numéricos devem ser números JSON, sem símbolo de moeda e com ponto como separador decimal.

                        Não calcule "valorTotal" multiplicando preço unitário pela quantidade. Prefira o total impresso para o produto.

                        Não confunda o total geral da compra com o total de um produto.

                        Produtos vendidos por peso podem ter quantidade ausente, mesmo quando o preço por quilograma e o valor total estiverem disponíveis.

                        Se não puder classificar um produto com segurança, use "Outros".

                        Não crie categorias novas nem altere os nomes das categorias permitidas.

                        Não adicione campos à estrutura definida.

                        Se o documento for válido, mas algumas informações estiverem ilegíveis, preserve os dados identificáveis e use null nos campos correspondentes.
                        """
                }
                ]
            }
        );

        var textoJson = response.Candidates?
    .FirstOrDefault()?
    .Content?
    .Parts?
    .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text))?
    .Text;

        if (string.IsNullOrWhiteSpace(textoJson))
        {
            throw new InvalidOperationException(
                "O Gemini não retornou conteúdo para a análise."
            );
        }

        try
        {
            var compra = JsonSerializer.Deserialize<CompraExtraida>(
                textoJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (compra is null)
            {
                throw new InvalidOperationException(
                    "Não foi possível interpretar os dados da compra."
                );
            }

            var erroValidacao = ValidarCompra(compra);

            if (erroValidacao is not null)
            {
                throw new InvalidOperationException(erroValidacao);
            }

            return compra;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "O Gemini retornou uma resposta que não é um JSON válido.",
                ex
            );
        }
    }
}
