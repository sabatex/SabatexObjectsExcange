
&НаСервереБезКонтекста
Процедура RunНаСервере()
	//sabatexExchangeUNF.TimerSabatexExchange();
КонецПроцедуры

&НаКлиенте
Процедура Run(Команда)
	RunНаСервере();
КонецПроцедуры

&AtServer
function GetDemoSender()
	result = new Structure;
	result.Insert("host","sabatex.francecentral.cloudapp.azure.com");
	result.Insert("https",true);
	result.insert("clientId",new UUID("432CC3F5-4227-494C-99AA-01034ACC78E6"));
	result.Insert("password","ttc$^%^bhs34");
	
	return result;
endfunction	

&AtServer
function GetDemoDestination()
	result = new Structure;
	result.Insert("host","sabatex.francecentral.cloudapp.azure.com");
	result.Insert("https",true);
	result.insert("clientId",new UUID("BA949E80-7D80-4E29-B05D-C33DEA49F0C2"));
	result.Insert("password","ysew4w7y9843&^%");
	
	return result;
endfunction	

&AtServer
function GetDemoCatalog()
	Query = New Query;
	Query.Text = 
		"SELECT TOP 1
		|	Контрагенты.Ref AS Ref
		|FROM
		|	Catalog.Контрагенты AS Контрагенты";
	
	QueryResult = Query.Execute();
	
	SelectionDetailRecords = QueryResult.Select();
	
	While SelectionDetailRecords.Next() Do
		return   SelectionDetailRecords.Ref;
	EndDo;
endfunction	


&AtServer
procedure TestApiAtServer()
	sender = sabatexExchange.GetConfig(GetDemoSender());
	destination = sabatexExchange.GetConfig(GetDemoDestination());
	
	dc = GetDemoCatalog();
	sabatexExchange.RegisterObjectForNode(dc.GetObject(),destination.clientId);
	sabatexExchange.RegisterObjectForNode(dc.GetObject(),destination.clientId);
	sabatexExchange.RegisterObjectForNode(dc.GetObject(),destination.clientId);
	
	sabatexExchange.ExchangeProcess(sender);
    sabatexExchange.ExchangeProcess(destination);
	//sabatexExchange.Login(sender);
	//sabatexExchange.Login(destination);

endprocedure	

&AtClient
Procedure TestApi(Command)
	TestApiAtServer();	
EndProcedure
