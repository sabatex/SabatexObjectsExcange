// Copyright (c) 2021 by Serhiy Lakas
// https://sabatex.github.io
#region Logged
// Log data 
// conf - structure (use)
// level - 0..3 message level (o - error,1 - warning, 2- information, 3 - Note
// sourceName - text source name
// message - 
procedure Logged(conf,level,sourceName,message) export
	try
		if level <= conf.LogLevel then
			logLevel = УровеньЖурналаРегистрации.Примечание;
			if level = 0 then
				logLevel = УровеньЖурналаРегистрации.Ошибка;
			ИначеЕсли level=1 then
				logLevel = УровеньЖурналаРегистрации.Предупреждение;
			ИначеЕсли level=2 then
				logLevel = УровеньЖурналаРегистрации.Информация;
			endif;	
			
			ЗаписьЖурналаРегистрации("sabatexExchange",logLevel,sourceName,
			,
			message);
			conf.Log = conf.Log + message + Символы.ПС;
		endif;
	except
		
	endtry;
endprocedure
#endregion

#region WebApi
function ErrorConnectionString(conf)
	return "Сервер https://" + conf.Host +":"+conf.Port; 	
endfunction	
function CreateHTTPSConnection(conf)
	https = undefined;
	if conf.https then
		https =	 new ЗащищенноеСоединениеOpenSSL( неопределено, неопределено );
	endif;	
	return new HTTPConnection(
		conf.host, // сервер (хост)
        conf.port, // порт, по умолчанию для http используется 80, для https 443
        , // пользователь для доступа к серверу (если он есть)
        , // пароль для доступа к серверу (если он есть)
        , // здесь указывается прокси, если он есть
        , // таймаут в секундах, 0 или пусто - не устанавливать
        https//new ЗащищенноеСоединениеOpenSSL( неопределено, неопределено ) // защищенное соединение, если используется https
	);
	
endfunction	
function BuildUrl(url,queries=null)
	result  = url;
		if queries <> null then
			result  = result +"?";
			last = false;
			for each item in queries do
				if last then
					result = result + "&";
				endif;
				last=true;
				result = result + item.Ключ +"="+item.Значение;
			enddo;
		endif;
	return result;
endfunction	
// Cteate HTTP Request 
// params:
//    url - complete url for service
//    headers - map headers for request
//    queries - url queries  
function CreateHTTPRequest(url,headers,queries=null)
	request = new HTTPRequest(BuildUrl(url,queries));
	for each header in headers do
		request.Headers.Insert(header.Ключ, header.Значение);
	enddo;
	return request;
endfunction	
// Get HTTP Reuest
// 
function HTTPSGet(conf,url,headers,queries=null) export
	connection = CreateHTTPSConnection(conf);
	request = CreateHTTPRequest(url,headers,queries);
	return connection.Get(request);
