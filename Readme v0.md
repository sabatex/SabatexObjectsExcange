# _ObjectExcange_ The simple service for exchange objects 



## Login Autentificate for take access token

### POST [host]/api/v0/login
#### Headers:
    "Content-Type":"application/json; charset=utf-8"
    "accept": "*/*"
#### Body:
```
	{	
		"clientId":<client GUID>,
		"password":<string>
	}
```
#### response:
```
    {
        "access_token":<string>,
        "refresh_token":<string>,
        "token_type":"Bearer",
        "expires_in":<int>
     }
```

### POST [host]/api/v0/RefresToken
#### Headers:
    "Content-Type":"application/json; charset=utf-8"
    "accept": "*/*"
#### Body:
```
	{	
		"clientId":<client GUID>,
		"password":<refreshToken>
	}
```
#### response:
```
    {
        "access_token":<string>,
        "refresh_token":<string>,
        "token_type":"Bearer",
        "expires_in":<int>
     }
```

# Objects exchange service
## POST object to service
### POST  [host]/api/v0/objects
#### Headers:
    "Content-Type":"application/json; charset=utf-8"
    "accept", "*/*"
    "clientId",<client GUID> 
    "destinationId", <destination client GUID>
    "apiToken", "<access Token>"
#### Body:
```
    {
        "objectType":"string", // max 50
        "objectId":"string",   // max 50
        "text":"string",       // unlimited (system limit)
        "dateStamp":"DateTime" // date time send message
    }
```
#### response:
    200

## GET objects from service
### GET [host]/api/v0/objects?take={1..200}  - default 10
#### Headers:
        "Content-Type":"application/json; charset=utf-8"
        "accept", "*/*"
        "clientId":<client GUID>
        "apiToken", "<access Token>"
#### request:
```
    [{
            "id":long,              // inner Id
            "sender": "string",     // client guid id 
            "destination":"string", // client guid id
            "objectId":"string",    // object id (max 50)
            "objectType":"string",  // object type (max 50)
            "dateStamp":"string",   // Date service registered 
            "objectAsText":"string" // serialized object (JSON,XML,CSV ...)
    }]
```
## DELETE object from service
### DELETE [host]/api/v0/objects/{id}
#### Headers:
        "accept", "*/*"
        "clientId":<client GUID>
        "apiToken", "<access Token>"
#### request:
    200


# QUERY exchange service
## POST query to service
### POST  [host]/api/v0/queries
#### Headers:
		"Content-Type":"application/json; charset=utf-8"
        "accept", "*/*"
        "clientId",<client GUID> 
        "destinationId", <destination client GUID>
        "apiToken", "<access Token>"
#### Body:
```
    {
        "objectType":"string", // max 50
        "objectId":"string"   // max 50
    }
```
#### response:
    200

## GET objects from service
### GET [host]/api/v0/queries?take={1..200}  - default 10
#### Headers:
        "Content-Type":"application/json; charset=utf-8"
        "accept", "*/*"
        "clientId":<client GUID>
        "apiToken", "<access Token>"

#### request:
```
    [{
            "id":long,              // inner Id
            "sender": "string",     // client guid id 
            "destination":"string", // client guid id
            "objectId":"string",    // object id (max 50)
            "objectType":"string"  // object type (max 50)
    }]
```
## DELETE object from service
### DELETE [host]/api/v0/queries/{id}
#### Headers:
        "accept", "*/*"
        "clientId":<client GUID>
        "apiToken", "<access Token>"
#### request:
    200