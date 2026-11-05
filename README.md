# autenticacao-service

Serviço de autenticação da oficina: login de staff e JWT de cliente (RS256),
com JWKS para os demais serviços validarem a assinatura. O documento do cliente
é consultado no administrativo.

Stack: .NET 8 e Firestore (database nomeado). Org [fiap-vcosta](https://github.com/fiap-vcosta).

Requisitos: [`docs/01_requisitos.md`](docs/01_requisitos.md).

## Layout

| Peça | Papel |
|------|--------|
| `src/Domain` | `Usuario`, `TipoUsuario`, exceções |
| `src/Application` | Login e emissão de JWT cliente (MediatR) |
| `src/Infrastructure` | Firestore, RS256 issuer, HTTP para administrativo, SHA-256 |
| `src/Api` | `POST /autenticacao/login`, `POST /autenticacao/clientes/token`, JWKS, health |
| `tests/UnitTests` | Gate Coverlet ≥ 80% line/branch |
| `tests/IntegrationTests` | HTTP + fakes (store em memória / administrativo stub) |

## Endpoints

| Método | Path | Acesso |
|--------|------|--------|
| `POST` | `/autenticacao/login` | anônimo — `{ "login", "senha" }` → JWT staff |
| `POST` | `/autenticacao/clientes/token` | anônimo — `{ "documento" }` → JWT cliente |
| `GET` | `/autenticacao/.well-known/jwks.json` | anônimo — chave pública RS256 |
| `GET` | `/autenticacao/health` | anônimo |

Seed local/GCP: usuário `admin` / `admin` (SHA-256, idempotente).

## Local

```bash
export NUGET_AUTH_TOKEN=…   # PAT read:packages
./scripts/setup-local-jwt.sh
# Emulador Firestore (FIRESTORE_EMULATOR_HOST) — compose na entrega seguinte
dotnet run --project src/Api/Api.csproj
```

Health: `http://localhost:8080/autenticacao/health`

## Testes

```bash
dotnet test tests/UnitTests/UnitTests.csproj -c Release \
  "/p:CollectCoverage=true" \
  "/p:CoverletOutputFormat=cobertura" \
  "/p:Include=[Domain]*%2c[Application]*%2c[Infrastructure]*%2c[Api]*" \
  "/p:Threshold=80" \
  "/p:ThresholdType=line%2Cbranch" \
  "/p:ThresholdStat=minimum"
dotnet test tests/IntegrationTests/IntegrationTests.csproj -c Release
```

## CI

Caller do reusable `ci-dotnet` (`fiap-vcosta/actions`) com SonarCloud `fiap-vcosta_autenticacao-service` e restore do `building-blocks` via GitHub Packages.
