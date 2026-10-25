# Requisitos — autenticacao-service

Sessão de Outubro de 2026. Este serviço autentica o staff e emite o JWT do cliente. Os dois tokens são RS256. A chave pública sai no JWKS. Onde a chave privada fica guardada não faz parte desta sessão. Persistência dos usuários staff em Firestore, num database nomeado. Não usa o database `(default)`. Não é dono de ordem de serviço nem das regras de negócio da oficina além de usuário e token.

Paths: [HTTP](https://github.com/fiap-vcosta/e2e/blob/main/docs/contratos-http.md).

## 1. Login staff

* **RF01:** `POST /autenticacao/login` é anônimo. O corpo é `{ "login", "senha" }`.
* **RF01.1:** Credencial válida responde o JWT staff. Os claims são o login, o papel e o id do usuário. A expiração é de 30 minutos.
* **RF01.2:** Credencial inválida não emite token.

## 2. JWT cliente

* **RF02:** `POST /autenticacao/clientes/token` é anônimo. O corpo é `{ "documento" }`, CPF ou CNPJ.
* **RF02.1:** O serviço consulta `GET /system/administrativo/clientes/por-documento/{documento}` com `X-Service-Key`. Não conhece o segredo com que o staff assina token, nem o contrário.
* **RF02.2:** Documento inválido responde 400 com `{ "errors": ["..."] }`. Cliente ausente responde 404. Cliente existente responde 200 com `{ "token" }`.
* **RF02.3:** O JWT cliente traz o claim `documento` e expira em 1800 segundos.

## 3. JWKS

* **RF03:** `GET /autenticacao/.well-known/jwks.json` é anônimo e publica a chave pública com que os outros serviços validam o JWT staff e o JWT cliente.
