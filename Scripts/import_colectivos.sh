#!/bin/bash

# Configuración
URL='http://localhost:5058/api/importdata/import'
API_KEY='QnVydmVsYUFwaVNlY3VyaXR5IzEyMg='
ARCHIVO="$(dirname "$0")/dataseed/colectivos_data.xlsx"
MODELO='Colectivo'

# Verificar existencia del archivo
if [ ! -f "$ARCHIVO" ]; then
    echo "\033[1;31m❌ Archivo no encontrado: $ARCHIVO\033[0m"
    exit 1
fi

# Mostrar información de la operación
echo "\033[1;34m📤 Importando datos del modelo: $MODELO\033[0m"
echo "Archivo: $ARCHIVO"
echo "URL: $URL"

# Realizar la petición
response=$(curl -s -w "\n%{http_code}" --location "$URL/$MODELO" \
    --header "x-api-key: $API_KEY" \
    --form "file=@$ARCHIVO" 2>&1)

# Verificar si hubo error en curl
curl_exit_code=$?
if [ $curl_exit_code -ne 0 ]; then
    echo "\033[1;31m❌ Error al conectar con el servidor\033[0m"
    echo "$response"
    exit 1
fi

# Procesar respuesta
status_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

# Mostrar resultados
echo "\033[1;32m✅ Respuesta del servidor\033[0m"
echo "Código de estado HTTP: $status_code"

if [ "$status_code" -ge 200 ] && [ "$status_code" -lt 300 ]; then
    echo "\033[1;32m✔ Importación completada con éxito\033[0m"
else
    echo "\033[1;31m✖ Hubo un problema con la importación\033[0m"
fi

echo "Contenido de respuesta:"
echo "$body"