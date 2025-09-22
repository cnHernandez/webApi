#!/bin/bash
# Script para ejecutar el procesamiento de archivos CSV de kilometraje

cd /home/ubuntu/web-api || exit 1

docker-compose run --rm api dotnet ApiSwagger.dll --process-csvs
