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
| `terraform/` | Firestore `autenticacao` + secret SM da chave RS256 |
| `k8s/` | Deployment (Datadog), Service, HPA, KSA, HTTPRoute `/autenticacao` |
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

Localmente o Firestore usa o database `(default)` do emulador. Na GCP o database nomeado é `autenticacao`.

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

Callers dos reusables em [`fiap-vcosta/actions`](https://github.com/fiap-vcosta/actions):

- **dotnet** — InspectCode, unit (gate 80%), integração, SonarCloud (`fiap-vcosta_autenticacao-service`)
- **terraform** — `fmt` / `validate` no root `terraform/`

## Terraform (GCP)

Root module: [`terraform/`](terraform/). State GCS prefix `autenticacao-service`.

| Recurso | Papel |
|---------|--------|
| Firestore database `autenticacao` | Persistência de usuários staff |
| Secret SM `autenticacao-service-jwt-signing-key` | Chave privada RS256 (`Jwt__SigningKeyPem`; `\n` escapados) |

Uma única chave assina JWT staff e cliente (claims/audiences distintos). A chave privada passa pelo tfstate ao gerar o secret (risco aceito na demo; sem rotação).

`tf-apply` / `tf-destroy` são manuais. O `deploy` monta o Secret Kubernetes a partir do SM (`Jwt__SigningKeyPem`) e dos org secrets `SERVICE_AUTH_KEY` + `DD_API_KEY`. A mesma `SERVICE_AUTH_KEY` da org é a chave da malha `X-Service-Key` (outbound deste serviço e inbound dos callees).

## CD (GCP)

| Workflow | Gatilho | Papel |
|----------|---------|--------|
| `build-push` | push em `main` | Imagem no Artifact Registry |
| `tf-apply` / `tf-destroy` | `workflow_dispatch` | Firestore + secret JWT |
| `deploy` | `workflow_dispatch` | Secret k8s + manifests + smoke em `https://vcosta-fiap.online/autenticacao/health` |

Pré-requisitos do `deploy`:

1. Cluster + Gateway do [`infra-k8s`](https://github.com/fiap-vcosta/infra-k8s) no ar
2. `tf-apply` deste repo (Firestore + JWT no SM)
3. Org secret `SERVICE_AUTH_KEY` (JWT cliente só funciona quando o administrativo estiver no ar com a **mesma** chave)
4. Imagem publicada pelo `build-push`

O `HTTPRoute` expõe `/autenticacao`. No cluster, a app chama o administrativo em `http://administrativo-service`.
