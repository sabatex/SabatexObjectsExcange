// sabatex Copyright (c) 2021 by Serhiy Lakas
// https://github.com/sabatex-1C
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

