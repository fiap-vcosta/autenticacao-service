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
| `docs/requestly/` | Collection Requestly (Docker / Local / GCP) |

## Endpoints

| Método | Path | Acesso |
|--------|------|--------|
| `POST` | `/autenticacao/login` | anônimo — `{ "login", "senha" }` → JWT staff |
| `POST` | `/autenticacao/clientes/token` | anônimo — `{ "documento" }` → JWT cliente |
| `GET` | `/autenticacao/.well-known/jwks.json` | anônimo — chave pública RS256 |
| `GET` | `/autenticacao/health` | anônimo |

Seed: usuário `admin` / `admin` (SHA-256, idempotente).

## Local

```bash
cp .env.example .env
export NUGET_AUTH_TOKEN=…          # PAT read:packages
./scripts/setup-local-jwt.sh       # .local/jwt + JWT_SIGNING_KEY_PEM no .env

# Só o emulador (dotnet run no host)
docker compose up firestore -d
export FIRESTORE_EMULATOR_HOST=127.0.0.1:8085
dotnet run --project src/Api/Api.csproj
# → http://localhost:8080/autenticacao/health

# API + emulador no Compose (porta 8081; administrativo no host em :8080)
# No .env: ADMINISTRATIVO_BASE_URL=http://host.docker.internal:8080
docker compose --profile app up --build
# → http://localhost:8081/autenticacao/health
```

JWT cliente: suba o `administrativo-service` com o **mesmo** `SERVICE_AUTH_KEY`, cadastre um cliente (Requestly do administrativo) e chame `POST /autenticacao/clientes/token`.

Requestly: importar [`docs/requestly/autenticacao.requestly.json`](docs/requestly/autenticacao.requestly.json).

Localmente o Firestore usa o database `(default)` do emulador. Na GCP (parte 4) o database nomeado é `autenticacao`.

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
