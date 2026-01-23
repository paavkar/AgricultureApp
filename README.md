# AgricultureApp

## Introduction

This application was made for farm owners to manage their field cultivations.
Following features are present currently:
- Farm information management,
- Assigning managers to farms,
- Managing fields owned and cultivated by a farm,
- Managing field cultivation information by a farm,
- AI interaction related to field cultivations.

Farm managers can be assigned and removed only by the farm owner. Owners can do
everything managers can, while managers have limited actions. Managers are allowed
to manage the fields farm owns and cultivates, and manage the field cultivation
information. Managers can change the cultivating farm of the field. Cultivating
farm of a field can be assigned by the owning farm, however, reverting this action
can be done by either the owning farm or the farm currently cultivating it.

Each field has cultivations related to it. Only the owner or a manager of the
currently cultivating farm can manage cultivations of the field. This means the
owning farm of the field cannot manage the cultivations until it agains is the
cultivating farm. Each cultivation has the cultivating farm attached with it.

The project structure is created by following Clean Architecture.

If this application was ran in production, it would be done with Containers.
The local containerisation is done with Docker Compose and the API and (eventually)
the web UI are served with Nginx. A version of the UI has been started with .NET MAUI
(not related to containers).

Authentication in the Web API is created with ASP.NET Core Identity and JWT. The JWT setup has support for multiple roles for users. There is a refresh token setup for the tokens. The refresh tokens are currently saved with HybridCache. Every endpoint is
protected by the `Authorize` tag in the API to use the .NET middleware for JWT.

The Web API has localization built in to it. This works via `Microsoft.AspNetCore.Localization` package and `Accept-Language` header in the HTTP request. Localization
is in progress with English (en-GB) and Finnish (fi-FI).

LLM integration depends on the ASP.NET Core Environment. In `Development` environment,
the integration is done with Ollama, while otherwise an Azure/Microsoft Foundry model is used.
Both are done via Semantic Kernel.
Users can interact with an LLM to ask about farms currently cultivated fields or
a specific field it is cultivating. Information fetching is done with function
calling to enrich the LLM context. NB! Function calling does not work with Ollama models and
Semantic Kernel, a cloud model is required. Chat history is saved with HybridCache for
one (1) hour at a time from latest message.

## User Secrets / App settings

These are the needed key-value pairs to be set:
```
{
  "Kestrel:Certificates:Development:Password": "<SOME_GUID>",
  "Jwt": {
    "Key": "<256_BIT_VALUE>",
    "Issuer": "Agriculture App",
    "Audience": "Agriculture App",
  },
  "ConnectionStrings:DefaultConnection": "<YOUR_SQL_SERVER_CONNECTION>",
  "LLM": {
    "ModelId": "<YOUR_LLM_ID>,
    "Endpoint": "<LLM_ENDPOINT>",
    "DeploymentName": "<AZURE_LLM_DEPLOYMENT_NAME>",
    "AzureModelId": "<AZURE_MODEL_ID>",
    "AzureEndpoint": "<AZURE_ENDPOINT>",
    "ApiKey": "<AZURE_LLM_API_KEY>"
  }
}
```

## Diagrams

### LLM Usage

#### Chat creation

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>CreateChat()
    participant LLM as LlmService
    participant Cache as Cache

    U->>API: POST /create-chat
    API->>LLM: CreateChatHistoryAsync()

    LLM->>LLM: Generate new chatId (Guid v7)

    LLM->>LLM: Create ChatHistory<br/>+ SystemPrompt<br/>+ FirstAssistantMessage

    LLM->>Cache: SetAsync(chatId, history,<br/>expiration: 1 hour)
    Cache-->>LLM: OK

    LLM-->>API: chatId
    API-->>U: 200 OK<br/>{ ChatId: chatId }
