setlocal

SET ASPNETCORE_ENVIRONMENT=Development

CMD /k dotnet ef migrations list ^
  -c VocabularyDbContext ^
  -s ..\..\src\Vocabulary.BlazorServer\Vocabulary.BlazorServer.csproj ^
  -p ..\..\src\Vocabulary.Adapters\Vocabulary.Adapters.csproj

SET /p "name=Migration name: "

CMD /k dotnet ef database update %name% ^
  -c VocabularyDbContext ^
  -s ..\..\src\Vocabulary.BlazorServer\Vocabulary.BlazorServer.csproj ^
  -p ..\..\src\Vocabulary.Adapters\Vocabulary.Adapters.csproj

endlocal

PAUSE