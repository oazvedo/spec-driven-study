# 0014. AwesomeAssertions no lugar de FluentAssertions

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
O ADR 0010 escolheu o FluentAssertions para as asserções dos testes .NET. A partir da v8, o FluentAssertions
passou a ter licença comercial (Xceed), paga para uso comercial. Este repositório é de estudos, mas pode virar
um app, e não queremos depender de uma licença paga só para escrever asserções.

## Decisão
- Usamos o **AwesomeAssertions** (fork comunitário do FluentAssertions, licença Apache 2.0) em todos os projetos de teste .NET.
- A API é a mesma (`.Should()`, `.Should().ThrowAsync<T>()`), então os exemplos dos ADRs e das skills continuam válidos.
- Esta decisão substitui **apenas** a escolha da biblioteca de asserções do ADR 0010. O restante do 0010 continua valendo.

## Alternativas consideradas
- **FluentAssertions 8.x:** exige licença comercial.
- **FluentAssertions 7.x:** a última versão Apache, mas congelada, sem correções futuras.
- **Shouldly:** open source, mas com uma API diferente, e seria preciso reescrever os exemplos.
- **Só `Assert` do xUnit:** sem dependência, mas as mensagens de erro ajudam menos a entender a falha.

## Consequências
- ✅ Licença livre, com a mesma API e a mesma ergonomia.
- ⚠️ É um fork mantido pela comunidade. Se ele for abandonado, volta a discussão (a migração para Shouldly seria mecânica).
