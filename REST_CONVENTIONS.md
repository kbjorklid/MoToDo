# REST conventions

## General

- **Versioning**: Use URI versioning: `GET /api/v1/....`
- **URI naming conventions**: use kebab case in URIs: `GET /api/v1/user-accounts`
- **JSON Key naming conventions**: use `camelCase`

## Filtering, Sorting, Field Selection and Pagination

**Important**: Consider carefully whether filterings, sorting, field selection and/or pagination are needed for a given endpoint. Do not always implement these (keep it simple). But when implementing, use the conventions described below.

### Filtering

`GET /api/v1/users?status=active`

### Sorting

Descending:`GET /api/v1/users?sort=-createdAt`
Ascending: `GET /api/v1/users?sort=createdAt`
Multi-field `GET /api/v1/users?sort=createdAt,email`


### Pagination

#### Request

`GET /api/v1/users?page=2&limit=100`

#### Response

```json
{
  "data": [...],
  "pagination": {
    "totalItems": 1230,
    "totalPages": 13,
    "currentPage": 2,
    "limit": 100
  }
}
```

### Field selection

`GET /api/v1/users?fields=id,name,email`

## Request/Response JSON structures

### No raw arrays

Responses or requests should never be raw arrays. Instead, an object wrapper should be used. This is to allow further 
non-breaking extension of the API. The key used should be called `data`:

Avoid returning raw arrays:
```json
[
  {
    "userName": "john",
    "email": "john@doe.com" 
  },
  { ... }
]
```

Good: use an object to wrap the array:
```json
{
  "data": [
    {
      "userName": "john",
      "email": "john@doe.com"
    },
    { ... }
  ]
}
```

Also good: return an object, no need to wrap single objects:
```json
{
  "userName": "john",
  "email": "john@doe.com"
}
```

### General

- Use descriptive **strings** to serialize enumeration values.
- Reuse structures. E.g. if there is one 'Address' structure used in a response, then reuse it in other responses rather than creating new, with different field names for example.
- Do not return 'null' values (remove the key-value pair from response)
- Use ISO 8601 (YYYY-MM-DDTHH:mm:ss.sssZ) as the date format


### Error responses

Common error structure:

```json
{
  "errors": [
    {
      "code": "AUTH_FAILURE",
      "message": "The provided API key is invalid.",
      "field": "..."
    }
  ]
}
```
`code` and `field` are optional.

## Status Codes

Most common status codes / scenarios to be used:

- 200 OK: General success for GET.
- 201 Created: Successful creation of a new resource (POST).
- 204 No Content: Successful request with no body to return (e.g., DELETE).
- 400 Bad Request: Client-side error (e.g., invalid JSON, validation error).
- 401 Unauthorized: Missing or invalid authentication.
- 403 Forbidden: Authenticated, but not permitted to access the resource.
- 404 Not Found: The requested resource does not exist.