endfunction
function HTTPSPostForm(conf,url,formParams=null) export
	connection = CreateHTTPSConnection(conf);
	request = CreateHTTPRequest(conf,url);
	request.Headers.Insert("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
	request.Headers.Insert("accept","*/*");

	if formParams <> null then
		form = "";
		last = false;
		for each item in formParams do
			if last then
				form = form + "&";
			endif;
			last = true;
			form = form + item.Ключ +"="+item.Значение;
		enddo;	
	endif;
	
	request.SetBodyFromString(form,"UTF-8",ИспользованиеByteOrderMark.НеИспользовать);
	try
		response = connection.Post(request);
		if response.StatusCode = 200 then
			return response.GetBodyAsString();
		else
			raise "Помилка запиту "+ url + " with StatusCode " + response.StatusCode; 
		endif;
 	except
		raise ErrorConnectionString(conf); 
	endtry;
	
	return response;
endfunction
function HTTPSPostJson(conf,url,jsonString) export
	connection = CreateHTTPSConnection(conf);
	request = CreateHTTPRequest(conf,url);
	request.Headers.Insert("Content-Type", "application/json; charset=utf-8");
	request.Headers.Insert("accept","*/*");
	request.SetBodyFromString(jsonString,"UTF-8",ИспользованиеByteOrderMark.НеИспользовать);
	return connection.Post(request);
endfunction	
function POSTObject(conf,url,object) export
	jsonText = Serialize(object);
	return HTTPSPostJson(conf,url,jsonText);
endfunction
#endregion

#region JSON
function Serialize(object) export
	jsonWriter = new JSONWriter;
	jsonParams = new JSONWriterSettings(ПереносСтрокJSON.Нет,,,,,,true);
	jsonWriter.SetString(jsonParams);
	if TypeOf(object) = Type("Structure")or TypeOf(object) = Type("Array")  then
		WriteJSON(jsonWriter,object);
	else
		СериализаторXDTO.WriteJSON(jsonWriter,object,НазначениеТипаXML.Неявное);
	endif;
	return jsonWriter.Close();
endfunction
function Deserialize(txt,datefields = undefined)  export
	if datefields = undefined then
		datefields = new array;
	endif;	
	
	jsonReader=Новый JSONReader;	
	jsonReader.SetString(txt);
	result = ReadJSON(jsonReader,true,datefields);
	if typeof(result) = type("Map") then
		objectXDTO = result.Get("#value");
		if objectXDTO <> undefined then
			return objectXDTO;
		endif;
	endif;
	return result;
endfunction	
#endregion

#region Autenfication
procedure Login(conf)
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/login"));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("Content-Type","application/json; charset=utf-8");
	jsonString = Serialize(new structure("clientId,password",string(conf.clientId),conf.password));
	try
		request.SetBodyFromString(jsonString,"UTF-8",ИспользованиеByteOrderMark.НеИспользовать);
		response = connection.Post(request);
		if response.StatusCode <> 200 then
			raise "Login error request with StatusCode="+ response.StatusCode;
		endif;

		apiToken = response.GetBodyAsString();
		if apiToken = "" then
			raise "Не отримано токен";
		endif;	
		token = Deserialize(apiToken);
		sabatexExchangeConfig.SetAccessToken(conf,token);
	except
		error = "Помилка ідентифікації на сервері! Error:"+ОписаниеОшибки();
		sabatexLog.LogError(conf,"Login",error);
		raise error;
	endtry;	
endprocedure	
procedure RefreshToken(conf)
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/refresh_token"));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("Content-Type","application/json; charset=utf-8");
	jsonString = Serialize(new structure("clientId,password",string(conf.clientId),conf.refresh_token));
	try
		request.SetBodyFromString(jsonString,"UTF-8",ИспользованиеByteOrderMark.НеИспользовать);
		response = connection.Post(request);
		if response.StatusCode <> 200 then
			raise "Login error request with StatusCode="+ response.StatusCode;
		endif;

		apiToken = response.GetBodyAsString();
		if apiToken = "" then
			raise "Не отримано токен";
		endif;
		token = Deserialize(apiToken);
		sabatexExchangeConfig.SetAccessToken(conf,token);
	except
		error = "Помилка ідентифікації на сервері! Error:"+ОписаниеОшибки();
		sabatexLog.LogError(conf,"Login",error);
		raise error;
	endtry;	

endprocedure
procedure checkToken(conf)
	if conf.access_token <> "" then
		if conf.expires_in <= (CurrentDate() + 60) then
			try
				RefreshToken(conf);
				return;
			except
			endtry;
		else
			return;
		endif;
	endif;	
	Login(conf);
endprocedure

#endregion

#region ExchangeWebApi

#region objects
// download objects from server bay sender nodeName
function GetObjectsExchange(conf) export
	var responce;
	checkToken(conf);
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/objects",new structure("take",conf.take)));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.access_token);
	request.Headers.Insert("Content-Type","application/json; charset=utf-8");
	try
		response = connection.Get(request);
		if response.StatusCode <> 200 then
			raise "GetObjectsExchange error request with StatusCode="+ response.StatusCode;
		endif;
	except
		raise "GetObjectsExchange error request with error:"+ОписаниеОшибки();
	endtry;	
	
	datefields = new array;
	datefields.Add("dateStamp");
	return Deserialize(response.GetBodyAsString(),datefields);	
endfunction
procedure DeleteExchangeObject(conf,id) export
	checkToken(conf);
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("/api/v0/objects/"+XMLString(id)));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.access_token);
 	response = connection.Delete(request);
	if response.StatusCode <> 200 then
		raise "Помилка запиту /api/v0/objects with id=" +XMLString(id)+ " with StatusCode: "+ response.StatusCode;	
	endif;
