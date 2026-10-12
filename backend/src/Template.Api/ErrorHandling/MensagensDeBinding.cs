using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Template.Api.ErrorHandling;

// O que falha antes de chegar ao service (query, rota, corpo malformado) responde em português (ADR 0006).
// As regras de entrada continuam no FluentValidation.
public static class MensagensDeBinding
{
    public static void EmPortugues(MvcOptions options)
    {
        // Sem o [Required] implícito das propriedades não anuláveis: campo ausente chega nulo ao validator
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

        var m = options.ModelBindingMessageProvider;
        m.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
        m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        m.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado para {campo} não é válido.");
        m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
        m.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        m.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
        m.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
        m.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");
        m.SetMissingKeyOrValueAccessor(() => "O valor é obrigatório.");
        m.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");
        m.SetValueMustNotBeNullAccessor(valor => $"O valor '{valor}' não é válido.");
    }

    // Resposta 400 do [ApiController]. Um erro sem mensagem é o JSON malformado
    // (AllowInputFormatterExceptionMessages = false); sem isto, o MVC escreveria um texto fixo em inglês.
    public static IActionResult RespostaDeErro(ActionContext context)
    {
        var erros = new ModelStateDictionary();
        foreach (var (campo, entrada) in context.ModelState)
            foreach (var erro in entrada.Errors)
                erros.AddModelError(campo, string.IsNullOrEmpty(erro.ErrorMessage) ? "O JSON enviado não é válido." : erro.ErrorMessage);

        var problem = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(context.HttpContext, erros, title: GlobalExceptionHandler.TituloValidacao);
        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }
}
