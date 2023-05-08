// sabatex Copyright (c) 2021 by Serhiy Lakas
// https://github.com/sabatex-1C
// version 1.2.0 (28.09.2022)
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
			conf.Log.Add(message);
		endif;
	except
		
	endtry;
endprocedure
procedure LogError(conf,sourceName,message) export
	Logged(conf,0,sourceName,message);		
endprocedure	
procedure LogWarning(conf,sourceName,message) export
	Logged(conf,1,sourceName,message);		
endprocedure
procedure LogInformation(conf,sourceName,message) export
	Logged(conf,2,sourceName,message);		
endprocedure
procedure LogNote(conf,sourceName,message) export
	Logged(conf,3,sourceName,message);		
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

#region ExchangeWebApi
procedure Login(conf) export
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
		conf.Insert("apiToken",apiToken);
	except
		error = "Помилка ідентифікації на сервері! Error:"+ОписаниеОшибки();
		LogError(conf,"Login",error);
		raise error;
	endtry;	
endprocedure	

#region objects
// download objects from server bay sender nodeName
function GetObjectsExchange(conf) export
	var responce;
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/objects",new structure("take",conf.take)));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.apiToken);
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
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("/api/v0/objects/"+id));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.apiToken);
 	response = connection.Delete(request);
	if response.StatusCode <> 200 then
		raise "Помилка запиту /api/v0/objects with id=" +id+ " with StatusCode: "+ response.StatusCode;	
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
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("api/v0/objects"));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("Content-Type","application/json; charset=utf-8");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.apiToken);
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
		LogError(conf,"PostObject1C",error);
		raise error;
	endtry;	
endprocedure	

#endregion

#region quries
// get queried objects
function GetQueriedObjects(conf) export
	var response;
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("/api/v0/queries",new structure("take",conf.take)));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",string(conf.clientId));
	request.Headers.Insert("apiToken",conf.apiToken);
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
	connection = CreateHTTPSConnection(conf);
	request = new HTTPRequest(BuildUrl("/api/v0/queries/"+id));
	request.Headers.Insert("accept","*/*");
	request.Headers.Insert("clientId",conf.clientId);
	request.Headers.Insert("apiToken",conf.apiToken);
 	response = connection.Delete(request);
	if response.StatusCode <> 200 then
		raise "Помилка запиту /api/v0/queries with id=" +id+ " with StatusCode: "+ response.StatusCode;	
	endif;
endprocedure	
// реєструє запит на сервері та повертає ід запита
function PostQueries(conf,destinationNode,ObjectId,ObjectTypeName) export
	query = new structure;
	query.Insert("Destination",destinationNode);
	query.Insert("ObjectId",ObjectId);
	query.Insert("ObjectType",ObjectTypeName);
	response = POSTObject(conf,"/api/v0/queries",query);
	if response.StatusCode = 200 then
		return Число(response.GetBodyAsString());	
	endif;
	raise "Помилка Post /api/v0/queries destinationNode="+destinationNode
	       +" ObjectId="+ObjectId+" ObjectTypeName="+ObjectTypeName + " with StatusCode " + response.StatusCode;
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
			Logged(conf,0,"sabatexExchange.GetCatalog","Невідомий тип " + complexType);
			return false;
		endif;
	except
		return false;
	endtry;
endfunction	

function GetObjectManager(conf,objectType)
	pos = Найти(objectType,".");
	if pos = -1 then
		Logged(conf,0,"getObjectManager","Задано неправильний тип objectType=" + objectType);
		return undefined;
	endif;	
	subType = Сред(objectType,1,pos-1);
	typeName = Сред(objectType,pos+1);
	
	if lower(subType) = "справочник" then
		return Catalogs[typeName];
	elsif lower(subType) = "документ" then
		return Documents[typeName];;
	else
		Logged(conf,0,"getObjectManager","Задано неправильний тип objectType=" + objectType);
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
		Logged(conf,0,"sabatexExchange.ParseQueriedObject","Помилка отримання обєкта документа "+objectName + " з ID="+objectId);
		return undefined;
	endif;
	return objRef;
endfunction
function GetCatalogById(conf,objectName,objectId)
	objRef = Catalogs[objectName].GetRef(new UUID(objectId));
	if objRef.GetObject() = undefined then
		Logged(conf,0,"sabatexExchange.ParseQueriedObject","Помилка отримання обєкта Довідника "+objectName + " з ID="+objectId);
		return undefined;
	endif;
	return objRef;
endfunction
function GetQueryObject(conf,objectType,objectId)
	pos = Найти(objectType,".");
	if pos = -1 then
		Logged(conf,0,"getObjectManager","Задано неправильний тип objectType=" + objectType);
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
	Logged(conf,0,"sabatexExchange.defaultQueryParser","Не задано QueryParser! ");
	result = undefined; 