endprocedure
// POST Object to exchange service
// params:
//	 conf 			- structure (configuration)
//   destinationId  - string destination node id
//   objectType     - string(50) object type
//   objectId		- string(50) object Id
//   dateStamp      - DateTime The registered moment
//   textJSON       - The serialized object to JSON 
procedure POSTExchangeObject(conf, destinationId, objectType, objectId, dateStamp,textJSON) export
	var response;
	checkToken(conf);
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/objects"));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("Content-Type","application/json; charset=utf-8");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.access_token);
	request.Headers.Insert("destinationId",destinationId);
				  
	jsonString = Serialize(new structure("objectType,objectId,dateStamp,text",objectType,objectId,dateStamp,textJSON));
	try
		request.SetBodyFromString(jsonString,"UTF-8",ИспользованиеByteOrderMark.НеИспользовать);
		response = connection.Post(request);
		if response.StatusCode <> 200 then
			raise "Помилка POST /api/v0/objects  with StatusCode: "+ response.StatusCode;	
		endif;
	except
		error = "Помилка ідентифікації на сервері! Error:"+ОписаниеОшибки();
		sabatexLog.LogError(conf,"PostObject1C",error);
		raise error;
	endtry;	
endprocedure	

#endregion

#region quries
// get queried objects
function GetQueriedObjects(conf) export
	var response;
	checkToken(conf);
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("/api/v0/queries",new structure("take",conf.take)));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",string(conf.clientId));
	request.Headers.Insert("apiToken",conf.access_token);
	try
		response = connection.Get(request);
		if response.StatusCode <> 200 then
			raise "Помилка запиту /api/v0/queries " + response.StatusCode;		
		endif;
	except  
		raise "Error GetQueriedObjects: "+ ОписаниеОшибки();	
	endtry;
	return Deserialize(response.GetBodyAsString());
endfunction	
procedure DeleteQueriesObject(conf,id) export
	checkToken(conf);
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("/api/v0/queries/"+id));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.access_token);
 	response = connection.Delete(request);
	if response.StatusCode <> 200 then
		raise "Помилка запиту /api/v0/queries with id=" +id+ " with StatusCode: "+ response.StatusCode;	
	endif;
endprocedure	
// реєструє запит на сервері та повертає ід запита
function PostQueries(conf,destinationId,ObjectId,ObjectType) export
	checkToken(conf);
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/queries"));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("Content-Type","application/json; charset=utf-8");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.access_token);
	request.Headers.Insert("destinationId",destinationId);
				  
	jsonString = Serialize(new structure("objectType,objectId",objectType,objectId));
	try
		request.SetBodyFromString(jsonString,"UTF-8",ИспользованиеByteOrderMark.НеИспользовать);
		response = connection.Post(request);
		if response.StatusCode <> 200 then
			raise "Помилка POST /api/v0/queries  with StatusCode: "+ response.StatusCode;	
		endif;
	except
		error = "Помилка ідентифікації на сервері! Error:"+ОписаниеОшибки();
		sabatexLog.LogError(conf,"PostObject1C",error);
		raise error;
	endtry;	

