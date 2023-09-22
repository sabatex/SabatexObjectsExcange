// Copyright (c) 2021 by Serhiy Lakas
// https://sabatex.github.io
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
function GetConfig(nodeName,incomingParser="sabatexExchange.IncomingDefaultParser",queryParser="sabatexExchange.defaultQueryParser") export
	if TypeOf(nodeName) <> Type("string") then
		raise "Параметр nodeName повинен бути типу string!";
	endif;
	
	if TypeOf(incomingParser) <> Type("string") then
		raise "Параметр incomingParser повинен бути типу string!";
	endif;
	
	filter = new structure;
	filter.Insert("NodeName",nodeName);
	config = InformationRegisters.sabatexNodeConfig.Get(filter);
	
	if config.Host = "" then
		raise "В регістрі відомостей sabatexNodeConfig не знайдено запису" + nodeName;	
	endif;	
		
	if not config.Property("destinationId") then
		raise "The config must be initialize DestinationId!";
	endif;
	
	config.Insert("NodeName",nodeName);
	config.Insert("IncomingParser",incomingParser);
	config.Insert("QueryParser",queryParser);
	
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
	
	
	config.Insert("ExportObjects",0);
	config.Insert("ImportObjects",0);
	config.Insert("MissObjects",0);
	config.Insert("QueriedObjects",0);
	config.Insert("Log","");
	
	table = new ValueTable;
	table.Columns.Add("nodeName");
	table.Columns.Add("objectType");
	table.Columns.Add("objectId");

	config.Insert("queryList",table);
	return config;
endfunction	
procedure SetAccessToken(conf,token) export
	reg = InformationRegisters.sabatexNodeConfig.CreateRecordManager();
	reg.NodeName = conf.NodeName;
	reg.Read();
	reg.access_token = token["access_token"];
	conf.Insert("access_token",token["access_token"]);
	reg.refresh_token = token["refresh_token"];
	conf.Insert("refresh_token",token["refresh_token"]);
	reg.token_type = token["token_type"];
	expires_in = CurrentDate()+token["expires_in"];
	conf.Insert("expires_in",expires_in);
	reg.expires_in = expires_in;
    reg.Write();
endprocedure
