setlocal

docker-compose -f docker-compose.yml down -v    vocabulary
docker-compose -f docker-compose.yml up --force-recreate --build -d    vocabulary

endlocal

PAUSE
exit /b 0