endfunction

#endregion

#endregion


#region internalcashe
// objectExchange
// type -  Map
// model - https://github.com/sabatex/WebApiDocumentsExcange/blob/master/WebApi1C8Exchange.Models/ObjectExchange.cs
// 
// objectRef
// type - Catalog,Document
procedure AddObjectToCashe(objectExchange,objectRef) export
	result = РегистрыСведений.sabatex_ExternalObjects.СоздатьМенеджерЗаписи();
	result.destination = objectExchange["destination"];
	result.sender = objectExchange["sender"];
	result.DateStamp = objectExchange["dateStamp"];
	result.ObjectId = objectExchange["objectId"];
	result.ObjectType = lower(objectExchange["objectType"]);
	result.ThisObject = objectRef;
	result.Записать(true);
endprocedure
// sender - node name sender
// objectType - sender object
// objectId 
function getObjectFromCashe(sender,objectType,ObjectId) export
	filter = new structure;
	filter.Insert("ObjectId",objectId);
	filter.Insert("Node",sender);
	filter.Insert("ObjectType",lower(objectType));
	result = РегистрыСведений.sabatex_ExternalObjects.Получить(filter);
	if result.ThisObject = Неопределено then
		return null;
	else
		return result.ThisObject;
	endif;
endfunction
function getObjectFromCasheByExchangeObject(conf,exchangeObject) export
	return getObjectFromCashe(exchangeObject["sender"],exchangeObject["objectType"],exchangeObject["objectId"]);
endfunction	
#endregion


#region GetObjects
function checkComplexType(conf,complexValue,objectId,objectType)
	try
		objectId = complexValue.Get("#value");
		complexType = complexValue.Get("#type");
		pos = Найти(complexType,".");
		subType = Сред(complexType,1,pos);
		typeName = Сред(complexType,pos+1);
		if subType = "jcfg:CatalogRef." then
			objectType = GetLocalObjectName(conf,"справочник."+lower(typeName));
			return true;
		elsif subType = "jcfg:DocumentRef." then
			objectType = GetLocalObjectName(conf,"документ."+lower(typeName));
			return  true;
		else
			sabatexLog.LogError(conf,"sabatexExchange.GetCatalog","Невідомий тип " + complexType);
			return false;
		endif;
	except
		return false;
	endtry;
endfunction	

function GetObjectManager(conf,objectType)
	pos = Найти(objectType,".");
	if pos = -1 then
		sabatexLog.LogError(conf,"getObjectManager","Задано неправильний тип objectType=" + objectType);
		return undefined;
	endif;	
	subType = Сред(objectType,1,pos-1);
	typeName = Сред(objectType,pos+1);
	
	if lower(subType) = "справочник" then
		return Catalogs[typeName];
	elsif lower(subType) = "документ" then
		return Documents[typeName];;
	else
		sabatexLog.LogError(conf,"getObjectManager","Задано неправильний тип objectType=" + objectType);
		return undefined;
	endif;
endfunction
procedure AddToMapDifferObjects(conf,localName,destinationName) export
	if not conf.Property("MapDifferObjects") then
		conf.Insert("MapDifferObjects",new structure);
		conf.MapDifferObjects.Insert("Forward",new map);
		conf.MapDifferObjects.Insert("Backward",new map);
	endif;	
	conf.MapDifferObjects.Forward.Insert(lower(localName),lower(destinationName));
	conf.MapDifferObjects.Backward.Insert(lower(destinationName),lower(localName));
endprocedure
function GetDestinationObjectName(conf,localObjectName)
	result = conf.MapDifferObjects.Forward[lower(localObjectName)];
	if result = undefined then
		return localObjectName;
	else
		return result;
	endif;
endfunction
function GetLocalObjectName(conf,destinationObjectName)
	result = conf.MapDifferObjects.Backward[lower(destinationObjectName)];
	if result = undefined then
		return destinationObjectName;
	else
		return result;
	endif;
