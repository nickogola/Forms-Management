dotnet publish Forms/Forms.csproj -c Release

IF EXIST \\BTSDETSTCSMWEB1\c$\inetpub\FormsCore\app_offline-template.htm ren \\BTSDETSTCSMWEB1\c$\inetpub\FormsCore\app_offline-template.htm app_offline.htm
timeout 10

xcopy /S /D /Y Forms\bin\Release\net462\publish \\BTSDETSTCSMWEB1\c$\inetpub\FormsCore

if exist \\BTSDETSTCSMWEB1\c$\inetpub\FormsCore\app_offline.htm del \\BTSDETSTCSMWEB1\c$\inetpub\FormsCore\app_offline.htm

pause