#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
dir="$root/.local/jwt"
mkdir -p "$dir"

if [[ ! -f "$dir/private.pem" ]]; then
  openssl genrsa -out "$dir/private.pem" 2048
  openssl rsa -in "$dir/private.pem" -pubout -out "$dir/public.pem"
  chmod 600 "$dir/private.pem"
  echo "Par RSA de teste criado em $dir (não versionado)."
else
  echo "Par RSA já existe em $dir — mantido."
  if [[ ! -f "$dir/public.pem" ]]; then
    openssl rsa -in "$dir/private.pem" -pubout -out "$dir/public.pem"
  fi
fi

priv_escaped="$(python3 - <<PY
from pathlib import Path
print(Path("$dir/private.pem").read_text().strip().replace("\n", r"\n"))
PY
)"

env_file="$root/.env"
if [[ ! -f "$env_file" ]]; then
  cat >"$env_file" <<EOF
ASPNETCORE_ENVIRONMENT=Development
FIRESTORE_PROJECT_ID=local-autenticacao
FIRESTORE_EMULATOR_HOST=127.0.0.1:8085
Firestore__ProjectId=local-autenticacao
Firestore__DatabaseId=(default)
JWT_ISSUER=autenticacao-service
JWT_AUDIENCE_FUNCIONARIO=funcionario
JWT_AUDIENCE_CLIENTE=cliente
JWT_KEY_ID=autenticacao-2026
JWT_SIGNING_KEY_PEM=${priv_escaped}
ADMINISTRATIVO_BASE_URL=http://localhost:8080
ServiceAuth__Key=local-service-auth-key-change-me
SERVICE_AUTH_KEY=local-service-auth-key-change-me
NUGET_AUTH_TOKEN=
DD_API_KEY=
DD_SITE=datadoghq.com
EOF
  echo "Criado $env_file — preencha NUGET_AUTH_TOKEN (PAT read:packages) para docker build."
else
  if grep -q '^JWT_SIGNING_KEY_PEM=' "$env_file"; then
    python3 - <<PY
from pathlib import Path
path = Path("$env_file")
lines = path.read_text().splitlines()
pem = Path("$dir/private.pem").read_text().strip().replace("\n", r"\n")
out = []
replaced = False
for line in lines:
    if line.startswith("JWT_SIGNING_KEY_PEM="):
        out.append(f"JWT_SIGNING_KEY_PEM={pem}")
        replaced = True
    else:
        out.append(line)
if not replaced:
    out.append(f"JWT_SIGNING_KEY_PEM={pem}")
path.write_text("\n".join(out) + "\n")
PY
    echo "Atualizado JWT_SIGNING_KEY_PEM em $env_file."
  else
    echo "JWT_SIGNING_KEY_PEM=${priv_escaped}" >>"$env_file"
    echo "Acrescentado JWT_SIGNING_KEY_PEM em $env_file."
  fi
fi
