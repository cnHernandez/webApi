#!/bin/bash
PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
echo "Ejecutando a $(date) por usuario $(whoami)" >> /home/ubuntu/procesar_kilometrajes.log
env >> /home/ubuntu/procesar_kilometrajes.log
# Script para ejecutar el procesamiento de archivos CSV de kilometraje

docker exec web-api-api-1 dotnet ApiSwagger.dll --process-csvs >> /home/ubuntu/procesar_kilometrajes.log 2>&1
if [ $? -ne 0 ]; then
  echo "Error ejecutando docker exec a $(date)" >> /home/ubuntu/procesar_kilometrajes.log
fi
