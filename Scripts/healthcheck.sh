#!/bin/bash

# Cargar variables de entorno desde el archivo .env
if [ -f .env ]; then
    export $(grep -v '^#' .env | xargs)
else
    echo "\033[1;31m❌ Archivo .env no encontrado.\033[0m"
    exit 1
fi

# Configuración
API_URL='http://localhost:5058/api/health'


# Función para formatear JSON
format_json() {
    local json="$1"
    echo "$json"
}

# Mostrar información
echo  "\n\033[1;34m🩺 Verificando salud de la API\033[0m"
echo  "  URL: \033[1;36m$API_URL\033[0m"
echo  "  Timestamp: \033[1;36m$(date +"%Y-%m-%d %H:%M:%S")\033[0m"

# Realizar la petición
echo  "\n\033[1;33m🔄 Consultando estado del servicio...\033[0m"
response=$(curl -s -w "\n%{http_code}" --location "$API_URL" \
    --header "Content-Type: application/json" \
    --header "x-api-key: $API_KEY" 2>&1)

# Verificar errores de conexión
if [ $? -ne 0 ]; then
    echo  "\n\033[1;31m❌ Error al conectar con la API:\033[0m"
    echo "$response"
    exit 1
fi

# Procesar respuesta
status_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

# Mostrar resultados
echo  "\n\033[1;34m📊 Resultado de la verificación\033[0m"
echo  "┌────────────────────────────────────────────┐"
echo  "│ \033[1;36mEstado HTTP:\033[0m \033[1;33m$status_code\033[0m"
echo  "└────────────────────────────────────────────┘"

echo  "\n\033[1;36mEstado del servicio:\033[0m"
format_json "$body"

# Evaluar estado
if [ "$status_code" -eq 200 ]; then
    echo  "\n\033[1;32m✅ API funcionando correctamente\033[0m"
elif [ "$status_code" -eq 503 ]; then
    echo  "\n\033[1;31m❌ Servicio no disponible\033[0m"
    echo  "\033[1;36mDetalles:\033[0m"
    format_json "$body"
elif [ "$status_code" -ge 400 ]; then
    echo  "\n\033[1;31m⚠️  Problema con la solicitud\033[0m"
    echo  "\033[1;36mDetalles:\033[0m"
    format_json "$body"
else
    echo  "\n\033[1;33m⚠️  Estado inesperado\033[0m"
    echo  "\033[1;36mDetalles:\033[0m"
    format_json "$body"
fi