```

#### Response generation

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>SendMessage()
    participant BG as Background Task
    participant LLM as LlmService
    participant Cache as Cache
    participant Chat as ChatCompletionService
    participant Sig as NotificationService

    U->>API: POST /send-message/{chatId}<br/>MessageDto
    API->>BG: Start background task (Task.Run)
    API-->>U: 202 Accepted

    BG->>LLM: GenerateStreamingResponseAsync(chatId, message, farmId)

    LLM->>Cache: GetOrCreateAsync(chatId)
    Cache-->>LLM: ChatHistory (existing or new)

    LLM->>LLM: Add user message to history

    LLM->>Chat: GetStreamingChatMessageContentsAsync(chatHistory)
    loop For each streamed token
        Chat-->>LLM: StreamingChatMessageContent token
        LLM->>LLM: Append token to assistantResponse
        LLM->>Sig: NotifyLlmStreamingResponseAsync(chatId, token)
    end

    LLM->>LLM: Add assistant message to history

    LLM->>Cache: SetAsync(chatId, updated ChatHistory)

    LLM->>Sig: NotifyLlmStreamingFinishedAsync(chatId)
```

### FarmController actions

The following diagrams have a base assumption that no exceptions (try-catch block) occur.
They all have base URL of `<DOMAIN>/api/v1/farm`. Each action requires authentication from
which userId is gotten.

#### Farm creation

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>CreateFarm()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper

    U->>API: POST /create<br/>CreateFarmDto farmDto
    API->>FS: CreateAsync(farmDto, userId)
    FS->>FR: AddAsync(farm)
    FR->>Dapper: ExecuteAsync(sql, farm)
    Dapper-->>FR: int rowsAffected
    FR-->>FS: int rowsAffected
    FS-->>API: FarmResult result
    alt operation not succesful
      API-->>U: 400 Bad Request<br/> { result }
    else operation successful
      API-->>U: 204 Created<br/> { result }
    end
```

#### Farm info fetch

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>GetFullFarmInfo()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper

    U->>API: GET /full-info/{farmId}
    API->>FS: GetFullInfoAsync(farmId)
    FS->>FR: GetFullInfoAsync(farmId)
    FR->>Dapper: QueryMultipleAsync(sql, { Id = farmId })
    Dapper-->>FR: Farm? farm
    FR-->>FS: Farm? farm
    FS-->>API: FarmResult result
    alt !result.Succeeded
      alt result.StatusCode is 404
        API-->>U: 404 Not Found<br/> { result }
      end
      alt authenticated user not permitted
        API-->>U: 403 Forbid
      end
      alt 
        API-->>U: 400 Bad Request<br/> { result }
      end
    else result.Succeeded
      API-->>U: 200 OK<br/> { result }
    end
```

#### Fetching owned farms

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>GetOwnedFarms()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper

    U->>API: GET /get-owned
    API->>FS: GetByOwnerAsync(userId)
    FS->>FR: GetByOwnerAsync(ownerId)
    FR->>Dapper: QueryAsync(sql, lambda)
    Dapper-->>FR: IEnumerable<FarmDto>? farms
    FR-->>FS: IEnumerable<FarmDto>? farms
    FS-->>API: FarmResult result
    alt operation not successful
      API-->>U: 400 Bad Request<br/> { result }
    else operation successful
      API-->>U: 200 OK<br/> { result }
    end
```

#### Fetching managed farms

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>GetManagedFarms()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper

    U->>API: GET /get-managed
    API->>FS: GetByManagerAsync(userId)
    FS->>FR: GetByManagerAsync(ownerId)
    FR->>Dapper: QueryAsync(sql, lambda)
    Dapper-->>FR: IEnumerable<FarmDto>? farms
    FR-->>FS: IEnumerable<FarmDto>? farms
    FS-->>API: FarmResult result
    alt operation not successful
      API-->>U: 400 BadRequest<br/> { result }
    else operation successful
      API-->>U: 200 OK<br/> { result }
    end
```

