# Tech Challenge — Guia para agentes (`autenticacao-service`)

Serviço de autenticação: login de staff, JWT de cliente (RS256) e JWKS. Org [fiap-vcosta](https://github.com/fiap-vcosta).

## Antes de mudar código

1. Ler o README deste repositório e [`docs/01_requisitos.md`](docs/01_requisitos.md)
2. Não inventar features fora do escopo descrito nas rules
3. **Git:** nunca commit/push direto em `main` — branch → PR → merge (ver [`.cursor/rules/git-workflow.mdc`](.cursor/rules/git-workflow.mdc))

## Layout

| Peça | Papel |
|------|--------|
| `src/Domain` | `Usuario`, `TipoUsuario`, exceções |
| `src/Application` | Use cases MediatR + ports |
| `src/Infrastructure` | Firestore, RS256, HTTP administrativo, SHA-256 |
| `src/Api` | Login, token cliente, JWKS, health |
| `tests/UnitTests` | Gate Coverlet 80% (exclui `FirestoreUsuarioRecordStore` — adapter SDK) |
| `tests/IntegrationTests` | HTTP + fakes |

## Fora de escopo

Ordem de serviço e regras de negócio de staff além de usuários e tokens.

## Comandos

```bash
dotnet restore TechChallenge.sln
dotnet build TechChallenge.sln -c Release
dotnet test tests/UnitTests/UnitTests.csproj -c Release \
 "/p:CollectCoverage=true" \
 "/p:CoverletOutputFormat=cobertura" \
 "/p:Include=[Domain]*%2c[Application]*%2c[Infrastructure]*%2c[Api]*" \
 "/p:Threshold=80" \
 "/p:ThresholdType=line%2Cbranch" \
 "/p:ThresholdStat=minimum"
dotnet test tests/IntegrationTests/IntegrationTests.csproj -c Release
./scripts/setup-local-jwt.sh
dotnet run --project src/Api/Api.csproj
```