endfunction
// Get object by id
// conf  - config struct (include sender node
// objectId - string 
// objectType - empty for complex type, string справочник.номнклатура,...
// success - bool (set false if error)
function GetObjectRef(conf,знач objectId,знач objectType, success) export
	// complex type
	if typeof(objectId) = type("Map") then
		complexObjectId = "";
		complexObjectType = "";
		if checkComplexType(conf,objectId,complexObjectId,complexObjectType) then
			return GetObjectRef(conf,complexObjectId,complexObjectType,success);
		endif;
		
		objectManager = GetObjectManager(conf,objectType);
		if objectManager <> undefined then
			return objectManager.EmptyRef();
		endif;
		success = false;
		return undefined;
	endif;	
	
	objectManager = GetObjectManager(conf,objectType);
	if objectManager = undefined then
		return undefined;
	endif;
	
	if ObjectId = "00000000-0000-0000-0000-000000000000" then
		return objectManager.EmptyRef();	
	endif;
	
	result = objectManager.GetRef(new UUID(objectId));

	if result.GetObject() = undefined then
		destinationFullName = GetDestinationObjectName(conf, objectType);
		if conf.Property("useObjectCashe") then
			if conf.useObjectCashe then
				result = getObjectFromCashe(conf.sender,destinationFullName,ObjectId);
				if result<>null then
					return result;
				endif;	
			endif;	
		endif;	
		
		success = false;
		AddQueryForExchange(conf,destinationFullName,objectId);
		return objectManager.EmptyRef();
	endif;
	return result;
endfunction	
#endregion


#region queriedObject
function GetDocumentById(conf,objectName,objectId)
	objRef = Documents[objectName].GetRef(new UUID(objectId));
	if objRef.GetObject() = undefined then
		sabatexLog.LogError(conf,"sabatexExchange.ParseQueriedObject","Помилка отримання обєкта документа "+objectName + " з ID="+objectId);
		return undefined;
	endif;
	return objRef;
endfunction
function GetCatalogById(conf,objectName,objectId)
	objRef = Catalogs[objectName].GetRef(new UUID(objectId));
	if objRef.GetObject() = undefined then
		sabatexLog.LogError(conf,"sabatexExchange.ParseQueriedObject","Помилка отримання обєкта Довідника "+objectName + " з ID="+objectId);
		return undefined;
	endif;
	return objRef;
endfunction
function GetQueryObject(conf,objectType,objectId)
	pos = Найти(objectType,".");
	if pos = -1 then
		sabatexLog.LogError(conf,"getObjectManager","Задано неправильний тип objectType=" + objectType);
		return undefined;
	endif;	
	subType = Сред(objectType,1,pos-1);
	typeName = Сред(objectType,pos+1);
	
	if lower(subType) = "справочник" then
		return GetCatalogById(conf,typeName,objectId);
	elsif lower(subType) = "документ" then
		return GetDocumentById(conf,typeName,objectId);
	elsif lower(subType) = "query" then
		result = undefined;
		Execute(conf.QueryParser + "(conf,typeName,objectId,result)");
		return result;
	else 
		return undefined;
	endif;
endfunction	
function ParseQueriedObject(conf,queryObject) export
	objectType = lower(queryObject["objectType"]);
	objectId = queryObject["objectId"];
	
	if objectId = "00000000-0000-0000-0000-000000000000" then
		return true; // miss empty query  
	endif;	
	
	result = GetQueryObject(conf,objectType,objectId);

	if result <> undefined then
		sabatexExchange.RegisterObjectForNode(result,conf.sender,objectType);
		return true;
	endif;	
	
	return false;
endfunction
procedure DoQueriedObjects(conf)
		queries = GetQueriedObjects(conf);
		for each item in queries do
			НачатьТранзакцию();
			try
				if ParseQueriedObject(conf,item) then
					DeleteQueriesObject(conf,item["Id"]);
				endif;
				ЗафиксироватьТранзакцию();
			except
				ОтменитьТранзакцию();
			endtry;	
		enddo;	
