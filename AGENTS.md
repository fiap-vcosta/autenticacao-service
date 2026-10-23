# Tech Challenge — Guia para agentes (`autenticacao-service`)

Serviço de autenticação: login de staff, JWT de cliente (RS256) e JWKS. Org [fiap-vcosta](https://github.com/fiap-vcosta).

## Antes de mudar código

1. Ler o README deste repositório e, quando existirem, ADRs em `docs/adrs/`
2. Não inventar features fora do escopo descrito nas rules
3. **Git:** nunca commit/push direto em `main` — branch → PR → merge (ver [`.cursor/rules/git-workflow.mdc`](.cursor/rules/git-workflow.mdc))

## Layout alvo

Quando o código chegar: `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Api`, `tests/` espelhando `src/`, `k8s/`, `terraform/`, `Dockerfile`, `docker-compose.yml`. Dependências apontam para dentro.

## Fora de escopo

Ordem de serviço e regras de negócio de staff além de usuários e tokens.

## Comandos

Ainda não há solution, testes nem scripts neste repositório. Não inventar comandos `dotnet` até o código existir.
