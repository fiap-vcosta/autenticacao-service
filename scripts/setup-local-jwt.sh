#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
JWT_DIR="${ROOT}/.local/jwt"
mkdir -p "${JWT_DIR}"

if [[ ! -f "${JWT_DIR}/private.pem" || ! -f "${JWT_DIR}/public.pem" ]]; then
  openssl genrsa -out "${JWT_DIR}/private.pem" 2048
  openssl rsa -in "${JWT_DIR}/private.pem" -pubout -out "${JWT_DIR}/public.pem"
  echo "Par RSA criado em ${JWT_DIR}"
else
  echo "Par RSA já existe em ${JWT_DIR}"
fi
