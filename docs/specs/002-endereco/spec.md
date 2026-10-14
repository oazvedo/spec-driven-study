# 002. Endereço: Spec

- **Status:** Aprovada
- **Data:** 2026-10-13

## Problema
Hoje o sistema só guarda o nome e o e-mail do usuário. Não há onde registrar onde ele mora, trabalha
ou recebe entregas, e o administrador precisa controlar essa informação fora do sistema.

## Objetivo
O administrador gerencia os **endereços** dos usuários: listar, cadastrar, editar, inativar, reativar e excluir.
Um usuário pode ter vários endereços, cada um com um tipo (comercial, residencial ou de entrega).

## Histórias de usuário
- Como **administrador**, quero **ver a lista de endereços paginada** para encontrá-los rapidamente.
- Como **administrador**, quero **cadastrar um endereço para um usuário** informando os dados e o tipo.
- Como **administrador**, quero **editar um endereço**, inclusive trocando o usuário dono dele.
- Como **administrador**, quero **inativar e reativar um endereço** sem perder os dados.
- Como **administrador**, quero **excluir de vez um endereço** cadastrado por engano.

## Regras de negócio
- **RN-01:** Todo endereço pertence a **um** usuário existente. Um usuário pode ter vários endereços.
- **RN-02:** São obrigatórios: logradouro, número, bairro, cidade, estado, CEP e tipo. O complemento é opcional.
- **RN-03:** Tamanhos máximos: logradouro, complemento, bairro e cidade com 100 caracteres; número com 10.
- **RN-04:** O estado é a UF com 2 letras maiúsculas (ex.: `SP`).
- **RN-05:** O CEP tem 8 dígitos, com ou sem hífen (`01310-100` ou `01310100`).
- **RN-06:** O tipo é um destes: **Comercial**, **Residencial** ou **Entrega**.
- **RN-07:** Todo endereço novo nasce **ativo**.
- **RN-08:** Diferente do usuário, o endereço **pode ser excluído de vez**. Depois de excluído, não aparece mais em lugar nenhum.
- **RN-09:** A listagem segue o padrão da spec 001: 20 itens por página por padrão, no máximo 100, os mais recentes primeiro.
  Ativos e inativos aparecem juntos, com a indicação do status.

## Critérios de aceite
- **CA-01:** Dado que existem 25 endereços, quando o administrador abre a listagem, então vê os 20 mais recentes
  e a indicação de que existem 2 páginas e 25 registros.
- **CA-02 (falha):** Quando alguém pede uma página com mais de 100 itens, ou uma página menor que 1,
  então o sistema recusa e informa o limite.
- **CA-03:** Dado um usuário existente e dados válidos, quando o administrador cadastra um endereço,
  então ele aparece na listagem como ativo, ligado a esse usuário.
- **CA-04 (falha):** Quando o administrador tenta cadastrar ou editar com um campo obrigatório vazio ou acima do tamanho máximo,
  então o sistema recusa, mostra a mensagem junto de cada campo e nada é gravado.
- **CA-05 (falha):** Quando a UF não tem 2 letras maiúsculas, o CEP não segue o formato ou o tipo não é um dos aceitos,
  então o sistema recusa com a mensagem do campo (ex.: "CEP inválido. Use 00000-000 ou 00000000.").
- **CA-06 (falha):** Quando o administrador cadastra ou edita um endereço apontando para um usuário que não existe,
  então o sistema informa que o usuário não foi encontrado e nada é gravado.
- **CA-07:** Dado um endereço existente, quando o administrador altera os dados (inclusive o usuário) com valores válidos,
  então a listagem passa a exibir os dados novos.
- **CA-08 (falha):** Quando alguém consulta, edita, inativa, reativa ou exclui um endereço que não existe,
  então o sistema informa que o endereço não foi encontrado.
- **CA-09:** Dado um endereço ativo, quando o administrador o inativa, então ele continua na listagem com o status "Inativo".
  Quando o reativa, o status volta a "Ativo".
- **CA-10:** Dado um endereço existente, quando o administrador confirma a exclusão, então ele some da listagem
  e uma nova consulta informa que não foi encontrado.
- **CA-11 (falha):** Quando o administrador clica em excluir e cancela a confirmação, então nada muda.

## Fora de escopo
- Busca de endereço pelo CEP (ViaCEP ou similar).
- Filtrar a listagem por usuário, tipo ou status.
- Endereço principal ou padrão por usuário, e limite de endereços por tipo.
- Validar se a UF e a cidade existem de fato.
- Login, senha e permissões (spec 003).

## Dúvidas em aberto
- Nenhuma. *(Resolvidas em 2026-10-13: a tela de endereços é uma página própria, com um seletor de usuário
  no formulário; e o endereço pode ser reativado, seguindo o padrão do ADR 0008.)*
