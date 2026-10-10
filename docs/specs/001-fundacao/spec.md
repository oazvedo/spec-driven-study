# 001. Fundação: Spec

- **Status:** Aceito
- **Data:** 2026-10-10

## Problema
O repositório ainda não tem código. Antes de qualquer domínio real, é preciso de uma base executável
que demonstre, de ponta a ponta, todos os padrões definidos nos ADRs, para servir de referência
ao estudo e de modelo para as próximas features.

## Objetivo
Ter um sistema rodando localmente em que um administrador gerencia **usuários** (listar, cadastrar,
editar, ativar e inativar). Esse cadastro é a entidade de referência que as próximas features copiam.

## Histórias de usuário
- Como **administrador**, quero **ver a lista de usuários paginada** para encontrá-los rapidamente.
- Como **administrador**, quero **cadastrar um usuário** informando nome e e-mail.
- Como **administrador**, quero **editar o nome e o e-mail** de um usuário.
- Como **administrador**, quero **inativar e reativar um usuário** sem perder o histórico.
- Como **operador**, quero **verificar se o sistema está no ar** e consegue acessar o banco de dados.

## Regras de negócio
- **RN-01:** O nome é obrigatório e tem no máximo 150 caracteres.
- **RN-02:** O e-mail é obrigatório, precisa ser válido, tem no máximo 254 caracteres e é **único** entre os usuários.
- **RN-03:** Todo usuário novo nasce **ativo**.
- **RN-04:** Não existe exclusão definitiva. Um usuário só pode ser inativado e reativado.
- **RN-05:** A listagem mostra 20 itens por página por padrão, aceita no máximo 100 e exibe primeiro os cadastrados mais recentemente.
  Usuários ativos e inativos aparecem juntos, com a indicação do status.

## Critérios de aceite
- **CA-01:** Dado que existem 25 usuários, quando o administrador abre a listagem, então vê os 20 mais recentes
  e a indicação de que existem 2 páginas e 25 registros.
- **CA-02 (falha):** Quando alguém pede uma página com mais de 100 itens, ou uma página menor que 1,
  então o sistema recusa e informa o limite.
- **CA-03:** Dado um nome e um e-mail válidos e inéditos, quando o administrador cadastra o usuário,
  então ele aparece na listagem como ativo.
- **CA-04 (falha):** Quando o administrador tenta cadastrar com o nome ou o e-mail vazios, então o sistema recusa,
  mostra a mensagem junto de cada campo e nada é gravado.
- **CA-05 (falha):** Quando o e-mail informado não é um e-mail válido, então o sistema recusa com "E-mail inválido.".
- **CA-06 (falha):** Dado que o e-mail já pertence a outro usuário, quando o administrador cadastra ou edita
  usando esse e-mail, então o sistema recusa informando que o e-mail já está em uso.
- **CA-07:** Dado um usuário existente, quando o administrador altera o nome e o e-mail com dados válidos,
  então a listagem passa a exibir os dados novos.
- **CA-08 (falha):** Quando alguém consulta, edita, ativa ou inativa um usuário que não existe,
  então o sistema informa que o usuário não foi encontrado.
- **CA-09:** Dado um usuário ativo, quando o administrador o inativa, então ele continua na listagem
  com o status "Inativo". Quando o reativa, o status volta a "Ativo".
- **CA-10:** Quando o operador consulta a saúde do sistema com o banco disponível, então recebe a confirmação de que está saudável.

## Fora de escopo
- Login, senha e permissões (spec 002).
- Busca, filtros e ordenação escolhida pelo usuário.
- Exclusão definitiva de registros.
- Deploy, CI e ambientes além do local.

## Dúvidas em aberto
- O e-mail deve ser único sem diferenciar maiúsculas de minúsculas (`Ana@x.com` = `ana@x.com`)?
  *Sugestão:* sim, guardando o e-mail sempre em minúsculas. 