endprocedure	
procedure defaultQueryParser(conf,queryName,queryParams,result)
	sabatexLog.LogError(conf,"sabatexExchange.defaultQueryParser","Не задано QueryParser! ");
	result = undefined; 
endprocedure	
// Add query for exchange
// params:
// 		conf 		Configuration structure
//      objectType  Queried object type (50) as (Справочник.Контрагенты)
//      objectId    destination objectId or object code
procedure AddQueryForExchange(conf,objectType,objectId) export
	Запрос = Новый Запрос;
	Запрос.Текст = 
		"SELECT TOP 1
		|	sabatexExchangeUnresolvedObjects.objectId AS objectId
		|FROM
		|	InformationRegister.sabatexExchangeUnresolvedObjects AS sabatexExchangeUnresolvedObjects
		|WHERE
		|	sabatexExchangeUnresolvedObjects.objectId = &objectId
		|	AND sabatexExchangeUnresolvedObjects.sender = &sender
		|	AND sabatexExchangeUnresolvedObjects.objectType = &objectType";
	
	Запрос.УстановитьПараметр("objectId", objectId);
	Запрос.УстановитьПараметр("objectType", objectType);
	Запрос.УстановитьПараметр("sender", new UUID(conf.destinationId));
	
	РезультатЗапроса = Запрос.Выполнить();
	
	ВыборкаДетальныеЗаписи = РезультатЗапроса.Выбрать();
	
	Пока ВыборкаДетальныеЗаписи.Следующий() Цикл
		// miss unresolved object
		return;
	КонецЦикла;
	
	query = conf.queryList.Add();
	query.nodeName = conf.destinationId;
	query.objectType = objectType;
	query.objectId = objectId;
	sabatexLog.LogInformation(conf,"AddQueryForExchange","Відправлено запит на отримання "+ objectType + " з Id=" + objectId);
endprocedure	

#endregion



#region ExchangeObjects
// Register object in cashe for send to destination
// params:
// 	obj           - object (Catalog or Documrnt)
//  destinationId - UUID destination clientId  
procedure RegisterObjectForNode(obj,destinationId) export
	reg = InformationRegisters.sabatexExchangeObject.CreateRecordManager();
	reg.Id = new UUID();
	reg.destinationId  = destinationId;
	reg.dateStamp = CurrentDate();
	ref	= obj.Ref;
	reg.objectType = Upper(ref.Metadata().FullName());
	reg.objectId = ref.UUID();
	reg.objectJSON =Serialize(ref.GetObject());
	reg.Write(true);
endprocedure	
// Delete object from cashe
// params:
//	destinationId - 
//  dateStamp     - 
procedure DeleteObjectForExchange(Id)
	reg = InformationRegisters.sabatexExchangeObject.CreateRecordManager();
	reg.Id = Id;
	reg.Delete();
endprocedure	
function GetRegisteredObjects(conf)
	query = new Query; // "+ conf.take + "
	query.Text = 
		"SELECT TOP "+ conf.take + "
		|	sabatexExchangeObject.objectId AS objectId,
		|	sabatexExchangeObject.dateStamp AS dateStamp,
		|	sabatexExchangeObject.destinationId AS destinationId,
		|	sabatexExchangeObject.objectType AS objectType,
		|	sabatexExchangeObject.objectJSON AS objectJSON
		|FROM
		|	InformationRegister.sabatexExchangeObject AS sabatexExchangeObject
		|
		|ORDER BY
		|	dateStamp";
	
	return query.Execute().Select();
