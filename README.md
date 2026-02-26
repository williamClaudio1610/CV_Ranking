
---

# CV_Ranking — Mini-lab de ranqueamento de currículos

Este projeto é um **laboratório prático** para experimentar técnicas de *matching* entre **descrições de vaga** e **currículos**, usando **ML.NET** com auxílio de **TF-IDF** e **similaridade de cosseno**.

Na prática, a aplicação:

* extrai texto de currículos em PDF,
* normaliza e prepara os textos,
* e expõe uma **API REST** para cadastrar candidatos e calcular o ranking de aderência de cada CV a uma vaga.
---

## Tecnologias utilizadas

* **.NET 10 / C# 14**
* **ML.NET** (TF-IDF + similaridade de cosseno)
* **Entity Framework Core** (persistência de dados)
* **API REST** (Controllers)
* **Ferramentas**: `dotnet`, EF Core Tools, Docker (opcional)

---

## Requisitos

* .NET 10 SDK instalado
* Docker (opcional, para execução em container)
* EF Core Tools (opcional)


---

## Execução local (passo a passo)

### 1. Clonar o repositório

```bash
git clone https://github.com/williamClaudio1610/CV_Ranking.git
cd CV_Ranking
```

### 2. Configurar a base de dados

Edite o arquivo `appsettings.json` e ajuste a **connection string** de acordo com o banco que pretende usar.

### 3. Restaurar dependências

```bash
dotnet restore
```

### 4. Criar ou aplicar migrações (EF Core)

* Se as migrações já existirem:

  ```bash
  dotnet ef database update
  ```
* Caso contrário, crie a migração inicial:

  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

### 5. Executar a aplicação

```bash
dotnet run
```

Ou, se estiver no Visual Studio:

```
Debug > Start Debugging
```

---

## Observações importantes

* O modelo do banco pode ser analisado em `DAL/AppDbContext`.
* Para ambientes isolados ou deploy rápido, é possível executar tudo via **Docker**, usando o `Dockerfile` incluído no repositório.
* Este projeto é intencionalmente simples e didático — serve como base para evoluções mais robustas.

---

## Endpoints (exemplos)

> **Importante:** as rotas e contratos definitivos estão nos arquivos
> `Controllers/CandidatosController.cs` e `Controllers/MatchingController.cs`.
> Os exemplos abaixo representam o uso típico esperado da API.

### 1) Cadastro de candidato (JSON)

**POST** `/api/candidatos`

Payload (exemplo):

```json
{
  "nome": "João Silva",
  "email": "joao@exemplo.com",
  "cvText": "Experiência em C#, .NET, ML.NET e APIs REST."
}
```

Resposta — **201 Created**:

```json
{
  "id": 1,
  "nome": "João Silva",
  "email": "joao@exemplo.com"
}
```

Use este endpoint quando o texto do CV já estiver disponível (por exemplo, extraído externamente).

---

### 2) Upload de CV (PDF) — alternativa

**POST** `/api/candidatos/upload`
**Content-Type:** `multipart/form-data`

* Campo `file`: arquivo PDF do currículo.

O serviço `Services/PdfTextExtractor.cs` é responsável por:

* extrair o texto do PDF,
* normalizar o conteúdo,
* persistir o candidato na base.

Este é o fluxo mais realista para uso em produção.

---

### 3) Calcular matching (descrição da vaga)

**POST** `/api/matching`

#### Variante A — descrição + IDs de candidatos

```json
{
  "jobDescription": "Vaga para desenvolvedor .NET com experiência em ML",
  "candidateIds": [1, 2, 3]
}
```

#### Variante B — descrição + candidatos inline

```json
{
  "jobDescription": "Vaga para desenvolvedor .NET com experiência em ML",
  "candidates": [
    { "id": 1, "text": "Currículo com experiência em .NET" },
    { "id": 2, "text": "Formação em ciência de dados e ML.NET" }
  ]
}
```

Resposta (exemplo):

```json
[
  { "candidateId": 2, "score": 0.87 },
  { "candidateId": 1, "score": 0.63 }
]
```

Os resultados já vêm **ordenados por relevância** (maior score primeiro).

---

## Estrutura do projeto

Principais pastas e responsabilidades:

* `Controllers/`
  Endpoints da API (`CandidatosController`, `MatchingController`)

* `Services/`
  Lógica de negócio e ML
  (`MatchingEngine.cs`, `PdfTextExtractor.cs`, `NormalizarTexto.cs`)

* `DAL/`
  Acesso a dados, `AppDbContext`, repositórios e interfaces

* `Models/`
  Entidades e DTOs
  (`Vaga.cs`, `CandidatoCV.cs`, `ResultadoMatching.cs`)

* Arquivos de configuração:
  `Program.cs`, `appsettings.json`, `Dockerfile`, `launchSettings.json`

---

## Como funciona o motor de matching

* O núcleo está em `Services/MatchingEngine.cs`.
* Usa `MLContext` do **ML.NET** para:

  * featurizar textos via `FeaturizeText` (TF-IDF),
  * gerar vetores para a vaga e para os currículos,
  * calcular **similaridade por cosseno**.
* O pré-processamento (limpeza e normalização de texto) fica em
  `Services/NormalizarTexto.cs`.
* Extração de texto de PDFs é feita em
  `Services/PdfTextExtractor.cs`.

---

## Repositório

[https://github.com/williamClaudio1610/CV_Ranking](https://github.com/williamClaudio1610/CV_Ranking)

---

## Notas finais

* Sempre confirme rotas e contratos diretamente nos controllers antes de consumir a API.
* Este projeto foi pensado como **base de evolução**, não como produto final.
* Dá para expandir facilmente para:

  * pesos por habilidades,
  * persistência de resultados,
  * ou integração com ATS.