#### Updating farm information

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>UpdateFarm()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper

    U-->>API: PATCH /update/{farmId}<br/>UpdateFarmDto farmDto
    alt authenticated user and farm owner not matching
      API-->>U: 403 Forbid
    end
    alt farm id not matching in URL and body
      API-->>U: 400 Bad Request<br/> { new BaseResult }
    end
    API->>FS: UpdateAsync(farmDto, userId)
    FS->>FR: UpdateAsync(farmDto, userId)
    FR->>Dapper: ExecuteAsync(sql, object)
    Dapper-->>FR: int rowsAffected
    FR-->>FS: int rowsfAffected
    FS-->>API: FarmResult result
    alt operation not successful
      API-->>U: 400 Bad Request<br/> { result }
    else operation successful
      API-->>U: 200 OK<br/> { result }
    end
```

#### Adding a manager to farm

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>AddFarmManager()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper
    participant UM as UserManager
    participant NS as NotificationService

    U->>API: POST /add-manager/{farmId}<br/>string email
    alt email not provided
      API-->>U: 400 BadRequest<br/> { new BaseResult }
    end
    API->>FS: AddManagerAsync(userId, farmId, email)
    FS->>FR: IsUserOwnerAsync(farmId, userId)
    FR->>Dapper: ExecuteScalarAsync(sql, object)
    Dapper-->>FR: int count
    FR-->>FS: bool count > 0

    alt user is not owner
      FS-->>API: ManagerResult result
      API-->>U: 403 Forbid

    else user is owner
      FS->>UM: FindByEmailAsync(email)
      UM-->>FS: ApplicationUser? user
      alt user not found
        FS-->>API: ManagerResult result
        API-->>U: 404 Not Found<br/> { result }

      else user with email is found
        alt authenticated user matches with the found user
          FS-->>API: ManagerResult result
          API-->>U: 400 Bad Request<br/> { result }

        else found user is not the same as authenticated
          FS->>FR: AddManagerAsync(farmId, user.Id, DateTimeOffset.Now)
          FR->>Dapper: ExecuteAsync(sql, object)
          Dapper-->>FR: int rowsAffected
          FR-->>FS: int rowsAffected

          alt insertion not successful
            FS-->>API: ManagerResult result
            API-->>U: 400 Bad Request<br/> { result }
          
          else insertion successful
            FS->>NS: NotifyUserAddedToFarmAsync(user.Id, farmId)
            FS-->>API: ManagerResult
            API-->>U: 200 Ok<br/> { result }
          end
        end
      end
    end
```

#### Deleting a manager from farm

```mermaid
sequenceDiagram
    autonumber

    participant U as Client
    participant API as API Controller<br/>RemoveFarmManager()
    participant FS as FarmService
    participant FR as FarmRepository
    participant Dapper as Database Connection<br/>Dapper
    participant NS as NotificationService

    U->>API: DELETE /remove-manager/{farmId}<br/>string managerId
    alt managerId not provided
      API-->>U: 400 Bad Request<br/> { new BaseResult }
    end
    API->>FS: DeleteManagerAsync(farmId, userId, managerId)
    FS->>FR: IsUserOwnerAsync(farmId, userId)
    FR->>Dapper: ExecuteScalarAsync(sql, object)
    Dapper-->>FR: int count
    FR-->>FS: bool count > 0

    alt user is not owner
      FS-->>API: ManagerResult result
      API-->>U: 403 Forbid

      else user is owner
        FS->>FR: DeleteManagerAsync(farmId, managerId)
        FR->>Dapper: ExecuteAsync(sql, object)
        Dapper-->>FR: int rowsAffected

        alt no rows in DB were affected
          FS-->>API: ManagerResult result
          API-->>U: 400 Bad Request<br/> { result }
          else a manager was removed
            FS->>NS: NotifyUserRemovedFromFarmAsync(managerId, farmId)
            FS-->>API: BaseResult result
            API-->>U: 200 OK<br/> { result }
        end
    end
```