endfunction	
// завантаження обєктів в систему
// conf - конфігурація
procedure ReciveObjects(conf)
		// read incoming objects 
		incoming = GetObjectsExchange(conf);
		for each item in incoming do
			objectId = "";
			objectType = "";
			BeginTransaction();
			try
				AddUnresolvedObject(conf,item);
				DeleteExchangeObject(conf,item["id"]);
				CommitTransaction();
			except
				sabatexLog.LogError(conf,"Recive Object","Do not load objectId=" + objectId + ";objectType="+ objectType + " Error Message: " + ОписаниеОшибки());
				RollbackTransaction();
			endtry;
		enddo;
endprocedure

function ConvertQueriesToTable(queryList)
	table = new ValueTable;
	table.Columns.Add("nodeName");
	table.Columns.Add("objectType");
	table.Columns.Add("objectId");
	for each query in queryList do
		row = table.Add();
		row.nodeName = query.nodeName;
		row.objectType = query.objectType;
		row.objectId = query.objectId;
	enddo;
	table.GroupBy("nodeName,objectType,objectId");
endfunction


procedure PostObjects(conf)
	// post queries
	conf.queryList.GroupBy("nodeName,objectType,objectId");
	for each query in conf.queryList do 
		PostQueries(conf,query.nodeName,query.objectId,query.objectType);
	enddo;	
	
	Запрос = Новый Запрос;
	Запрос.Текст = 
		"SELECT TOP 200
		|	sabatexExchangeObject.Id AS Id,
		|	sabatexExchangeObject.objectId AS objectId,
		|	sabatexExchangeObject.objectType AS objectType,
		|	sabatexExchangeObject.objectJSON AS objectJSON,
		|	sabatexExchangeObject.dateStamp AS dateStamp,
		|	sabatexExchangeObject.destinationId AS destinationId
		|FROM
		|	InformationRegister.sabatexExchangeObject AS sabatexExchangeObject";
	
	РезультатЗапроса = Запрос.Выполнить();
	
	items = РезультатЗапроса.Выбрать();
	
	while items.Next() do
		try
			POSTExchangeObject(conf,items.destinationId,items.objectType,items.objectId,items.dateStamp,items.objectJSON);
			DeleteObjectForExchange(items.Id);
		except
			
		endtry;
	enddo;
endprocedure	
#endregion
#region AnalizeObjects
procedure defaultIncomingParser(conf,exchangeObject,success) export
	sabatexLog.LogError(conf,"sabatexExchange.defaultIncomingParser","Не задано IncomingParser!");
	success = false; 
endprocedure	
procedure AddUnresolvedObject(conf,item,newObject = true)
	reg = InformationRegisters.sabatexExchangeUnresolvedObjects.CreateRecordManager();
	reg.Id = item["id"];
	reg.sender = new UUID(item["sender"]);
	reg.destination = new UUID(item["destination"]);
	reg.objectId = Upper(item["objectId"]);
	reg.objectType = Upper(item["objectType"]);
	reg.dateStamp = CurrentDate();
	reg.serverDateStamp = item["dateStamp"];
	reg.objectAsText = item["objectAsText"];
	reg.Log = ?(newObject,"",conf.Log);
	reg.Write();
endprocedure
procedure LevelUpUnresolvedObject(conf,item)
	reg = InformationRegisters.sabatexExchangeUnresolvedObjects.CreateRecordManager();
	reg.sender = item.sender;
	reg.Id = item.id;
	reg.Read();
	reg.levelLive = reg.levelLive +1;
	reg.Log = conf.Log;
	reg.Write();
endprocedure
procedure DeleteUnresolvedObject(conf,item)
	reg = InformationRegisters.sabatexExchangeUnresolvedObjects.CreateRecordManager();;
	reg.sender = item.sender;
	reg.Id = item.Id;
	reg.dateStamp = item.dateStamp;
	reg.Delete();
