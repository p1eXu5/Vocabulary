setlocal

SET ASPNETCORE_ENVIRONMENT=Development

CMD /k dotnet ef migrations remove ^
  -c VocabularyDbContext ^
  -s ..\..\src\Vocabulary.BlazorServer\Vocabulary.BlazorServer.csproj ^
  -p ..\..\src\Vocabulary.Adapters\Vocabulary.Adapters.csproj

endlocal

PAUSE