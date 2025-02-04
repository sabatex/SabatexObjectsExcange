# Simple secure data exchange service. 

## API V1 Description [depricated V0](Readme%20v0.md)

### Login Autentificate for take access token

#### POST [host]/api/v1/login
##### Headers:
    "Content-Type":"application/json; charset=utf-8"
    "accept": "*/*"
##### Body:
```
	{	
		"clientId":<client GUID>,
		"password":<string>
	}
```
##### response:
```
    {
        "access_token":<string>,
        "refresh_token":<string>,
        "token_type":"Bearer",
        "expires_in":<int>
     }
```
#### response  status code
    200 - success
    401 - Unauthorized

#### POST [host]/api/v1/RefresToken
##### Headers:
    "Content-Type":"application/json; charset=utf-8"
    "accept": "*/*"
##### Body:
```
	{	
		"clientId":<client GUID>,
		"password":<refreshToken>
	}
```
##### response:
```
    {
        "access_token":<string>,
        "refresh_token":<string>,
        "token_type":"Bearer",
        "expires_in":<int>
     }
```
#### response  status code
    200 - success
    401 - Unauthorized

# Objects exchange service
## POST object to service
### POST  [host]/api/v1/objects
#### Headers:
```html
    "Content-Type":"application/json; charset=utf-8"
    "accept", "*/*"
    "clientId",<client GUID> 
    "destinationId", <destination client GUID>
    "apiToken", "<access Token>"
```
#### Body:
```json
    {
        "messageHeader":"object header  (max 150)",
        "message":"serialized object (JSON,XML,CSV ...)",
        "dateStamp":"DateTime" // date time send message
    }
```
#### response  status code
    200 - success
    401 - Unauthorized


## GET objects from service
### GET [host]/api/v1/objects?take={1..200}  - default 10
#### Headers:
```html
        "Content-Type":"application/json; charset=utf-8"
        "accept", "*/*"
        "clientId":<client GUID>
        "destinationId", <destination client GUID>
        "apiToken", "<access Token>"
```
#### responce:
```
    [{
            "id":long,                   // inner Id
            "sender": "client GUID",  
            "destination":"client GUID",
            "messageHeader":"object header  (max 150)",
            "dateStamp":"Date registered on service", 
            "senderDateStamp":"sender date registered"
            "message":"serialized object (JSON,XML,CSV ...)"
    }]
```
#### response  status code
    200 - success
    401 - Unauthorized

## DELETE object from service
### DELETE [host]/api/v1/objects/{id}
#### Headers:
        "accept", "*/*"
        "clientId":<client GUID>
        "apiToken", "<access Token>"
#### response:
#### response  status code
    200 - success
    401 - Unauthorized