endprocedure	
procedure AnalizeUnresolvedObjects(conf)
	try
		Query = New Query;
		Query.Text = 
		"SELECT TOP 200
		|	sabatexExchangeUnresolvedObjects.sender AS sender,
		|	sabatexExchangeUnresolvedObjects.destination AS destination,
		|	sabatexExchangeUnresolvedObjects.dateStamp AS dateStamp,
		|	sabatexExchangeUnresolvedObjects.objectId AS objectId,
		|	sabatexExchangeUnresolvedObjects.objectType AS objectType,
		|	sabatexExchangeUnresolvedObjects.objectAsText AS objectAsText,
		|	sabatexExchangeUnresolvedObjects.Log AS Log,
		|	sabatexExchangeUnresolvedObjects.senderDateStamp AS senderDateStamp,
		|	sabatexExchangeUnresolvedObjects.serverDateStamp AS serverDateStamp,
		|	sabatexExchangeUnresolvedObjects.Id AS Id,
		|	sabatexExchangeUnresolvedObjects.levelLive AS levelLive
		|FROM
		|	InformationRegister.sabatexExchangeUnresolvedObjects AS sabatexExchangeUnresolvedObjects
		|WHERE
		|	sabatexExchangeUnresolvedObjects.sender = &sender
		|
		|ORDER BY
		|	levelLive";
		
		Query.SetParameter("sender",new UUID(conf.destinationId));
		QueryResult = Query.Execute();
		
		SelectionDetailRecords = QueryResult.Select();
		
		While SelectionDetailRecords.Next() Do
			objectId = "";
			objectType = "";
			
			НачатьТранзакцию();
			try
				objectId = SelectionDetailRecords["objectId"];
				objectType = SelectionDetailRecords["objectType"];
				
				success = true;
				conf.Log = "";
				Execute(conf.IncomingParser+"(conf,SelectionDetailRecords,success)");
				
				if success then
					DeleteUnresolvedObject(conf,SelectionDetailRecords);
				else
					LevelUpUnresolvedObject(conf,SelectionDetailRecords);
				endif;
				
			except
				sabatexLog.LogError(conf,"Recive Object","Do not load objectId=" + objectId + ";objectType="+ objectType + " Error Message: " + ОписаниеОшибки());
				ОтменитьТранзакцию();
				continue;
			endtry;
			ЗафиксироватьТранзакцию();
		enddo;
	except
		sabatexLog.Logged(conf,0,"ExchangeProcess","Загальна помилка AnalizeUnresolvedObjects для клієнта -"+ string(conf.clientId) + " - " + ОписаниеОшибки());
	endtry;
endprocedure
#endregion

// parse incoming object and return 
// true - object is Ok
// false - missed object
// unresolved - array structure queried objects
procedure IncomingDefaultParser(conf,obj,success)
endprocedure	

// Розпочати процесс обміну
// params:
// 		nodeName - назва нода обміну в регістрі sabatexNodeConfig
procedure ExchangeProcess(nodeName,incomingParser="sabatexExchange.IncomingDefaultParser",queryParser="sabatexExchange.defaultQueryParser") export
	try
		start = CurrentDate();
		conf = sabatexExchangeConfig.GetConfig(nodeName,incomingParser,queryParser);

		// ansver the query and set to queue
		DoQueriedObjects(conf);
		
		// read  input objects
		ReciveObjects(conf);
		
		AnalizeUnresolvedObjects(conf);
		
		PostObjects(conf);
		
		end = ТекущаяДата();
		log = "The exchange with "+conf.destinationId + " process result:" + Chars.LF;
		log = log + "Import objects - " + conf.ImportObjects + Chars.LF;
		log = log + "Missed objects - " + conf.MissObjects + Chars.LF;
		log = log + "Export objects - " +conf.ExportObjects  +Chars.LF;
		log = log+ "Duration process = " + string(end - start)+ " сек.";
	    sabatexLog.Logged(conf,3,"ExchangeProcess",log); 
	
	except
		sabatexLog.Logged(conf,0,"ExchangeProcess",string(conf.clientId) + " - " + ОписаниеОшибки());
	endtry;
endprocedure

