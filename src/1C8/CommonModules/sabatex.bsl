#region sabatex
// Copyright (c) 2021 by Serhiy Lakas
// https://sabatex.github.io
// version 1.0.0


Function DateAddDay(value, count = 1) Export
	Return value + count * 86400;
EndFunction

Function DateAddHour(value, count = 1) Export
	Return value + count * 3600;
EndFunction

Function DateAddMinute(value, count = 1) Export
	Return value + count * 60;
EndFunction

Function StringStartWith(value, searchString) Export
	Return StrFind(value, searchString) = 1;
EndFunction

Function StringSplit(value, delimiter = ";", includeEmpty = true) Export
	
	If StrLen(delimiter) <> 1 Then
		Raise "Роздільник має бути тільки 1 символ.";
	EndIf;
	
	result = New array;
	position = 1;
	success = true;
	While success Do
		nextPosition = StrFind(value, delimiter, position);
		If nextPosition = 0 Then
			success = false;
			Continue;
		EndIf;
		count = nextPosition - position;
		If count = 0 Then
			If includeEmpty Then
				result.Add("");
			EndIf;
			position = position + 1;
			Continue;
		EndIf;
		result.Add(Mid(value, position, count));
		position = position + count + 1;
	EndDo;
	Return result;
EndFunction


#endregion