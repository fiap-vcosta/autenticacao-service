resource "tls_private_key" "jwt" {
  algorithm = "RSA"
  rsa_bits  = 2048
}

locals {
  jwt_signing_key_pem = replace(tls_private_key.jwt.private_key_pem, "\n", "\\n")
}

resource "google_secret_manager_secret" "jwt_signing_key" {
  secret_id = "autenticacao-service-jwt-signing-key"
  project   = var.project_id

  labels = {
    service = "autenticacao-service"
  }

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret_version" "jwt_signing_key" {
  secret      = google_secret_manager_secret.jwt_signing_key.id
  secret_data = local.jwt_signing_key_pem
}
