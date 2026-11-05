output "firestore_database_id" {
  description = "Id do database Firestore nomeado deste serviço."
  value       = google_firestore_database.autenticacao.name
}

output "jwt_signing_key_secret_id" {
  description = "Secret Manager id da chave privada RS256 (Jwt__SigningKeyPem)."
  value       = google_secret_manager_secret.jwt_signing_key.secret_id
}
