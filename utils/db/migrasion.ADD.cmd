setlocal

SET /p "name=Migration name: "

SET ASPNETCORE_ENVIRONMENT=Development

CMD /k dotnet ef migrations add %name% ^
  -o Persistance\Migrations ^
  -c VocabularyDbContext ^
  -s ..\..\src\Vocabulary.BlazorServer\Vocabulary.BlazorServer.csproj ^
  -p ..\..\src\Vocabulary.Adapters\Vocabulary.Adapters.csproj

endlocal

PAUSE