endprocedure	

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
	Запрос.УстановитьПараметр("sender", conf.sender);
	
	РезультатЗапроса = Запрос.Выполнить();
	
	ВыборкаДетальныеЗаписи = РезультатЗапроса.Выбрать();
	
	Пока ВыборкаДетальныеЗаписи.Следующий() Цикл
		// miss unresolved object
		return;
	КонецЦикла;
	
	query = new structure;
	query.Insert("nodeName",conf.sender);
	query.Insert("objectType",objectType);
	query.Insert("objectId",objectId);
	conf.queryList.Add(query);
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
				Logged(conf,0,"Recive Object","Do not load objectId=" + objectId + ";objectType="+ objectType + " Error Message: " + ОписаниеОшибки());
				RollbackTransaction();
			endtry;
		enddo;
endprocedure
procedure PostObjects(conf)
	// post queries
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
	Logged(conf,0,"sabatexExchange.defaultIncomingParser","Не задано IncomingParser!");
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
		|	sabatexExchangeUnresolvedObjects.Id AS Id
		|FROM
		|	InformationRegister.sabatexExchangeUnresolvedObjects AS sabatexExchangeUnresolvedObjects
		|
		|ORDER BY
		|	dateStamp";
	
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
				Logged(conf,0,"Recive Object","Do not load objectId=" + objectId + ";objectType="+ objectType + " Error Message: " + ОписаниеОшибки());
				ОтменитьТранзакцию();
				continue;
		endtry;
		ЗафиксироватьТранзакцию();
	enddo;
endprocedure
#endregion
#region Config
procedure SetSenderValueIfDestinationEmpty(valueName,sender,destination)
	var propValue;
	if sender.Property(valueName,propValue) then
		if not destination.Property(valueName) then
			destination.Insert(valueName,sender[ValueName]);
		endif;	
	endif	
endprocedure
procedure FillStructFromSenderIsEmpty(sender,destination)
	for each item in sender do
		SetSenderValueIfDestinationEmpty(string(item.key),sender,destination);
	enddo;	
endprocedure	
// return config struct and check
function GetConfig(config = null) export
	if config = null then
		config = new structure;
	endif;
	
	try
		filter = new structure;
		filter.Insert("Id",1);
		lConfig = РегистрыСведений.sabatexNodeConfig.Получить(filter);
		if lConfig.Host <> "" then
			FillStructFromSenderIsEmpty(lConfig,config);
		endif;	
	except
	endtry;	
	
	if not config.Property("https") then
		config.Insert("https",false);
	endif;
	
	if not config.Property("Host") then
		config.Insert("Host","sabatex.francecentral.cloudapp.azure.com");	
	endif;	
	
	if not config.Property("Port") then
		if config.https then
			config.Insert("Port",443);
		else
			config.Insert("Port",80);
		endif;	
	endif;	

	if not config.Property("NodeName") then
		config.Insert("NodeName","1C8");	
	endif;
	
	if not config.Property("password") then
		config.Insert("password","1");	
	endif;
	
	if not config.Property("Take") then
		config.Insert("Take",50);	
	endif;	
	
	if not config.Property("LogLevel") then
		config.Insert("LogLevel",4);	
	endif;	
	
	if not config.Property("useObjectCashe") then
		config.Insert("useObjectCashe",false);	
	endif;	
	
	if not config.Property("MapDifferObjects") then
		config.Insert("MapDifferObjects",new structure);
		config.MapDifferObjects.Insert("Forward",new map);
		config.MapDifferObjects.Insert("Backward",new map);
	endif;	
	
	if not config.Property("IncomingParser") then
		config.Insert("IncomingParser","sabatexExchange.defaultIncomingParser");
	endif;

	
	if not config.Property("QueryParser") then
		config.Insert("QueryParser","sabatexExchange.defaultQueryParser");
	endif;
	return config;
endfunction	
#endregion

// Start exchange process
// conf - config for exchange (struct)
// node - destination node name
// analizer - procedure name  for analize incoming objects
procedure ExchangeProcess(conf) export
	start = ТекущаяДата();
	//conf = GetConfig(conf);
	conf.Insert("ImportObjects",0);
	conf.Insert("ExportObjects",0);
	conf.Insert("MissObjects",0);
	conf.Insert("QueriedObjects",0);
    conf.Insert("queryList",new array);
	conf.Insert("Log","");
	

	try
	
		// login to server
		Login(conf);
		
		// ansver the query and set to queue
		DoQueriedObjects(conf);
		
		// read and analize input objects
		ReciveObjects(conf);
		
		AnalizeUnresolvedObjects(conf);
		
		PostObjects(conf);
		
		end = ТекущаяДата();
		log = "The exchange with "+conf.sender + " process result:" + Chars.LF;
		log = log + "Import objects - " + conf.ImportObjects + Chars.LF;
		log = log + "Missed objects - " + conf.MissObjects + Chars.LF;
		log = log + "Export objects - " +conf.ExportObjects  +Chars.LF;
		log = log+ "Duration process = " + string(end - start)+ " сек.";
	    sabatexExchange.Logged(conf,3,"ExchangeProcess",log); 
	
	except
		Logged(conf,0,"ExchangeProcess",string(conf.clientId) + " - " + ОписаниеОшибки());
	endtry;
endprocedure

